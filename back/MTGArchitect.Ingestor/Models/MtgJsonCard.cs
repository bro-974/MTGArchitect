namespace MTGArchitect.Ingestor.Models;

public sealed class MtgJsonCard
{
    public string? ScryfallId { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }         // OracleText in MTGJson
    public string? Type { get; set; }         // TypeLine in MTGJson
    public List<string>? Colors { get; set; }
    public float? ManaValue { get; set; }
    public float? ConvertedManaCost { get; set; } // fallback for older formats
}
