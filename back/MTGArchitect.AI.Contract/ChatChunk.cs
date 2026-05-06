namespace MTGArchitect.AI.Contract;

public class ChatChunk(string content, ChunkType type)
{
    public string Content { get; } = content;
    public ChunkType Type { get; } = type;
}

public enum ChunkType { Reasoning, Answer, Metadata }
