using System.Net;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using static GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions.HealthCheckFunctions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class HealthCheckFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class HealthCheckTests(ProcessorFunctionsIntegrationTestFixture fixture) : HealthCheckFunctionTests(fixture)
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

            var expectedHealthCheckResult = new HealthCheckResponse(
                PsqlConnection: HealthCheckSummary.Healthy(),
                FileShareMount: HealthCheckSummary.Healthy(),
                CoreStorageConnection: HealthCheckSummary.Healthy(),
                ContentDbConnection: HealthCheckSummary.Healthy()
            );

            result.AssertOkObjectResult(expectedHealthCheckResult);
        }

        [Fact]
        public async Task Failure_NoFileShareMount()
        {
            var function = GetRequiredService<HealthCheckFunctions>();

            // Call the Health Check without firstly adding a test File Share folder.
            var httpContext = new DefaultHttpContext();
            var result = await function.HealthCheck(httpContext.Request);

            var expectedHealthCheckResult = new HealthCheckResponse(
                PsqlConnection: HealthCheckSummary.Healthy(),
                FileShareMount: HealthCheckSummary.Unhealthy("File Share Mount folder does not exist"),
                CoreStorageConnection: HealthCheckSummary.Healthy(),
                ContentDbConnection: HealthCheckSummary.Healthy()
            );

            result.AssertObjectResult(HttpStatusCode.InternalServerError, expectedHealthCheckResult);
        }
    }
}
