namespace MTGArchitect.Scryfall.Service.Models;

public record CardItem(
        string Id,
        string Name,
        string ManaCost,
        string TypeLine,
        string SetCode,
        string ImageUrl);

