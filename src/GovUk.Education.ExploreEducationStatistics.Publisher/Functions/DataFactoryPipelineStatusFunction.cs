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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "publisher/datafactory/pipeline/status/")]
            HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation($"{executionContext.FunctionName} triggered");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var releaseId = new Guid(data.ReleaseId);
            var releaseStatusId = new Guid(data.ReleaseStatusId);
            string status = data.Status;
            string err = data.ErrorMessage;

            if (status == "Failed")
            {
                log.LogError($"Datafactory pipelined failed with error: {err}");
            }
            
            await _releaseStatusService.UpdateFilesStageAsync(releaseId, releaseStatusId, status == "Complete"? Complete : Failed);
            
            log.LogInformation($"{executionContext.FunctionName} complete");

            return status != null
                ? (ActionResult) new OkObjectResult($"status, {status}")
                : new BadRequestObjectResult("No status was passed in the request body");
        }
    }
}