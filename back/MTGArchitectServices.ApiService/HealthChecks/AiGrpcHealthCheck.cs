using Grpc.Core;
using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MTGArchitectServices.ApiService.HealthChecks;

public sealed class AiGrpcHealthCheck(Health.HealthClient grpcHealthClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct)
    {
        try
        {
            var response = await grpcHealthClient.CheckAsync(new HealthCheckRequest(), cancellationToken: ct);
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
