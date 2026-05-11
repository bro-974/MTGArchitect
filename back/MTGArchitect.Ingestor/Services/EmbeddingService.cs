using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Pgvector;

namespace MTGArchitect.Ingestor.Services;

public sealed class EmbeddingService(HttpClient http, string lmStudioUri)
{
    private const string Model = "nomic-embed-text";

    public async Task<Vector?> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var results = await GetEmbeddingsAsync([text], cancellationToken);
        return results.Count > 0 ? results[0] : null;
    }

    public async Task<List<Vector>> GetEmbeddingsAsync(IReadOnlyList<string> inputs, CancellationToken cancellationToken = default)
    {
        var request = new { model = Model, input = inputs };

        var response = await http.PostAsJsonAsync(
            $"{lmStudioUri}/embeddings",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken: cancellationToken);

        return result?.Data?
            .OrderBy(x => x.Index)
            .Select(x => new Vector(x.Embedding ?? []))
            .ToList() ?? [];
    }

    private sealed class EmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingData>? Data { get; set; }
    }

    private sealed class EmbeddingData
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }
    }
}
