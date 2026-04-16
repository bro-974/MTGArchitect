using Microsoft.AspNetCore.Identity;

namespace MTGArchitect.Data.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string? Language { get; set; }
    public string? Theme { get; set; }

    public ICollection<Deck> DeckWorkspace { get; set; } = new List<Deck>();
}
