using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http.Json;

namespace MTGArchitectServices.ApiService.HealthChecks;

public sealed class LmStudioRemoteHealthCheck(HttpClient httpClient) : IHealthCheck
{
    private sealed record StatusResponse(string Status, string? Description);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct)
    {
        var response = await httpClient.GetFromJsonAsync<StatusResponse>("/internal/status/lmstudio", ct);
        if (response is null)
            return HealthCheckResult.Unhealthy("Empty response from AI service.");

        return Enum.TryParse<HealthStatus>(response.Status, ignoreCase: true, out var parsed) && parsed == HealthStatus.Healthy
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy(response.Description);
    }
}
