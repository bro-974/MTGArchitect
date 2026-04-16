using MTGArchitect.Data.Models;

namespace MTGArchitect.Data.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdWithSettingsAndDecksAsync(string userId, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailWithSettingsAndDecksAsync(string email, CancellationToken cancellationToken = default);
}
