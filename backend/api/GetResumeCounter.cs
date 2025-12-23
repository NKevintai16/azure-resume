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
            _container = _cosmosClient.GetDatabase("resume-database").GetContainer("Container");
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
                var itemResponse = await _container.ReadItemAsync<dynamic>("1", new PartitionKey("1"));
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
