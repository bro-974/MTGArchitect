using Grpc.Core;

namespace MTGArchitect.AI.Service.Services
{
    public class MindService(ILogger<MindService> logger) : Mind.MindBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            logger.LogInformation("The message is received from {Name}", request.Name);

            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
