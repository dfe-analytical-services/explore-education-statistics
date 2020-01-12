using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class DataFactoryPipelineStatusFunction
    {
        private readonly IReleaseStatusService _releaseStatusService;
        
        public DataFactoryPipelineStatusFunction(IReleaseStatusService releaseStatusService)
        {
            _releaseStatusService = releaseStatusService;
        }
        
        [FunctionName("DataFactoryPipelineStatusFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "datafactory/pipeline/status/")]
            HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation($"{executionContext.FunctionName} triggered");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PipelineResponse response = JsonConvert.DeserializeObject<PipelineResponse>(requestBody);

            if (response.Status == "Failed")
            {
                log.LogError($"Datafactory pipelined failed with error: {response.ErrorMessage}");
            }
            
            await _releaseStatusService.UpdateFilesStageAsync(response.ReleaseId, response.ReleaseStatusId, response.Status == "Complete"? Complete : Failed);
            
            log.LogInformation($"{executionContext.FunctionName} complete");

            return response.Status != null
                ? (ActionResult) new OkObjectResult($"status, {response.Status}")
                : new BadRequestObjectResult("No status was passed in the request body");
        }
    }

    public class PipelineResponse
    {
        public Guid ReleaseId { get; set; }
        public Guid ReleaseStatusId { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}