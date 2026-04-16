using Microsoft.EntityFrameworkCore;
using MTGArchitect.Data.Data;
using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Repositories;

internal sealed class UserRepository(AuthDbContext dbContext) : IUserRepository
{
    public async Task<ApplicationUser?> GetByIdWithSettingsAndDecksAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(x => x.DeckWorkspace)
                .ThenInclude(x => x.Cards)
            .Include(x => x.DeckWorkspace)
                .ThenInclude(x => x.QuerySearches)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<ApplicationUser?> GetByEmailWithSettingsAndDecksAsync(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Include(x => x.DeckWorkspace)
                .ThenInclude(x => x.Cards)
            .Include(x => x.DeckWorkspace)
                .ThenInclude(x => x.QuerySearches)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }
}
