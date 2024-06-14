using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using PublicationSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Content.ViewModels.PublicationSummaryViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public abstract class DataSetsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/v1/data-sets";

    public class GetDataSetTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        [Theory]
        [InlineData(DataSetStatus.Published)]
        [InlineData(DataSetStatus.Deprecated)]
        public async Task DataSetIsAvailable_Returns200(DataSetStatus dataSetStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(dataSetStatus);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await GetDataSet(dataSet.Id);

            var content = response.AssertOk<DataSetViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(dataSet.Id, content.Id);
            Assert.Equal(dataSet.Title, content.Title);
            Assert.Equal(dataSet.Summary, content.Summary);
            Assert.Equal(dataSet.Status, content.Status);
            Assert.Equal(dataSet.SupersedingDataSetId, content.SupersedingDataSetId);
            Assert.Equal(dataSetVersion.Version, content.LatestVersion.Version);
            Assert.Equal(
                dataSetVersion.Published.TruncateNanoseconds(),
                content.LatestVersion.Published
            );
            Assert.Equal(dataSetVersion.TotalResults, content.LatestVersion.TotalResults);
            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary!.TimePeriodRange.Start.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Code),
                content.LatestVersion.TimePeriods.Start);
            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Code),
                content.LatestVersion.TimePeriods.End);
            Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, content.LatestVersion.GeographicLevels);
            Assert.Equal(dataSetVersion.MetaSummary.Filters, content.LatestVersion.Filters);
            Assert.Equal(dataSetVersion.MetaSummary.Indicators, content.LatestVersion.Indicators);
        }

        [Theory]
        [InlineData(DataSetStatus.Draft)]
        [InlineData(DataSetStatus.Withdrawn)]
        public async Task DataSetNotAvailable_Returns403(DataSetStatus dataSetStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(dataSetStatus);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetDataSet(dataSet.Id);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            var response = await GetDataSet(Guid.NewGuid());

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSet(Guid dataSetId)
        {
            var client = BuildApp().CreateClient();

            var uri = new Uri($"{BaseUrl}/{dataSetId}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class ListDataSetVersionsTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        [Theory]
        [InlineData(1, 2, 1)]
        [InlineData(1, 2, 2)]
        [InlineData(1, 2, 9)]
        [InlineData(1, 3, 2)]
        [InlineData(2, 2, 9)]
        public async Task MultipleAvailableVersionsForRequestedDataSet_Returns200_PaginatedCorrectly(
           int page,
           int pageSize,
           int numberOfAvailableDataSetVersions)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .GenerateList(numberOfAvailableDataSetVersions);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.AddRange(dataSetVersions));

            var pagedDataSetVersions = dataSetVersions
                .OrderByDescending(dsv => dsv.VersionMajor)
                .ThenByDescending(dsv => dsv.VersionMinor)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var releaseFiles = pagedDataSetVersions
                .Select(
                    dsv => DefaultReleaseFileViewModel()
                        .ForInstance(s => s.Set(rf => rf.Id, dsv.ReleaseFileId))
                        .Generate()
                )
                .ToList();

            var contentApiClient = new Mock<IContentApiClient>(MockBehavior.Strict);

            contentApiClient
                .Setup(c => c.ListReleaseFiles(
                    It.Is<ReleaseFileListRequest>(req =>
                        releaseFiles.Select(rf => rf.Id).SequenceEqual(req.Ids)),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(releaseFiles);

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: page,
                pageSize: pageSize,
                contentApiClient: contentApiClient.Object);

            var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            MockUtils.VerifyAllMocks(contentApiClient);

            Assert.NotNull(viewModel);
            Assert.Equal(page, viewModel.Paging.Page);
            Assert.Equal(pageSize, viewModel.Paging.PageSize);
            Assert.Equal(numberOfAvailableDataSetVersions, viewModel.Paging.TotalResults);
            Assert.Equal(pagedDataSetVersions.Count, viewModel.Results.Count);
        }

        [Fact]
        public async Task MultipleAvailableVersionsForRequestedDataSet_Returns200_OrderedCorrectly()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .ForIndex(0, dsv => dsv.SetVersionNumber(1, 0))
                .ForIndex(1, dsv => dsv.SetVersionNumber(1, 1))
                .ForIndex(2, dsv => dsv.SetVersionNumber(3, 1))
                .ForIndex(3, dsv => dsv.SetVersionNumber(3, 0))
                .ForIndex(4, dsv => dsv.SetVersionNumber(2, 0))
                .ForIndex(5, dsv => dsv.SetVersionNumber(2, 1))
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersions);
            });

            var releaseFiles = dataSetVersions
                .OrderByDescending(dsv => dsv.VersionMajor)
                .ThenByDescending(dsv => dsv.VersionMinor)
                .Select(dsv => DefaultReleaseFileViewModel()
                    .ForInstance(s => s.Set(rf => rf.Id, dsv.ReleaseFileId))
                    .Generate())
                .ToList();

            var releaseFileIds = releaseFiles
                .Select(rf => rf.Id)
                .ToHashSet();

            var contentApiClient = new Mock<IContentApiClient>(MockBehavior.Strict);

            contentApiClient
                .Setup(c => c.ListReleaseFiles(
                    It.Is<ReleaseFileListRequest>(req =>
                        req.Ids.All(id => releaseFileIds.Contains(id))),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync( releaseFiles[..3]);

            var page1Response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 3,
                contentApiClient: contentApiClient.Object);

            var page1ViewModel = page1Response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.Equal(3, page1ViewModel.Results.Count);
            Assert.Equal("3.1", page1ViewModel.Results[0].Version);
            Assert.Equal("3.0", page1ViewModel.Results[1].Version);
            Assert.Equal("2.1", page1ViewModel.Results[2].Version);

            contentApiClient
                .Setup(c => c.ListReleaseFiles(
                    It.Is<ReleaseFileListRequest>(req =>
                        req.Ids.All(id => releaseFileIds.Contains(id))),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(releaseFiles[3..6]);

            var page2Response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 2,
                pageSize: 3,
                contentApiClient: contentApiClient.Object);

            var page2ViewModel = page2Response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.Equal(3, page2ViewModel.Results.Count);
            Assert.Equal("2.0", page2ViewModel.Results[0].Version);
            Assert.Equal("1.1", page2ViewModel.Results[1].Version);
            Assert.Equal("1.0", page2ViewModel.Results[2].Version);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Withdrawn)]
        [InlineData(DataSetVersionStatus.Deprecated)]
        public async Task DataSetVersionIsAvailable_Returns200_CorrectViewModel(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersionGenerator = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatus(dataSetVersionStatus)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithDataSetId(dataSet.Id);

            DataSetVersion dataSetVersion = dataSetVersionStatus == DataSetVersionStatus.Withdrawn
                ? dataSetVersionGenerator.WithWithdrawn(DateTimeOffset.UtcNow)
                : dataSetVersionGenerator;

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var releaseFiles = DefaultReleaseFileViewModel()
                .ForInstance(s => s.Set(rf => rf.Id, dataSetVersion.ReleaseFileId))
                .GenerateList(1);

            var releaseFileIds = releaseFiles
                .Select(rf => rf.Id)
                .ToHashSet();

            var contentApiClient = new Mock<IContentApiClient>(MockBehavior.Strict);

            contentApiClient
                .Setup(c => c.ListReleaseFiles(
                    It.Is<ReleaseFileListRequest>(req =>
                        req.Ids.All(id => releaseFileIds.Contains(id))),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(releaseFiles);

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 1,
                contentApiClient: contentApiClient.Object);

            var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(viewModel);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(1, viewModel.Paging.PageSize);
            Assert.Equal(1, viewModel.Paging.TotalResults);

            var result = Assert.Single(viewModel.Results);

            Assert.Equal(dataSetVersion.Version, result.Version);
            Assert.Equal(dataSetVersion.VersionType, result.Type);
            Assert.Equal(dataSetVersion.Status, result.Status);
            Assert.Equal(
                dataSetVersion.Published.TruncateNanoseconds(),
                result.Published
            );
            if (dataSetVersionStatus == DataSetVersionStatus.Withdrawn)
            {
                Assert.Equal(
                    dataSetVersion.Withdrawn.TruncateNanoseconds(),
                    result.Withdrawn
                );
            }
            Assert.Equal(dataSetVersion.Notes, result.Notes);
            Assert.Equal(dataSetVersion.TotalResults, result.TotalResults);

            Assert.Equal(releaseFiles[0].DataSetFileId, result.File.Id);

            Assert.Equal(releaseFiles[0].Release.Title, result.Release.Title);
            Assert.Equal(releaseFiles[0].Release.Slug, result.Release.Slug);

            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary!.TimePeriodRange.Start.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Code),
                result.TimePeriods.Start);
            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Code),
                result.TimePeriods.End);
            Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, result.GeographicLevels);
            Assert.Equal(dataSetVersion.MetaSummary.Filters, result.Filters);
            Assert.Equal(dataSetVersion.MetaSummary.Indicators, result.Indicators);
        }

        [Fact]
        public async Task AvailableVersionForOtherDataSet_Returns200_OnlyVersionForRequestedDataSet()
        {
            DataSet dataSet1 = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            DataSet dataSet2 = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSets.AddRange(dataSet1, dataSet2));

            DataSetVersion dataSet1Version = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatusPublished()
                .WithVersionNumber(1, 1)
                .WithDataSetId(dataSet1.Id);

            DataSetVersion dataSet2Version = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatusPublished()
                .WithVersionNumber(2, 2)
                .WithDataSetId(dataSet2.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.AddRange(dataSet1Version, dataSet2Version));

            var releaseFiles = DefaultReleaseFileViewModel()
                .ForInstance(s => s.Set(rf => rf.Id, dataSet1Version.ReleaseFileId))
                .GenerateList(1);

            var releaseFileIds = releaseFiles
                .Select(rf => rf.Id)
                .ToHashSet();

            var contentApiClient = new Mock<IContentApiClient>(MockBehavior.Strict);

            contentApiClient
                .Setup(c => c.ListReleaseFiles(
                    It.Is<ReleaseFileListRequest>(req =>
                        req.Ids.All(id => releaseFileIds.Contains(id))),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(releaseFiles);

            var response = await ListDataSetVersions(
                dataSetId: dataSet1.Id,
                page: 1,
                pageSize: 1,
                contentApiClient: contentApiClient.Object);

            var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(viewModel);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(1, viewModel.Paging.PageSize);
            Assert.Equal(1, viewModel.Paging.TotalResults);
            var result = Assert.Single(viewModel.Results);
            Assert.Equal(dataSet1Version.Version, result.Version);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Processing)]
        [InlineData(DataSetVersionStatus.Failed)]
        [InlineData(DataSetVersionStatus.Mapping)]
        [InlineData(DataSetVersionStatus.Draft)]
        public async Task DataSetVersionUnavailable_Returns200_EmptyList(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 1);

            var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(viewModel);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(1, viewModel.Paging.PageSize);
            Assert.Equal(0, viewModel.Paging.TotalResults);
            Assert.Empty(viewModel.Results);
        }

        [Fact]
        public async Task NoDataSetVersions_Returns200_EmptyList()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 1);

            var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(viewModel);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(1, viewModel.Paging.PageSize);
            Assert.Equal(0, viewModel.Paging.TotalResults);
            Assert.Empty(viewModel.Results);
        }

        [Fact]
        public async Task ReleaseFilesDoNotExist_Returns500()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatus(DataSetVersionStatus.Published)
                .WithDataSetId(dataSet.Id)
                .GenerateList(2);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.AddRange(dataSetVersions));

            var contentApiClient = new Mock<IContentApiClient>(MockBehavior.Strict);

            contentApiClient
                .Setup(c => c.ListReleaseFiles(
                    It.IsAny<ReleaseFileListRequest>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync([]);

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 1,
                contentApiClient: contentApiClient.Object);

            response.AssertInternalServerError();
        }

        [Fact]
        public async Task ContentApiClientThrows_Returns500()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatus(DataSetVersionStatus.Published)
                .WithDataSetId(dataSet.Id)
                .GenerateList(2);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.AddRange(dataSetVersions));

            var contentApiClient = new Mock<IContentApiClient>(MockBehavior.Strict);

            contentApiClient
                .Setup(c => c.ListReleaseFiles(
                    It.IsAny<ReleaseFileListRequest>(),
                    It.IsAny<CancellationToken>()
                ))
                .ThrowsAsync(new Exception("Something went wrong"));

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 1,
                contentApiClient: contentApiClient.Object);

            response.AssertInternalServerError();
        }

        [Fact]
        public async Task PageTooBig_Returns200_EmptyList()
        {
            var page = 2;
            var pageSize = 2;
            var numberOfDataSetVersions = 2;

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .GenerateList(numberOfDataSetVersions);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.AddRange(dataSetVersions));

            var response = await ListDataSetVersions(
                dataSetId: dataSet.Id,
                page: page,
                pageSize: pageSize);

            var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(viewModel);
            Assert.Equal(page, viewModel.Paging.Page);
            Assert.Equal(pageSize, viewModel.Paging.PageSize);
            Assert.Equal(numberOfDataSetVersions, viewModel.Paging.TotalResults);
            Assert.Empty(viewModel.Results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task PageTooSmall_Returns400(int page)
        {
            var response = await ListDataSetVersions(
                dataSetId: Guid.NewGuid(),
                page: page,
                pageSize: 1);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasGreaterThanOrEqualError("page", comparisonValue: 1);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(21)]
        public async Task PageSizeOutOfBounds_Returns400(int pageSize)
        {
            var response = await ListDataSetVersions(
                dataSetId: Guid.NewGuid(),
                page: 1,
                pageSize: pageSize);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasInclusiveBetweenError("pageSize", from: 1, to: 20);
        }

        [Theory]
        [InlineData(DataSetStatus.Draft)]
        [InlineData(DataSetStatus.Withdrawn)]
        public async Task UnavailableDataSet_Returns503(DataSetStatus status)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(status);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await ListDataSetVersions(dataSetId: dataSet.Id);

            response.AssertForbidden();
        }

        [Fact]
        public async Task InvalidDataSetId_Returns404()
        {
            var client = TestApp.CreateClient();

            var query = new Dictionary<string, string?>
            {
                { "page", "1" },
                { "pageSize", "1" },
            };

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/not_a_valid_guid/versions", query);

            var response = await client.GetAsync(uri);

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> ListDataSetVersions(
            Guid dataSetId,
            int? page = null,
            int? pageSize = null,
            IContentApiClient? contentApiClient = null)
        {
            var query = new Dictionary<string, string?>
            {
                { "page", page?.ToString() },
                { "pageSize", pageSize?.ToString() },
            };

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSetId}/versions", query);

            var client = BuildApp(contentApiClient).CreateClient();

            return await client.GetAsync(uri);
        }
    }

    public class GetDataSetVersionTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        [Theory]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Withdrawn)]
        [InlineData(DataSetVersionStatus.Deprecated)]
        public async Task VersionIsAvailable_Returns200(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersionGenerator = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                .WithStatus(dataSetVersionStatus)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithDataSetId(dataSet.Id);
            
            DataSetVersion dataSetVersion = dataSetVersionStatus == DataSetVersionStatus.Withdrawn
                ? dataSetVersionGenerator.WithWithdrawn(DateTimeOffset.UtcNow)
                : dataSetVersionGenerator;

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var releaseFiles = DefaultReleaseFileViewModel()
                .GenerateList(1);

            var contentApiClient = new Mock<IContentApiClient>(MockBehavior.Strict);

            contentApiClient
                .Setup(c => c.ListReleaseFiles(
                    It.Is<ReleaseFileListRequest>(req => req.Ids.Contains(dataSetVersion.ReleaseFileId)),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(releaseFiles);

            var response = await GetDataSetVersion(
                dataSet.Id,
                dataSetVersion.Version,
                contentApiClient.Object
            );

            var viewModel = response.AssertOk<DataSetVersionViewModel>(useSystemJson: true);

            MockUtils.VerifyAllMocks(contentApiClient);

            Assert.NotNull(viewModel);
            Assert.Equal(dataSetVersion.Version, viewModel.Version);
            Assert.Equal(dataSetVersion.VersionType, viewModel.Type);
            Assert.Equal(dataSetVersion.Status, viewModel.Status);
            Assert.Equal(
                dataSetVersion.Published.TruncateNanoseconds(),
                viewModel.Published
            );
            if (dataSetVersionStatus == DataSetVersionStatus.Withdrawn)
            {
                Assert.Equal(
                    dataSetVersion.Withdrawn.TruncateNanoseconds(),
                    viewModel.Withdrawn
                );
            }
            Assert.Equal(dataSetVersion.Notes, viewModel.Notes);
            Assert.Equal(dataSetVersion.TotalResults, viewModel.TotalResults);

            Assert.Equal(releaseFiles[0].DataSetFileId, viewModel.File.Id);

            Assert.Equal(releaseFiles[0].Release.Title, viewModel.Release.Title);
            Assert.Equal(releaseFiles[0].Release.Slug, viewModel.Release.Slug);

            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary!.TimePeriodRange.Start.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Code),
                viewModel.TimePeriods.Start);
            Assert.Equal(
                TimePeriodFormatter.FormatLabel(
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Code),
                viewModel.TimePeriods.End);
            Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, viewModel.GeographicLevels);
            Assert.Equal(dataSetVersion.MetaSummary.Filters, viewModel.Filters);
            Assert.Equal(dataSetVersion.MetaSummary.Indicators, viewModel.Indicators);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Processing)]
        [InlineData(DataSetVersionStatus.Failed)]
        [InlineData(DataSetVersionStatus.Mapping)]
        [InlineData(DataSetVersionStatus.Draft)]
        public async Task VersionNotAvailable_Returns403(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersion(dataSet.Id, dataSetVersion.Version);

            response.AssertForbidden();
        }

        [Fact]
        public async Task VersionExistsForOtherDataSet_Returns404()
        {
            DataSet dataSet1 = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            DataSet dataSet2 = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(dataSet1, dataSet2));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatusPublished()
                .WithDataSetId(dataSet1.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersion(dataSet2.Id, dataSetVersion.Version);

            response.AssertNotFound();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetDataSetVersion(dataSet.Id, "1.0");

            response.AssertNotFound();
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersion(Guid.NewGuid(), dataSetVersion.Version);

            response.AssertNotFound();
        }

        [Fact]
        public async Task ReleaseFileDoesNotExist_Returns500()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var contentApiClient = new Mock<IContentApiClient>(MockBehavior.Strict);

            contentApiClient
                .Setup(c => c.ListReleaseFiles(
                    It.Is<ReleaseFileListRequest>(req => req.Ids.Contains(dataSetVersion.ReleaseFileId)),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync([]);

            var response = await GetDataSetVersion(dataSet.Id, dataSetVersion.Version, contentApiClient.Object);

            response.AssertInternalServerError();
        }

        [Fact]
        public async Task ContentApiClientThrows_Returns500()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var contentApiClient = new Mock<IContentApiClient>(MockBehavior.Strict);

            contentApiClient
                .Setup(c => c.ListReleaseFiles(
                    It.Is<ReleaseFileListRequest>(req => req.Ids.Contains(dataSetVersion.ReleaseFileId)),
                    It.IsAny<CancellationToken>()
                ))
                .ThrowsAsync(new Exception("Something went wrong"));

            var response = await GetDataSetVersion(dataSet.Id, dataSetVersion.Version, contentApiClient.Object);

            response.AssertInternalServerError();
        }

        private async Task<HttpResponseMessage> GetDataSetVersion(
            Guid dataSetId,
            string dataSetVersion,
            IContentApiClient? contentApiClient = null)
        {
            var client = BuildApp(contentApiClient).CreateClient();

            var uri = new Uri($"{BaseUrl}/{dataSetId}/versions/{dataSetVersion}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class GetDataSetMetaTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        public class NoQueryParametersTests(TestApplicationFactory testApp) : GetDataSetMetaTests(testApp)
        {
            [Fact]
            public async Task ReturnsCorrectViewModel()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var filterMetas = DataFixture
                    .DefaultFilterMeta()
                    .WithOptions(() => DataFixture
                        .DefaultFilterOptionMeta()
                        .GenerateList(3))
                    .GenerateList(3);

                var allLocationOptionMetaTypesGeneratorByLevel = new Dictionary<GeographicLevel, Func<LocationOptionMeta>>
                {
                    { GeographicLevel.School, () => DataFixture.DefaultLocationSchoolOptionMeta() },
                    { GeographicLevel.LocalAuthority, () => DataFixture.DefaultLocationLocalAuthorityOptionMeta() },
                    { GeographicLevel.RscRegion, () => DataFixture.DefaultLocationRscRegionOptionMeta() },
                    { GeographicLevel.Provider, () => DataFixture.DefaultLocationProviderOptionMeta() },
                    { GeographicLevel.EnglishDevolvedArea, () => DataFixture.DefaultLocationCodedOptionMeta() },
                };

                var locationMetas = allLocationOptionMetaTypesGeneratorByLevel
                    .Select(locationOptionMetaGenerator => DataFixture
                        .DefaultLocationMeta()
                        .WithOptions(() => new List<LocationOptionMeta>
                        {
                            locationOptionMetaGenerator.Value.Invoke(),
                            locationOptionMetaGenerator.Value.Invoke(),
                            locationOptionMetaGenerator.Value.Invoke()
                        })
                        .WithLevel(locationOptionMetaGenerator.Key))
                    .Select(locationMeta => (LocationMeta)locationMeta)
                    .ToList();

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .WithFilterMetas(() => filterMetas)
                    .WithLocationMetas(() => locationMetas)
                    .WithGeographicLevelMeta()
                    .WithIndicatorMetas(() =>
                        DataFixture
                        .DefaultIndicatorMeta()
                        .GenerateList(3)
                    )
                    .WithTimePeriodMetas(() =>
                        DataFixture
                        .DefaultTimePeriodMeta()
                        .GenerateList(3)
                    )
                    .WithMetaSummary()
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(dataSet.Id);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                foreach (var filterMetaViewModel in content.Filters)
                {
                    var filterMeta = Assert.Single(dataSetVersion.FilterMetas, fm => fm.PublicId == filterMetaViewModel.Id);
                    Assert.Equal(filterMeta.Hint, filterMetaViewModel.Hint);
                    Assert.Equal(filterMeta.Label, filterMetaViewModel.Label);

                    var allFilterMetaLinks = filterMeta.Options
                        .SelectMany(o => o.MetaLinks)
                        .ToList();

                    foreach (var filterOptionMetaViewModel in filterMetaViewModel.Options)
                    {
                        var filterOptionMetaLink = Assert.Single(
                            allFilterMetaLinks,
                            link => link.PublicId == filterOptionMetaViewModel.Id);

                        var filterOptionMeta = Assert.Single(
                            filterMeta.Options,
                            o => o.Id == filterOptionMetaLink.OptionId);

                        Assert.Equal(filterOptionMeta.Label, filterOptionMetaViewModel.Label);
                        Assert.Equal(filterOptionMeta.IsAggregate, filterOptionMetaViewModel.IsAggregate);
                    }
                }

                foreach (var locationMetaViewModel in content.Locations)
                {
                    var locationMeta = Assert.Single(
                        dataSetVersion.LocationMetas,
                        fm => fm.Level == locationMetaViewModel.Level);

                    Assert.Equal(locationMeta.Level.GetEnumLabel(), locationMetaViewModel.Label);

                    foreach (var locationOptionMetaViewModel in locationMetaViewModel.Options)
                    {
                        var locationOptionMeta = Assert.Single(
                            locationMeta.Options,
                            o => o.PublicId == locationOptionMetaViewModel.Id);

                        switch (locationOptionMeta)
                        {
                            case LocationCodedOptionMeta codedMeta:
                                var codedViewModel =
                                    Assert.IsType<LocationCodedOptionMetaViewModel>(locationOptionMetaViewModel);
                                Assert.Equal(codedMeta.Label, codedViewModel.Label);
                                Assert.Equal(codedMeta.Code, codedViewModel.Code);
                                break;
                            case LocationLocalAuthorityOptionMeta localAuthorityMeta:
                                var localAuthorityViewModel =
                                    Assert.IsType<LocationLocalAuthorityOptionMetaViewModel>(locationOptionMetaViewModel);
                                Assert.Equal(localAuthorityMeta.Label, localAuthorityViewModel.Label);
                                Assert.Equal(localAuthorityMeta.Code, localAuthorityViewModel.Code);
                                Assert.Equal(localAuthorityMeta.OldCode, localAuthorityViewModel.OldCode);
                                break;
                            case LocationProviderOptionMeta providerMeta:
                                var providerViewModel =
                                    Assert.IsType<LocationProviderOptionMetaViewModel>(locationOptionMetaViewModel);
                                Assert.Equal(providerMeta.Label, providerViewModel.Label);
                                Assert.Equal(providerMeta.Ukprn, providerViewModel.Ukprn);
                                break;
                            case LocationRscRegionOptionMeta rscRegionMeta:
                                var rscRegionViewModel =
                                    Assert.IsType<LocationRscRegionOptionMetaViewModel>(locationOptionMetaViewModel);
                                Assert.Equal(rscRegionMeta.Label, rscRegionViewModel.Label);
                                break;
                            case LocationSchoolOptionMeta schoolMeta:
                                var schoolViewModel =
                                    Assert.IsType<LocationSchoolOptionMetaViewModel>(locationOptionMetaViewModel);
                                Assert.Equal(schoolMeta.Label, schoolViewModel.Label);
                                Assert.Equal(schoolMeta.Urn, schoolViewModel.Urn);
                                Assert.Equal(schoolMeta.LaEstab, schoolViewModel.LaEstab);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }

                Assert.All(
                    content.GeographicLevels,
                    level => Assert.Equal(level.Level.GetEnumLabel(), level.Label)
                );

                foreach (var indicator in content.Indicators)
                {
                    var indicatorMeta = Assert.Single(dataSetVersion.IndicatorMetas, im => im.PublicId == indicator.Id);

                    Assert.Equal(indicatorMeta.Label, indicator.Label);
                    Assert.Equal(indicatorMeta.Unit, indicator.Unit);
                    Assert.Equal(indicatorMeta.DecimalPlaces, indicator.DecimalPlaces);
                }

                foreach (var timePeriod in content.TimePeriods)
                {
                    var timePeriodMeta = Assert.Single(
                        dataSetVersion.TimePeriodMetas,
                        tp => tp.Code == timePeriod.Code
                        && tp.Period == timePeriod.Period);

                    Assert.Equal(
                        TimePeriodFormatter.FormatLabel(timePeriodMeta.Period, timePeriodMeta.Code),
                        timePeriod.Label);
                }
            }                   

            [Theory]
            [InlineData(DataSetVersionStatus.Published)]
            [InlineData(DataSetVersionStatus.Withdrawn)]
            [InlineData(DataSetVersionStatus.Deprecated)]
            public async Task VersionAvailable_Returns200(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 3)
                    .WithStatus(dataSetVersionStatus)
                    .WithPublished(DateTimeOffset.UtcNow)
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(dataSet.Id);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);
            }

            [Theory]
            [InlineData(DataSetVersionStatus.Processing)]
            [InlineData(DataSetVersionStatus.Failed)]
            [InlineData(DataSetVersionStatus.Mapping)]
            [InlineData(DataSetVersionStatus.Draft)]
            public async Task VersionNotAvailable_Returns403(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 3)
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(dataSet.Id);

                response.AssertForbidden();
            }           

            [Fact]
            public async Task DataSetDoesNotExist_Returns404()
            {
                var response = await GetDataSetMeta(Guid.NewGuid());

                response.AssertNotFound();
            }
        }

        public class DataSetVersionQueryParameterTests(TestApplicationFactory testApp) : GetDataSetMetaTests(testApp)
        {
            [Fact]
            public async Task VersionSpecified_ReturnsCorrectVersion()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var dataSetVersions = DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .ForIndex(0, dsv => dsv
                        .SetVersionNumber(1, 0)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 1")
                            ]
                        )
                    )
                    .ForIndex(1, dsv => dsv
                        .SetVersionNumber(1, 1)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 2")
                            ]
                        )
                    )
                    .ForIndex(2, dsv => dsv
                        .SetVersionNumber(2, 0)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 3")
                            ]
                        )
                    )
                    .ForIndex(3, dsv => dsv
                        .SetVersionNumber(2, 1)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 4")
                            ]
                        )
                    )
                    .GenerateList();

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                    context.DataSetVersions.AddRange(dataSetVersions));

                var response = await GetDataSetMeta(dataSet.Id, "2.0");

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);
                Assert.Equal("filter 3", content.Filters.Single().Label);
            }

            [Fact]
            public async Task VersionUnspecified_ReturnsLatestVersion()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var dataSetVersions = DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .ForIndex(0, dsv => dsv
                        .SetVersionNumber(1, 0)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 1")
                            ]
                        )
                    )
                    .ForIndex(1, dsv => dsv
                        .SetVersionNumber(1, 1)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 2")
                            ]
                        )
                    )
                    .ForIndex(2, dsv => dsv
                        .SetVersionNumber(2, 0)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 3")
                            ]
                        )
                    )
                    .ForIndex(3, dsv => dsv
                        .SetVersionNumber(2, 1)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                            .DefaultFilterMeta()
                            .WithLabel("filter 4")
                            ]
                        )
                    )
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv)
                    .GenerateList();

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersions);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(dataSet.Id);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);
                Assert.Equal("filter 4", content.Filters.Single().Label);
            }

            [Fact]
            public async Task VersionExistsForOtherDataSet_Returns404()
            {
                DataSet dataSet1 = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                DataSet dataSet2 = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(dataSet1, dataSet2));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet1.Id);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await GetDataSetMeta(dataSet2.Id, dataSetVersion.Version);

                response.AssertNotFound();
            }

            [Fact]
            public async Task VersionDoesNotExist_Returns404()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var response = await GetDataSetMeta(dataSet.Id, "1.0");

                response.AssertNotFound();
            }
        }

        public class TypesQueryParameterTests(TestApplicationFactory testApp) : GetDataSetMetaTests(testApp)
        {
            [Fact]
            public async Task TypesNotSpecified_ReturnsAllMeta()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 2)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(dataSet.Id);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                Assert.NotEmpty(content.Filters);
                Assert.NotEmpty(content.Locations);
                Assert.NotEmpty(content.Indicators);
                Assert.NotEmpty(content.TimePeriods);
            }

            [Theory]
            [InlineData(DataSetMetaType.Filters)]
            [InlineData(DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Indicators)]
            [InlineData(DataSetMetaType.TimePeriods)]
            public async Task OneTypeSpecified_ReturnsOnlySpecifiedMetaType(DataSetMetaType metaType)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 2)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(
                    dataSet.Id,
                    types: [metaType.ToString()]);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                switch (metaType)
                {
                    case DataSetMetaType.Filters:
                        Assert.NotEmpty(content.Filters);
                        Assert.Empty(content.Locations);
                        Assert.Empty(content.Indicators);
                        Assert.Empty(content.TimePeriods);
                        break;
                    case DataSetMetaType.Locations:
                        Assert.Empty(content.Filters);
                        Assert.NotEmpty(content.Locations);
                        Assert.Empty(content.Indicators);
                        Assert.Empty(content.TimePeriods);
                        break;
                    case DataSetMetaType.Indicators:
                        Assert.Empty(content.Filters);
                        Assert.Empty(content.Locations);
                        Assert.NotEmpty(content.Indicators);
                        Assert.Empty(content.TimePeriods);
                        break;
                    case DataSetMetaType.TimePeriods:
                        Assert.Empty(content.Filters);
                        Assert.Empty(content.Locations);
                        Assert.Empty(content.Indicators);
                        Assert.NotEmpty(content.TimePeriods);
                        break;
                }
            }

            [Theory]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations, DataSetMetaType.Indicators)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations, DataSetMetaType.Indicators, DataSetMetaType.TimePeriods)]
            public async Task MultipleTypesSpecified_ReturnsOnlySpecifiedMetaTypes(params DataSetMetaType[] metaTypes)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 2)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(
                    dataSet.Id,
                    types: metaTypes.Select(t => t.ToString()).ToList());

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                var allMetaTypes = EnumUtil.GetEnums<DataSetMetaType>().ToHashSet();
                var specifiedMetaTypes = metaTypes.ToHashSet();
                var unspecifiedMetaTypes = allMetaTypes.Except(specifiedMetaTypes);

                foreach (var type in specifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.NotEmpty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.NotEmpty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.NotEmpty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.NotEmpty(content.TimePeriods);
                            break;
                    }
                }

                foreach (var type in unspecifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.Empty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.Empty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.Empty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.Empty(content.TimePeriods);
                            break;
                    }
                }
            }

            [Theory]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Filters)]
            [InlineData(DataSetMetaType.Locations, DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Indicators, DataSetMetaType.Indicators)]
            [InlineData(DataSetMetaType.TimePeriods, DataSetMetaType.TimePeriods)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Filters, DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Locations, DataSetMetaType.Locations, DataSetMetaType.Filters)]
            [InlineData(DataSetMetaType.Indicators, DataSetMetaType.Indicators, DataSetMetaType.Filters)]
            [InlineData(DataSetMetaType.TimePeriods, DataSetMetaType.TimePeriods, DataSetMetaType.Filters)]
            public async Task DuplicateTypesSpecified_ReturnsOnlySpecifiedMetaTypes(params DataSetMetaType[] metaTypes)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 2)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(
                    dataSet.Id,
                    types: metaTypes.Select(t => t.ToString()).ToList());

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                var allMetaTypes = EnumUtil.GetEnums<DataSetMetaType>().ToHashSet();
                var specifiedMetaTypes = metaTypes.ToHashSet();
                var unspecifiedMetaTypes = allMetaTypes.Except(specifiedMetaTypes);

                foreach (var type in specifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.NotEmpty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.NotEmpty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.NotEmpty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.NotEmpty(content.TimePeriods);
                            break;
                    }
                }

                foreach (var type in unspecifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.Empty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.Empty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.Empty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.Empty(content.TimePeriods);
                            break;
                    }
                }
            }

            [Theory]
            [InlineData(DataSetMetaType.Filters)]
            [InlineData(DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Indicators)]
            [InlineData(DataSetMetaType.TimePeriods)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations, DataSetMetaType.Indicators)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations, DataSetMetaType.Indicators, DataSetMetaType.TimePeriods)]
            public async Task ArrayQueryParameterSyntax_ReturnsOnlySpecifiedMetaTypes(params DataSetMetaType[] metaTypes)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 2)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var query = metaTypes
                    .Select((mt, index) => new { mt, index })
                    .ToDictionary(a => $"types[{a.index}]", a => a.mt.ToString());

                var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSet.Id}/meta", query!);

                var client = TestApp.CreateClient();

                var response = await client.GetAsync(uri);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                var allMetaTypes = EnumUtil.GetEnums<DataSetMetaType>().ToHashSet();
                var specifiedMetaTypes = metaTypes.ToHashSet();
                var unspecifiedMetaTypes = allMetaTypes.Except(specifiedMetaTypes);

                foreach (var type in specifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.NotEmpty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.NotEmpty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.NotEmpty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.NotEmpty(content.TimePeriods);
                            break;
                    }
                }

                foreach (var type in unspecifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.Empty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.Empty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.Empty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.Empty(content.TimePeriods);
                            break;
                    }
                }
            }

            [Fact]
            public async Task EmptyList_AllowedValueError()
            {
                var response = await GetDataSetMeta(Guid.NewGuid(), types: []);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[0]",
                    value: null,
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());
            }

            [Theory]
            [InlineData("filters", "filters")]
            [InlineData("locations", "locations")]
            [InlineData("indicators", "indicators")]
            [InlineData("timePeriods", "timePeriods")]
            [InlineData("invalid", "invalid")]
            [InlineData(null, "")]
            [InlineData(null, " ")]
            public async Task InvalidMetaType_AllowedValueError(string? invalidType, string metaType)
            {
                var response = await GetDataSetMeta(Guid.NewGuid(), types: [metaType]);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[0]",
                    value: invalidType,
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());
            }

            [Fact]
            public async Task MultipleInvalidMetaTypes_AllowedValueError()
            {
                var response = await GetDataSetMeta(Guid.NewGuid(), types: ["invalid1", "invalid2"]);

                var validationProblem = response.AssertValidationProblem();

                Assert.Equal(2, validationProblem.Errors.Count);

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[0]",
                    value: "invalid1",
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[1]",
                    value: "invalid2",
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());
            }

            [Fact]
            public async Task MixedValidAndInvalidMetaTypes_AllowedValueError()
            {
                var response = await GetDataSetMeta(Guid.NewGuid(), types: [DataSetMetaType.Filters.ToString(), "invalid"]);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[1]",
                    value: "invalid",
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());
            }
        }

        private async Task<HttpResponseMessage> GetDataSetMeta(
            Guid dataSetId,
            string? dataSetVersion = null,
            IReadOnlyList<string>? types = null)
        {
            var query = new Dictionary<string, string?>
            {
                { "dataSetVersion", dataSetVersion },
            };

            if (types is not null)
            {
                query.Add("types", types.JoinToString(","));
            }

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSetId}/meta", query);

            var client = BuildApp().CreateClient();

            return await client.GetAsync(uri);
        }
    }

    private WebApplicationFactory<Startup> BuildApp(IContentApiClient? contentApiClient = null)
    {
        return TestApp.ConfigureServices(services =>
        {
            services.ReplaceService(contentApiClient ?? Mock.Of<IContentApiClient>());
        });
    }

    private Generator<ReleaseFileViewModel> DefaultReleaseFileViewModel() =>
        DataFixture.Generator<ReleaseFileViewModel>()
            .ForInstance(s => s
                .SetDefault(r => r.Id)
                .Set(r => r.File, () => DataFixture.DefaultFileInfo())
                .SetDefault(r => r.DataSetFileId)
                .Set(r => r.Release, () => DefaultReleaseSummaryViewModel())
            );

    private Generator<ReleaseSummaryViewModel> DefaultReleaseSummaryViewModel() =>
        DataFixture.Generator<ReleaseSummaryViewModel>()
            .ForInstance(s => s
                .SetDefault(r => r.Id)
                .SetDefault(r => r.Title)
                .SetDefault(r => r.Slug)
                .Set(r => r.Publication, () => DefaultPublicationSummaryViewModel()));

    private Generator<PublicationSummaryViewModel> DefaultPublicationSummaryViewModel() =>
        DataFixture.Generator<PublicationSummaryViewModel>()
            .ForInstance(s => s
                .SetDefault(r => r.Id)
                .SetDefault(r => r.Title)
                .SetDefault(r => r.Slug));
}
