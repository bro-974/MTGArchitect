using Grpc.Core;
using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http.Json;

namespace MTGArchitect.AI.Client.Services;

public interface IMindHealthClient
{
    Task<MindHealthSnapshot> GetHealthAsync(CancellationToken ct = default);
}

public sealed record MindDependencyHealth(string Name, HealthStatus Status, string? Description);

public sealed record MindHealthSnapshot(HealthStatus Status, IReadOnlyList<MindDependencyHealth> Dependencies);

public sealed class MindHealthClient(HttpClient httpClient, Health.HealthClient grpcHealthClient) : IMindHealthClient
{
    public async Task<MindHealthSnapshot> GetHealthAsync(CancellationToken ct = default)
    {
        var grpc = await GetGrpcHealthAsync(ct);
        var lmStudio = await GetLmStudioHealthAsync(ct);

        return new MindHealthSnapshot(AggregateStatus(grpc.Status, lmStudio.Status), [grpc, lmStudio]);
    }

    private async Task<MindDependencyHealth> GetGrpcHealthAsync(CancellationToken ct)
    {
        try
        {
            var response = await grpcHealthClient.CheckAsync(new HealthCheckRequest(), cancellationToken: ct);
            var status = response.Status == HealthCheckResponse.Types.ServingStatus.Serving
                ? HealthStatus.Healthy
                : HealthStatus.Unhealthy;

            return new MindDependencyHealth("grpc", status, $"AI gRPC health returned {response.Status}.");
        }
        catch (RpcException ex)
        {
            return new MindDependencyHealth("grpc", HealthStatus.Unhealthy, ex.Status.Detail);
        }
        catch (Exception ex)
        {
            return new MindDependencyHealth("grpc", HealthStatus.Unhealthy, ex.Message);
        }
    }

    private async Task<MindDependencyHealth> GetLmStudioHealthAsync(CancellationToken ct)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<LmStudioHealthResponse>("/internal/status/lmstudio", ct);
            if (response is null)
            {
                return new MindDependencyHealth("llm", HealthStatus.Unhealthy, "AI service returned an empty LM Studio health response.");
            }

            return new MindDependencyHealth("llm", ParseStatus(response.Status), response.Description);
        }
        catch (HttpRequestException ex)
        {
            return new MindDependencyHealth("llm", HealthStatus.Unhealthy, ex.Message);
        }
        catch (NotSupportedException ex)
        {
            return new MindDependencyHealth("llm", HealthStatus.Unhealthy, ex.Message);
        }
        catch (Exception ex)
        {
            return new MindDependencyHealth("llm", HealthStatus.Unhealthy, ex.Message);
        }
    }

    private static HealthStatus ParseStatus(string? status)
    {
        return Enum.TryParse<HealthStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : HealthStatus.Unhealthy;
    }

    private static HealthStatus AggregateStatus(params HealthStatus[] statuses)
    {
        if (statuses.Any(status => status == HealthStatus.Unhealthy))
        {
            return HealthStatus.Unhealthy;
        }

        return statuses.Any(status => status == HealthStatus.Degraded)
            ? HealthStatus.Degraded
            : HealthStatus.Healthy;
    }

    private sealed record LmStudioHealthResponse(string Status, string? Description);
}