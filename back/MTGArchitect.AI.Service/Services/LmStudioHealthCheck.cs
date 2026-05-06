using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MTGArchitect.AI.Service.Services;

public sealed class LmStudioHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var lmStudioUri = configuration["LmStudioUri"];
        if (string.IsNullOrWhiteSpace(lmStudioUri))
        {
            return HealthCheckResult.Unhealthy("LmStudioUri configuration is missing.");
        }

        var modelsEndpoint = new Uri(new Uri($"{lmStudioUri.TrimEnd('/')}/"), "models");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, modelsEndpoint);
            using var response = await httpClientFactory.CreateClient("lmstudio-health").SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"LM Studio responded from {modelsEndpoint}.")
                : HealthCheckResult.Unhealthy($"LM Studio returned HTTP {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("LM Studio is unreachable.", ex);
        }
    }
}