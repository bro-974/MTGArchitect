using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MTGArchitectServices.ApiService.HealthChecks;

public sealed class ScryfallGrpcHealthCheck(GrpcClientFactory grpcClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct)
    {
        try
        {
            var client = grpcClientFactory.CreateClient<Health.HealthClient>("scryfall-health");
            var response = await client.CheckAsync(new HealthCheckRequest(), cancellationToken: ct);
            return response.Status == HealthCheckResponse.Types.ServingStatus.Serving
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"gRPC returned {response.Status}");
        }
        catch (RpcException ex)
        {
            return HealthCheckResult.Unhealthy(ex.Status.Detail);
        }
    }
}
