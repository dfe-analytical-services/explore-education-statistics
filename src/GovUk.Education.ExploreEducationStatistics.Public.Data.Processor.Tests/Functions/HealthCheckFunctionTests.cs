using System.Net;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class HealthCheckFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class HealthCheckTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : HealthCheckFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            // Ensure that the test folder for simulating the File Share Mount is present prior to
            // running the Health Check.
            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            Directory.CreateDirectory(dataSetVersionPathResolver.BasePath());
            
            var function = GetRequiredService<HealthCheckFunctions>();

            var httpContext = new DefaultHttpContext();
            var result = await function.HealthCheck(httpContext.Request);

            var expectedHealthCheckResult = new HealthCheckFunctions.HealthCheckResponse(
                PsqlConnection: new HealthCheckFunctions.HealthCheckSummary(Healthy: true),
                FileShareMount: new HealthCheckFunctions.HealthCheckSummary(Healthy: true));
            
            result.AssertOkObjectResult(expectedHealthCheckResult);
            Assert.Equal((int) HttpStatusCode.OK, httpContext.Response.StatusCode);
        }
        
        [Fact]
        public async Task Failure_NoFileShareMount()
        {
            var function = GetRequiredService<HealthCheckFunctions>();

            // Call the Health Check without firstly adding a test File Share folder.
            var httpContext = new DefaultHttpContext();
            var result = await function.HealthCheck(httpContext.Request);

            var expectedHealthCheckResult = new HealthCheckFunctions.HealthCheckResponse(
                PsqlConnection: new HealthCheckFunctions.HealthCheckSummary(Healthy: true),
                FileShareMount: new HealthCheckFunctions.HealthCheckSummary(
                    Healthy: false, 
                    "File Share Mount folder does not exist"));
            
            result.AssertObjectResult(HttpStatusCode.InternalServerError, expectedHealthCheckResult);
        }
    }
}
