using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MTGArchitectServices.ApiService.HealthChecks;

public sealed class AuthServiceHealthCheck(HttpClient httpClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct)
    {
        try
        {
            var response = await httpClient.GetAsync("/health", ct);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Auth service returned {(int)response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
