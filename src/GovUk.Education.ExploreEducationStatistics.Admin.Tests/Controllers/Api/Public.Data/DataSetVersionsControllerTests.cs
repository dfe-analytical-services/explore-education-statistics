#nullable enable
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public abstract class DataSetVersionsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/data-set-versions";

    public class DeleteVersionTests(TestApplicationFactory testApp) : DataSetVersionsControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var processorClient = new Mock<IProcessorClient>();
            processorClient
                .Setup(c => c.DeleteDataSetVersion(
                    dataSetVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Either<ActionResult, Unit>(Unit.Instance));

            var client = BuildApp(processorClient.Object).CreateClient();

            var response = await DeleteVersion(dataSetVersion.Id, client);

            response.AssertNoContent();
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: AuthenticatedUser()).CreateClient();

            var response = await DeleteVersion(Guid.NewGuid(), client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task ProcessorReturns404_Returns404()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var processorClient = new Mock<IProcessorClient>();
            processorClient
                .Setup(c => c.DeleteDataSetVersion(
                    dataSetVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Either<ActionResult, Unit>(new NotFoundResult()));

            var client = BuildApp(processorClient.Object).CreateClient();

            var response = await DeleteVersion(dataSetVersion.Id, client);

            response.AssertNotFound();
        }

        [Fact]
        public async Task ProcessorReturns400_Returns400()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var processorClient = new Mock<IProcessorClient>();
            processorClient
                .Setup(c => c.DeleteDataSetVersion(
                    dataSetVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Either<ActionResult, Unit>(
                    new BadRequestObjectResult(
                        new ValidationProblemViewModel
                        {
                            Errors = new ErrorViewModel[]
                                {
                                    new() {
                                       Code = "error code",
                                       Path = "error path"
                                    }
                                }
                        })));

            var client = BuildApp(processorClient.Object).CreateClient();

            var response = await DeleteVersion(dataSetVersion.Id, client);

            var validationProblem = response.AssertValidationProblem();

            var error = validationProblem.AssertHasError("error path", "error code");
        }

        [Fact]
        public async Task ProcessorFailureStatusCode_Returns500()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var processorClient = new Mock<IProcessorClient>();
            processorClient
                .Setup(c => c.DeleteDataSetVersion(
                    dataSetVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException());
            
            var client = BuildApp(processorClient.Object).CreateClient();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await DeleteVersion(dataSetVersion.Id, client));
            Assert.IsType<HttpRequestException>(exception.InnerException);
        }

        private WebApplicationFactory<TestStartup> BuildApp(
            IProcessorClient? processorClient = null,
            ClaimsPrincipal? user = null)
        {
            return TestApp.ConfigureServices(
                    services => { services.ReplaceService(processorClient ?? Mock.Of<IProcessorClient>()); }
                )
                .SetUser(user ?? BauUser());
        }

        private async Task<HttpResponseMessage> DeleteVersion(
            Guid dataSetVersionId,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var uri = new Uri($"{BaseUrl}/{dataSetVersionId}", UriKind.Relative);

            return await client.DeleteAsync(uri);
        }
    }
}
