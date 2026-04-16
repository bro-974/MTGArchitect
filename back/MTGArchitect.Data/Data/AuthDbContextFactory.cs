using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MTGArchitect.Data.Data;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Database=MTGArchitectAuthDesignTime;Username=postgres;Password=postgres");

        return new AuthDbContext(optionsBuilder.Options);
    }
}
