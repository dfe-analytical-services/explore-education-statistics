using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
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

public abstract class DataSetVersionsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/v1/data-sets";

    public class ListDataSetVersionsTests(TestApplicationFactory testApp) : DataSetVersionsControllerTests(testApp)
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

    public class GetDataSetVersionTests(TestApplicationFactory testApp) : DataSetVersionsControllerTests(testApp)
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
