using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class DataProcessorFunction
{
    private readonly ILogger<DataProcessorFunction> _logger;
    private readonly PublicDataDbContext _context;

    public DataProcessorFunction(
        ILogger<DataProcessorFunction> logger,
        PublicDataDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("DataProcessor")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req,
        FunctionContext executionContext)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        return new OkObjectResult($"Found {_context.DataSets.Count()} datasets");
    }
}