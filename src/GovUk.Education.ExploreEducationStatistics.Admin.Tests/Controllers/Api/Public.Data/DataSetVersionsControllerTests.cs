#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data.PublicDataApiClient;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using Newtonsoft.Json;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public abstract class DataSetVersionsControllerTests(
    TestApplicationFactory testApp)
    : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/data-set-versions";

    public class ListVersionsTests(
        TestApplicationFactory testApp) : DataSetVersionsControllerTests(testApp)
    {
        [Theory]
        [MemberData(nameof(DataSetVersionStatusTheoryData.AvailableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData))]
        public async Task OnlyPreviouslyPublishedVersionsReturned(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublication(DataFixture.DefaultPublication())))
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseFile.Id))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await ListLiveVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 10);

            var viewModel = response.AssertOk<PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>>();

            Assert.NotNull(viewModel);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(10, viewModel.Paging.PageSize);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(1, viewModel.Paging.TotalResults);

            var liveVersion = Assert.Single(viewModel.Results);
            Assert.Equal(currentDataSetVersion.Id, liveVersion.Id);
            Assert.Equal(currentDataSetVersion.PublicVersion, liveVersion.Version);
            Assert.Equal(currentDataSetVersion.Status, liveVersion.Status);
            Assert.Equal(currentDataSetVersion.VersionType, liveVersion.Type);

            Assert.Equal(releaseFile.ReleaseVersion.Id, liveVersion.ReleaseVersion.Id);
            Assert.Equal(releaseFile.ReleaseVersion.Release.Title, liveVersion.ReleaseVersion.Title);

            Assert.Equal(releaseFile.File.DataSetFileId, liveVersion.File.Id);
            Assert.Equal(releaseFile.Name, liveVersion.File.Title);

            liveVersion.Published.AssertEqual(currentDataSetVersion.Published!.Value);
        }

        [Theory]
        [MemberData(nameof(DataSetVersionStatusTheoryData.UnavailableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData))]
        public async Task DraftVersionsNotReturned(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await ListLiveVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 10);

            var viewModel = response.AssertOk<PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>>();

            Assert.NotNull(viewModel);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(10, viewModel.Paging.PageSize);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(0, viewModel.Paging.TotalResults);

            Assert.Empty(viewModel.Results);
        }

        [Theory]
        [InlineData(1, 1, 0)]
        [InlineData(1, 2, 1)]
        [InlineData(1, 2, 2)]
        [InlineData(1, 2, 9)]
        [InlineData(2, 2, 9)]
        [InlineData(2, 2, 2)]
        public async Task ResultsArePaginatedCorrectly(
            int page,
            int pageSize,
            int numberOfPublishedDataSetVersions)
        {
            var releaseFiles = DataFixture.DefaultReleaseFile()
                .ForInstance(s => s.Set(rf => rf.ReleaseVersion, () => DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublication(DataFixture.DefaultPublication()))))
                .ForInstance(s => s.Set(rf => rf.File, () => DataFixture.DefaultFile(FileType.Data)))
                .GenerateList(numberOfPublishedDataSetVersions);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(releaseFiles);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var now = DateTimeOffset.UtcNow.AddDays(-numberOfPublishedDataSetVersions);
            var dataSetVersions = releaseFiles
                .Select(
                    (rf, index) => DataFixture
                        .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                        .WithVersionNumber(major: 1, minor: index, patch: 1)
                        .WithStatusPublished()
                        .WithPublished(now.AddDays(index))
                        .WithDataSet(dataSet)
                        .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                            .WithReleaseFileId(rf.Id))
                        .Generate()
                )
                .ToList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersions);
                context.DataSets.Update(dataSet);
            });

            var response = await ListLiveVersions(
                dataSetId: dataSet.Id,
                page: page,
                pageSize: pageSize);

            var viewModel = response.AssertOk<PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>>();

            var pagedDataSetVersionIds = dataSetVersions
                .OrderByDescending(dsv => dsv.Published)
                .Select(dsv => dsv.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Assert.NotNull(viewModel);
            Assert.Equal(page, viewModel.Paging.Page);
            Assert.Equal(pageSize, viewModel.Paging.PageSize);
            Assert.Equal(numberOfPublishedDataSetVersions, viewModel.Paging.TotalResults);
            Assert.Equal(pagedDataSetVersionIds.Count, viewModel.Results.Count);
            Assert.All(viewModel.Results, dsv => Assert.Contains(dsv.Id, pagedDataSetVersionIds));
        }

        [Fact]
        public async Task VersionsForDifferentDataSetNotReturned()
        {
            ReleaseFile targetReleaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublication(DataFixture.DefaultPublication())))
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            ReleaseFile otherReleaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublication(DataFixture.DefaultPublication())))
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(targetReleaseFile, otherReleaseFile);
            });

            DataSet targetDataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            DataSet otherDataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSets.AddRange(targetDataSet, otherDataSet));

            DataSetVersion targetDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(targetDataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(targetReleaseFile.Id))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion otherDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(otherDataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(otherReleaseFile.Id))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(targetDataSetVersion, otherDataSetVersion);
                context.DataSets.UpdateRange(targetDataSet, otherDataSet);
            });

            var response = await ListLiveVersions(
                dataSetId: targetDataSet.Id,
                page: 1,
                pageSize: 10);

            var viewModel = response.AssertOk<PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>>();

            Assert.NotNull(viewModel);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(10, viewModel.Paging.PageSize);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(1, viewModel.Paging.TotalResults);

            var liveVersion = Assert.Single(viewModel.Results);
            Assert.Equal(targetDataSetVersion.Id, liveVersion.Id);
            Assert.Equal(targetDataSetVersion.PublicVersion, liveVersion.Version);
            Assert.Equal(targetDataSetVersion.Status, liveVersion.Status);
            Assert.Equal(targetDataSetVersion.VersionType, liveVersion.Type);

            Assert.Equal(targetReleaseFile.ReleaseVersion.Id, liveVersion.ReleaseVersion.Id);
            Assert.Equal(targetReleaseFile.ReleaseVersion.Release.Title, liveVersion.ReleaseVersion.Title);

            Assert.Equal(targetReleaseFile.File.DataSetFileId, liveVersion.File.Id);
            Assert.Equal(targetReleaseFile.Name, liveVersion.File.Title);

            liveVersion.Published.AssertEqual(targetDataSetVersion.Published!.Value);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: DataFixture.AuthenticatedUser()).CreateClient();

            var response = await ListLiveVersions(
                dataSetId: Guid.NewGuid(),
                page: 1,
                pageSize: 1,
                client: client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task NoDataSetId_Returns400()
        {
            var client = BuildApp().CreateClient();

            var response = await client.GetAsync(BaseUrl);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasNotEmptyError("dataSetId");
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            var response = await ListLiveVersions(dataSetId: Guid.NewGuid());

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> ListLiveVersions(
            Guid dataSetId,
            int? page = null,
            int? pageSize = null,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var queryParams = new Dictionary<string, string?>
            {
                { "dataSetId", dataSetId.ToString() },
                { "page", page?.ToString() },
                { "pageSize", pageSize?.ToString() }
            };

            var uri = QueryHelpers.AddQueryString(BaseUrl, queryParams);

            return await client.GetAsync(uri);
        }
    }

    public class CreateNextVersionTests(
        TestApplicationFactory testApp) : DataSetVersionsControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublication(DataFixture.DefaultPublication())))
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(currentDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersion? nextVersion = null;

            var processorClient = new Mock<IProcessorClient>(MockBehavior.Strict);

            processorClient
                .Setup(c => c.CreateNextDataSetVersionMappings(dataSet.Id,
                    releaseFile.Id,
                    It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    var savedDataSet = await TestApp.GetDbContext<PublicDataDbContext>()
                        .DataSets
                        .SingleAsync(ds => ds.Id == dataSet.Id);

                    nextVersion = DataFixture
                        .DefaultDataSetVersion()
                        .WithStatusMapping()
                        .WithVersionNumber(major: 1, minor: 1)
                        .WithDataSet(savedDataSet)
                        .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                            .WithReleaseFileId(releaseFile.Id))
                        .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

                    await TestApp.AddTestData<PublicDataDbContext>(context =>
                    {
                        context.DataSetVersions.Add(nextVersion);
                        context.DataSets.Update(savedDataSet);
                    });

                    return new ProcessDataSetVersionResponseViewModel
                    {
                        DataSetId = dataSet.Id,
                        DataSetVersionId = nextVersion.Id,
                        InstanceId = Guid.NewGuid()
                    };
                });

            var client = BuildApp(processorClient.Object).CreateClient();

            var response = await CreateNextVersion(
                dataSetId: dataSet.Id,
                releaseFileId: releaseFile.Id,
                client);

            var viewModel = response.AssertOk<DataSetVersionSummaryViewModel>();

            Assert.NotNull(nextVersion);
            Assert.Equal(viewModel.Id, nextVersion.Id);
            Assert.Equal(viewModel.Version, nextVersion.PublicVersion);
            Assert.Equal(viewModel.Status, nextVersion.Status);
            Assert.Equal(viewModel.Type, nextVersion.VersionType);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: DataFixture.AuthenticatedUser()).CreateClient();

            var response = await CreateNextVersion(
                dataSetId: Guid.NewGuid(),
                releaseFileId: Guid.NewGuid(),
                client: client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var client = BuildApp().CreateClient();

            var response = await CreateNextVersion(
                dataSetId: Guid.Empty,
                releaseFileId: Guid.Empty,
                client: client);

            var validationProblem = response.AssertValidationProblem();
            Assert.Equal(2, validationProblem.Errors.Count);
            validationProblem.AssertHasNotEmptyError("dataSetId");
            validationProblem.AssertHasNotEmptyError("releaseFileId");
        }

        private async Task<HttpResponseMessage> CreateNextVersion(
            Guid dataSetId,
            Guid releaseFileId,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var uri = new Uri(BaseUrl, UriKind.Relative);

            return await client.PostAsync(uri,
                new JsonNetContent(new NextDataSetVersionCreateRequest
                {
                    DataSetId = dataSetId,
                    ReleaseFileId = releaseFileId
                }));
        }
    }

    public class CompleteNextVersionTests(
        TestApplicationFactory testApp) : DataSetVersionsControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublication(DataFixture.DefaultPublication())))
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseFile.Id))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var processorClient = new Mock<IProcessorClient>(MockBehavior.Strict);

            processorClient
                .Setup(c => c.CompleteNextDataSetVersionImport(
                    nextDataSetVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ProcessDataSetVersionResponseViewModel
                {
                    DataSetId = dataSet.Id,
                    DataSetVersionId = nextDataSetVersion.Id,
                    InstanceId = Guid.NewGuid()
                });

            var client = BuildApp(processorClient.Object).CreateClient();

            var response = await CompleteNextVersionImport(
                dataSetVersionId: nextDataSetVersion.Id,
                client);

            var viewModel = response.AssertOk<DataSetVersionSummaryViewModel>();

            Assert.Equal(viewModel.Id, nextDataSetVersion.Id);
            Assert.Equal(viewModel.Version, nextDataSetVersion.PublicVersion);
            Assert.Equal(viewModel.Status, nextDataSetVersion.Status);
            Assert.Equal(viewModel.Type, nextDataSetVersion.VersionType);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: DataFixture.AuthenticatedUser()).CreateClient();

            var response = await CompleteNextVersionImport(
                dataSetVersionId: Guid.NewGuid(),
                client: client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var client = BuildApp().CreateClient();

            var response = await CompleteNextVersionImport(
                dataSetVersionId: Guid.Empty,
                client: client);

            var validationProblem = response.AssertValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasNotEmptyError("dataSetVersionId");
        }

        private async Task<HttpResponseMessage> CompleteNextVersionImport(
            Guid dataSetVersionId,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var uri = new Uri($"{BaseUrl}/complete", UriKind.Relative);

            return await client.PostAsync(uri,
                new JsonNetContent(new NextDataSetVersionCompleteImportRequest
                {
                    DataSetVersionId = dataSetVersionId,
                }));
        }
    }

    public class DeleteVersionTests(TestApplicationFactory testApp) : DataSetVersionsControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            var dataSetVersionId = Guid.NewGuid();

            var processorClient = new Mock<IProcessorClient>(MockBehavior.Strict);

            processorClient
                .Setup(c => c.DeleteDataSetVersion(
                    dataSetVersionId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Either<ActionResult, Unit>(Unit.Instance));

            var client = BuildApp(processorClient.Object).CreateClient();

            var response = await DeleteVersion(dataSetVersionId, client);

            response.AssertNoContent();
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: DataFixture.AuthenticatedUser()).CreateClient();

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
                .WithVersionNumber(1, 0)
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

            var processorClient = new Mock<IProcessorClient>(MockBehavior.Strict);

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
                .WithVersionNumber(1, 0)
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

            var processorClient = new Mock<IProcessorClient>(MockBehavior.Strict);

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
                                new()
                                {
                                    Code = "error code",
                                    Path = "error path"
                                }
                            }
                        })));

            var client = BuildApp(processorClient.Object).CreateClient();

            var response = await DeleteVersion(dataSetVersion.Id, client);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError("error path", "error code");
        }

        [Fact]
        public async Task ProcessorClientThrows_Returns500()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
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

            var processorClient = new Mock<IProcessorClient>(MockBehavior.Strict);

            processorClient
                .Setup(c => c.DeleteDataSetVersion(
                    dataSetVersion.Id,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException());

            var client = BuildApp(processorClient.Object).CreateClient();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => DeleteVersion(dataSetVersion.Id, client)
            );

            Assert.IsType<HttpRequestException>(exception.InnerException);
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

    public class GetVersionChangesTests(TestApplicationFactory testApp) : DataSetVersionsControllerTests(testApp)
    {
        public static TheoryData<ChangeSetViewModelDto> PublicApiChangeSetData =>
            [
                // Changes with every change type
                new() {
                    Filters = [
                            new FilterChangeViewModelDto {
                                CurrentState = new FilterViewModelDto {
                                    Id = Guid.NewGuid().ToString(),
                                    Column = "filter1",
                                    Label = "Filter 1 after",
                                    Hint = "Filter 1 hint"
                                },
                                PreviousState = new FilterViewModelDto {
                                    Id = Guid.NewGuid().ToString(),
                                    Column = "filter1",
                                    Label = "Filter 1 before",
                                    Hint = "Filter 1 hint"
                                }
                            }
                        ],
                    FilterOptions = [
                            new FilterOptionChangesViewModelDto {
                                Filter = new FilterViewModelDto {
                                    Id = Guid.NewGuid().ToString(),
                                    Column = "filter1",
                                    Label = "Filter 1 after",
                                    Hint = "Filter 1 hint",
                                },
                                Options = [
                                    new FilterOptionChangeViewModelDto {
                                        CurrentState = new FilterOptionViewModelDto {
                                            Id = Guid.NewGuid().ToString(),
                                            Label = "Filter 1 Option 1 after",
                                        },
                                        PreviousState = new FilterOptionViewModelDto {
                                            Id = Guid.NewGuid().ToString(),
                                            Label = "Filter 1 Option 1 before",
                                        }
                                    }
                                ]
                            }
                        ],
                    GeographicLevels = [
                            new GeographicLevelChangeViewModelDto {
                                CurrentState = new GeographicLevelViewModelDto {
                                    Code = GeographicLevel.Country,
                                    Label = "Country after",
                                },
                                PreviousState = new GeographicLevelViewModelDto {
                                    Code = GeographicLevel.Country,
                                    Label = "Country before",
                                }
                            }
                        ],
                    Indicators = [
                            new IndicatorChangeViewModelDto {
                                CurrentState = new IndicatorViewModelDto {
                                    Id = Guid.NewGuid().ToString(),
                                    Column = "indicator1",
                                    Label = "Indicator 1 after",
                                    Unit =  IndicatorUnit.PercentagePoint,
                                    DecimalPlaces = 2,
                                },
                                PreviousState = new IndicatorViewModelDto {
                                    Id = Guid.NewGuid().ToString(),
                                    Column = "indicator1",
                                    Label = "Indicator 1 before",
                                    Unit =  IndicatorUnit.PercentagePoint,
                                    DecimalPlaces = 2,
                                }
                            }
                        ],
                    LocationGroups = [
                            new LocationGroupChangeViewModelDto {
                                CurrentState = new LocationGroupViewModelDto {
                                    Level = new GeographicLevelViewModelDto {
                                        Code = GeographicLevel.Country,
                                        Label = "Country after",
                                    }
                                },
                                PreviousState = new LocationGroupViewModelDto {
                                    Level = new GeographicLevelViewModelDto {
                                        Code = GeographicLevel.Country,
                                        Label = "Country before",
                                    }
                                }
                            }
                        ],
                    LocationOptions = [
                            new LocationOptionChangesViewModelDto {
                                Level = new GeographicLevelViewModelDto {
                                    Code = GeographicLevel.Country,
                                    Label = "Country after"
                                },
                                Options = [
                                    new LocationOptionChangeViewModelDto {
                                        CurrentState = new LocationOptionViewModelDto {
                                            Id = Guid.NewGuid().ToString(),
                                            Label = "Country 1 after",
                                            Code = "country 1 code",
                                            OldCode = "country 1 old code",
                                            Ukprn = "country 1 ukprn",
                                            Urn = "country 1 urn",
                                            LaEstab = "country 1 laEstab",
                                        },
                                        PreviousState = new LocationOptionViewModelDto {
                                            Id = Guid.NewGuid().ToString(),
                                            Label = "Country 1 before",
                                            Code = "country 1 code",
                                            OldCode = "country 1 old code",
                                            Ukprn = "country 1 ukprn",
                                            Urn = "country 1 urn",
                                            LaEstab = "country 1 laEstab",
                                        }
                                    }
                                ]
                            }
                        ],
                    TimePeriods = [
                            new TimePeriodOptionChangeViewModelDto {
                                CurrentState = new TimePeriodOptionViewModelDto {
                                    Label = "Time Period Option 1 after",
                                    Code = TimeIdentifier.AcademicYearQ1,
                                    Period = "2024/25",
                                },
                                PreviousState = new TimePeriodOptionViewModelDto {
                                    Label = "Time Period Option 1 before",
                                    Code = TimeIdentifier.AcademicYearQ1,
                                    Period = "2024/25",
                                }
                            }
                        ],
                },
                // Empty changes
                new(),
                new() {
                    Filters = [],
                    FilterOptions = [],
                    GeographicLevels = [],
                    Indicators = [],
                    LocationGroups = [],
                    LocationOptions = [],
                    TimePeriods = [],
                }
            ];

        [Theory]
        [MemberData(nameof(PublicApiChangeSetData))]
        public async Task Success_Returns200(ChangeSetViewModelDto changeSetDto)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var mockedChanges = new DataSetVersionChangesViewModelDto
            {
                MajorChanges = changeSetDto,
                MinorChanges = changeSetDto,
            };

            var publicDataApiClient = new Mock<IPublicDataApiClient>(MockBehavior.Strict);

            publicDataApiClient
                .Setup(c => c.GetDataSetVersionChanges(
                    dataSetVersion.DataSetId,
                    dataSetVersion.PublicVersion,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockedChanges);

            var client = BuildApp(publicDataApiClient: publicDataApiClient.Object)
                .CreateClient();

            var response = await GetVersionChanges(dataSetVersion.Id, client);

            var result = response.AssertOk<DataSetVersionChangesViewModel>();

            Assert.Equal(dataSet.Id, result.DataSet.Id);
            Assert.Equal(dataSet.Title, result.DataSet.Title);
            Assert.Equal(dataSetVersion.Id, result.DataSetVersion.Id);
            Assert.Equal(dataSetVersion.PublicVersion, result.DataSetVersion.Version);
            Assert.Equal(dataSetVersion.Status, result.DataSetVersion.Status);
            Assert.Equal(dataSetVersion.VersionType, result.DataSetVersion.Type);
            Assert.Equal(dataSetVersion.Notes, result.DataSetVersion.Notes);

            // The changes returned from the Public API should have its properties mapped 1-to-1
            // onto the Admin API's intended view model. Hence, the serialized objects should look the same.
            Assert.Equal(JsonSerializer.Serialize(mockedChanges), JsonConvert.SerializeObject(result.Changes));
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: DataFixture.AuthenticatedUser()).CreateClient();

            var response = await GetVersionChanges(Guid.NewGuid(), client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            var response = await GetVersionChanges(Guid.NewGuid());

            response.AssertNotFound();
        }

        [Fact]
        public async Task PublicDataApiReturns400_Returns400()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var publicDataApiClient = new Mock<IPublicDataApiClient>(MockBehavior.Strict);

            publicDataApiClient
                .Setup(c => c.GetDataSetVersionChanges(
                    dataSetVersion.DataSetId,
                    dataSetVersion.PublicVersion,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidationUtils.ValidationResult());

            var client = BuildApp(publicDataApiClient: publicDataApiClient.Object)
                .CreateClient();

            var response = await GetVersionChanges(dataSetVersion.Id, client);

            MockUtils.VerifyAllMocks(publicDataApiClient);

            response.AssertValidationProblem();
        }

        [Fact]
        public async Task PublicDataApiClientThrows_Returns500()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var publicDataApiClient = new Mock<IPublicDataApiClient>(MockBehavior.Strict);

            publicDataApiClient
                .Setup(c => c.GetDataSetVersionChanges(
                    dataSetVersion.DataSetId,
                    dataSetVersion.PublicVersion,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException());

            var client = BuildApp(publicDataApiClient: publicDataApiClient.Object)
                .CreateClient();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => GetVersionChanges(dataSetVersion.Id, client)
            );

            Assert.IsType<HttpRequestException>(exception.InnerException);

            MockUtils.VerifyAllMocks(publicDataApiClient);
        }

        private async Task<HttpResponseMessage> GetVersionChanges(
            Guid dataSetVersionId,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var uri = new Uri($"{BaseUrl}/{dataSetVersionId}/changes", UriKind.Relative);

            return await client.GetAsync(uri);
        }

        private record MockedChanges
        {
            public List<string> Changes { get; init; } = [];
        }
    }

    public class UpdateVersionTests(TestApplicationFactory testApp) : DataSetVersionsControllerTests(testApp)
    {
        [Theory]
        [MemberData(nameof(DataSetVersionStatusTheoryData.UpdateableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData))]
        public async Task Success(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion()
                    .WithRelease(DataFixture.DefaultRelease()
                        .WithPublication(DataFixture.DefaultPublication())))
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease()
                    .WithReleaseFileId(releaseFile.Id))
                .WithNotes("initial notes.")
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var updateRequest = new DataSetVersionUpdateRequest { Notes = "updated notes." };

            var response = await UpdateVersion(nextDataSetVersion.Id, updateRequest);

            var viewModel = response.AssertOk<DataSetDraftVersionViewModel>();

            Assert.NotNull(viewModel);
            Assert.Equal(nextDataSetVersion.Id, viewModel.Id);
            Assert.Equal(nextDataSetVersion.PublicVersion, viewModel.Version);
            Assert.Equal(nextDataSetVersion.Status, viewModel.Status);
            Assert.Equal(nextDataSetVersion.VersionType, viewModel.Type);
            Assert.Equal(releaseFile.File.DataSetFileId!.Value, viewModel.File.Id);
            Assert.Equal(releaseFile.Name, viewModel.File.Title);
            Assert.Equal(releaseFile.ReleaseVersion.Id, viewModel.ReleaseVersion.Id);
            Assert.Equal(releaseFile.ReleaseVersion.Release.Title, viewModel.ReleaseVersion.Title);
            Assert.Equal(nextDataSetVersion.TotalResults, viewModel.TotalResults);
            Assert.Equal("updated notes.", viewModel.Notes);
            Assert.Equal(
                nextDataSetVersion.MetaSummary!.GeographicLevels.Select(l => l.GetEnumLabel()),
                viewModel.GeographicLevels);
            Assert.Equal(
                TimePeriodRangeViewModel.Create(nextDataSetVersion.MetaSummary!.TimePeriodRange),
                viewModel.TimePeriods);
            Assert.Equal(
                nextDataSetVersion.MetaSummary!.Filters,
                viewModel.Filters);
            Assert.Equal(
                nextDataSetVersion.MetaSummary!.Indicators,
                viewModel.Indicators);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: DataFixture.AuthenticatedUser()).CreateClient();

            var updateRequest = new DataSetVersionUpdateRequest();

            var response = await UpdateVersion(Guid.NewGuid(), updateRequest, client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionDoesNotExist_Returns404()
        {
            var updateRequest = new DataSetVersionUpdateRequest();

            var response = await UpdateVersion(Guid.NewGuid(), updateRequest);

            response.AssertNotFound();
        }

        [Theory]
        [MemberData(nameof(DataSetVersionStatusTheoryData.ReadOnlyStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData))]
        public async Task DataSetVersionCannotBeUpdated_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var updateRequest = new DataSetVersionUpdateRequest();

            var response = await UpdateVersion(nextDataSetVersion.Id, updateRequest);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "dataSetVersionId",
                expectedCode: ValidationMessages.DataSetVersionCannotBeUpdated.Code);
        }

        [Fact]
        public async Task DataSetVersionIsFirstVersion_UpdatingNotes_Returns400()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var updateRequest = new DataSetVersionUpdateRequest { Notes = "updated notes." };

            var response = await UpdateVersion(dataSetVersion.Id, updateRequest);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "notes",
                expectedCode: ValidationMessages.DataSetVersionCannotHaveNotes.Code);
        }

        private async Task<HttpResponseMessage> UpdateVersion(
            Guid dataSetVersionId,
            DataSetVersionUpdateRequest updateRequest,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            return await client.PatchAsJsonAsync($"{BaseUrl}/{dataSetVersionId}", updateRequest);
        }
    }

    private WebApplicationFactory<TestStartup> BuildApp(
        IProcessorClient? processorClient = null,
        IPublicDataApiClient? publicDataApiClient = null,
        ClaimsPrincipal? user = null)
    {
        return TestApp.ConfigureServices(services =>
            {
                services.ReplaceService(processorClient ?? Mock.Of<IProcessorClient>());
                services.ReplaceService(publicDataApiClient ?? Mock.Of<IPublicDataApiClient>());
            })
            .SetUser(user ?? DataFixture.BauUser());
    }
}
