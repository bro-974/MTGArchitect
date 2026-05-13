using Grpc.Core;
using MTGArchitect.RAG.Data.Repositories;
using MTGArchitect.RAG.Data.Services;
using OpenAI.Chat;
using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;

namespace MTGArchitect.AI.Service.Services;

public class MindServiceHandler(
    ChatClient chatClient,
    IConfiguration configuration,
    ICardSearchService cardSearchService,
    ICardEmbeddingRepository cardEmbeddingRepository) : MindService.MindServiceBase
{
    public override async Task Chat(
        ChatRequest request,
        IServerStreamWriter<ChatResponseStream> responseStream,
        ServerCallContext context)
    {
        var messages = new List<ChatMessage>();

        // 1. Main system prompt with format substitution
        var systemPromptTemplate = configuration["AiService:SystemPrompt"] ?? string.Empty;
        var format = string.IsNullOrEmpty(request.Format) ? "Custom" : request.Format;
        var systemPrompt = systemPromptTemplate.Replace("{{format}}", format);
        messages.Add(new SystemChatMessage(systemPrompt));

        // 2. Deck context — batch-fetch embeddings by ScryfallId for enriched card lines
        if (request.DeckCards.Count > 0)
        {
            var ids = request.DeckCards
                .Where(c => Guid.TryParse(c.ScryfallId, out _))
                .Select(c => Guid.Parse(c.ScryfallId))
                .ToList();

            var embeddings = await cardEmbeddingRepository.GetByIdsAsync(ids, context.CancellationToken);
            var embeddingMap = embeddings.ToDictionary(e => e.ScryfallId);

            var mainLines = new List<string>();
            var sideLines = new List<string>();

            foreach (var card in request.DeckCards)
            {
                string line;
                if (Guid.TryParse(card.ScryfallId, out var guid) && embeddingMap.TryGetValue(guid, out var emb))
                    line = $"{card.Quantity}x {emb.Name} ({emb.TypeLine}) {string.Join("", emb.Colors)} — CMC {emb.ManaValue} | {emb.OracleText}";
                else
                    line = $"{card.Quantity}x {card.Name}";

                if (card.IsSideBoard) sideLines.Add(line);
                else mainLines.Add(line);
            }

            var sb = new StringBuilder();
            sb.AppendJoin('\n', mainLines);
            if (sideLines.Count > 0)
            {
                sb.Append("\n---Sideboard---\n");
                sb.AppendJoin('\n', sideLines);
            }

            messages.Add(new SystemChatMessage($"The user is playing the following deck:\n{sb}"));
        }

        // 2.5. RAG: inject relevant cards found from the database
        var ragCards = await cardSearchService.GetSimilarCardsAsync(request.Prompt, limit: 5, context.CancellationToken);
        if (ragCards.Count > 0)
        {
            var cardContext = string.Join("\n\n", ragCards.Select(c =>
                $"**{c.Name}** ({c.TypeLine}) — {string.Join("", c.Colors)} — CMC {c.ManaValue}\nOracle: {c.OracleText}"));
            messages.Add(new SystemChatMessage(
                $"The following cards are suggestions found in the database based on your query, " +
                $"they are NOT necessarily in the user's current deck:\n{cardContext}"));
        }

        // 3. Conversation history
        foreach (var turn in request.History)
        {
            messages.Add(new UserChatMessage(turn.UserPrompt));
            messages.Add(new AssistantChatMessage(turn.Answer));
        }

        // 4. Current prompt
        messages.Add(new UserChatMessage(request.Prompt));

        await foreach (var update in chatClient.CompleteChatStreamingAsync(messages, cancellationToken: context.CancellationToken))
        {
            // Thinking tokens — DeepSeek-R1 returns them in reasoning_content (non-standard field)
            var reasoningChunk = ExtractReasoningContent(update);
            if (!string.IsNullOrEmpty(reasoningChunk))
            {
                await responseStream.WriteAsync(new ChatResponseStream
                {
                    ReasoningChunk = reasoningChunk
                });
            }

            // Standard answer tokens
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    await responseStream.WriteAsync(new ChatResponseStream
                    {
                        AnswerChunk = part.Text
                    });
                }
            }
        }
    }

    // reasoning_content is a DeepSeek-specific non-standard field — not exposed by the OpenAI SDK
    // We parse it from the raw JSON of each streaming update
    private static string? ExtractReasoningContent(StreamingChatCompletionUpdate update)
    {
        try
        {
            var raw = ModelReaderWriter.Write(update, ModelReaderWriterOptions.Json);
            using var doc = JsonDocument.Parse(raw.ToMemory());
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var delta = choices[0].GetProperty("delta");
                if (delta.TryGetProperty("reasoning_content", out var rc))
                    return rc.GetString();
            }
        }
        catch { /* non-fatal — skip reasoning on parse error */ }
        return null;
    }
}
