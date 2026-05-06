using MTGArchitect.AI.Client.Services;

namespace MTGArchitectServices.ApiService.Services;

public sealed class MindHealthService(IMindHealthClient mindHealthClient)
{
    public Task<MindHealthSnapshot> GetHealthAsync(CancellationToken ct)
    {
        return mindHealthClient.GetHealthAsync(ct);
    }
}