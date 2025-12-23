using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Common;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace api
{
    public class GetResumeCounter
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public GetResumeCounter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetResumeCounter>();

            _cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("AzureResumeConnectionString"));
            _container = _cosmosClient.GetDatabase("resumeapidb").GetContainer("Counter");
        }

        [Function("GetResumeCounter")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getresumecounter")]
            HttpRequestData req,
            FunctionContext context)
        {
            _logger.LogInformation("Resume counter function triggered.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            try
            {
                var itemResponse = await _container.ReadItemAsync<dynamic>("visitor-counter-id", new PartitionKey("visitor-counter-pk"));
                var count = itemResponse.Resource.count;
                response.WriteString($"{{ \"count\": {count} }}");
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                response.WriteString("{ \"count\": 0 }");
            }
            return response;
        }
    }
}
