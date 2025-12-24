using System.Net;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using Microsoft.AspNetCore.Http;
using static GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions.HealthCheckFunctions;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class HealthCheckFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities:
        [
            PublicDataProcessorIntegrationTestCapability.Postgres,
            PublicDataProcessorIntegrationTestCapability.Azurite,
        ]
    )
{
    public HealthCheckFunctions Function = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        Function = lookups.GetService<HealthCheckFunctions>();
    }
}

[CollectionDefinition(nameof(HealthCheckFunctionTestsFixture))]
public class HealthCheckFunctionTestsCollection : ICollectionFixture<HealthCheckFunctionTestsFixture>;

[Collection(nameof(HealthCheckFunctionTestsFixture))]
public abstract class HealthCheckFunctionTests(HealthCheckFunctionTestsFixture fixture)
    : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    public class HealthCheckTests(HealthCheckFunctionTestsFixture fixture) : HealthCheckFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            // Ensure that the test folder for simulating the File Share Mount is present prior to
            // running the Health Check.
            Directory.CreateDirectory(fixture.GetDataSetVersionPathResolver().BasePath());

            var httpContext = new DefaultHttpContext();
            var result = await fixture.Function.HealthCheck(httpContext.Request);

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
            // Delete any pre-existing test File Share folder.
            Directory.Delete(fixture.GetDataSetVersionPathResolver().BasePath(), recursive: true);

            // Call the Health Check without an existing test File Share folder.
            var httpContext = new DefaultHttpContext();
            var result = await fixture.Function.HealthCheck(httpContext.Request);

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
