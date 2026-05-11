using NpgsqlTypes;
using Pgvector;

namespace MTGArchitect.RAG.Data.Models;

public class CardEmbedding
{
    public Guid ScryfallId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OracleText { get; set; } = string.Empty;
    public string TypeLine { get; set; } = string.Empty;
    public string[] Colors { get; set; } = [];
    public float ManaValue { get; set; }
    public Vector? Embedding { get; set; }
    public NpgsqlTsVector? SearchVector { get; set; }
}
