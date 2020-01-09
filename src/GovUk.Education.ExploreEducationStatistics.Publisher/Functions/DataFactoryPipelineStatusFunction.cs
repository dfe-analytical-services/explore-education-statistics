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
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "publisher/datafactory/pipeline/status")]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var releaseId = data.ReleaseId;
            var releaseStatusId = data.ReleaseStatusId;
            var status = data.status;
            
            // TODO check status etc

            await _releaseStatusService.UpdateFilesStageAsync(releaseId, releaseStatusId, Complete);

            return status != null
                ? (ActionResult) new OkObjectResult($"status, {status}")
                : new BadRequestObjectResult("No status was passed in the request body");
        }
    }
    
}