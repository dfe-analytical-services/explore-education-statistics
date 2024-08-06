using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
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
                { "page", page.ToString() },
                { "pageSize", pageSize.ToString() },
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

    public class GetDataSetVersionChangesTests(TestApplicationFactory testApp) : DataSetVersionsControllerTests(testApp)
    {
        [Fact]
        public async Task VersionAvailable_Returns200_AllChanges()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 3, indicators: 4, locations: 0, timePeriods: 3)
                .WithVersionNumber(1, 0)
                .WithDataSetId(dataSet.Id)
                .WithLocationMetas(DataFixture.DefaultLocationMeta(options: 3)
                    .ForIndex(0, s => s.SetLevel(GeographicLevel.Country))
                    .ForIndex(1, s => s.SetLevel(GeographicLevel.Region))
                    .ForIndex(2, s => s.SetLevel(GeographicLevel.LocalAuthorityDistrict))
                    .GenerateList)
                .WithGeographicLevelMeta();

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 3, indicators: 4, locations: 0, timePeriods: 3)
                .WithVersionNumber(2, 0)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .WithLocationMetas(DataFixture.DefaultLocationMeta(options: 3)
                    .ForIndex(0, s => s.SetLevel(GeographicLevel.Country))
                    .ForIndex(1, s => s.SetLevel(GeographicLevel.Region))
                    .ForIndex(2, s => s.SetLevel(GeographicLevel.OpportunityArea))
                    .GenerateList)
                .WithGeographicLevelMeta();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
            });

            var filterMetaChanges = DataFixture
                .DefaultFilterMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousStateId(oldDataSetVersion.FilterMetas[0].Id))
                .ForIndex(1, s => s
                    .SetCurrentStateId(dataSetVersion.FilterMetas[0].Id))
                .GenerateList();

            var filterOptionMetaChanges = DataFixture
                .DefaultFilterOptionMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousState(oldDataSetVersion.FilterMetas[0].OptionLinks[0]))
                .ForIndex(1, s => s
                    .SetCurrentState(dataSetVersion.FilterMetas[0].OptionLinks[0]))
                .GenerateList();

            var geographicLevelMetaChanges = DataFixture
                .DefaultGeographicLevelMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .WithPreviousStateId(oldDataSetVersion.GeographicLevelMeta!.Id)
                .WithCurrentStateId(dataSetVersion.GeographicLevelMeta!.Id)
                .GenerateList(1);

            var indicatorMetaChanges = DataFixture
                .DefaultIndicatorMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousStateId(oldDataSetVersion.IndicatorMetas[0].Id))
                .ForIndex(1, s => s
                    .SetCurrentStateId(dataSetVersion.IndicatorMetas[0].Id))
                .GenerateList();

            var locationMetaChanges = DataFixture
                .DefaultLocationMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousStateId(oldDataSetVersion.LocationMetas[2].Id))
                .ForIndex(1, s => s
                    .SetCurrentStateId(dataSetVersion.LocationMetas[2].Id))
                .GenerateList();

            var locationOptionMetaChanges = DataFixture
                .DefaultLocationOptionMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousState(oldDataSetVersion.LocationMetas[1].OptionLinks[0]))
                .ForIndex(1, s => s
                    .SetCurrentState(dataSetVersion.LocationMetas[1].OptionLinks[0]))
                .GenerateList();

            var timePeriodMetaChanges = DataFixture
                .DefaultTimePeriodMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousStateId(oldDataSetVersion.TimePeriodMetas[0].Id))
                .ForIndex(1, s => s
                    .SetCurrentStateId(dataSetVersion.TimePeriodMetas[0].Id))
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.FilterMetaChanges.AddRange(filterMetaChanges);
                context.FilterOptionMetaChanges.AddRange(filterOptionMetaChanges);
                context.GeographicLevelMetaChanges.AddRange(geographicLevelMetaChanges);
                context.IndicatorMetaChanges.AddRange(indicatorMetaChanges);
                context.LocationMetaChanges.AddRange(locationMetaChanges);
                context.LocationOptionMetaChanges.AddRange(locationOptionMetaChanges);
                context.TimePeriodMetaChanges.AddRange(timePeriodMetaChanges);
            });

            var response = await GetDataSetVersionChanges(dataSet.Id, dataSetVersion.Version);

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            // We just assert on counts for brevity - more in-depth
            // assertions are performed in other test cases.

            Assert.NotNull(viewModel.MajorChanges);
            Assert.Single(viewModel.MajorChanges.Filters!);
            Assert.Single(viewModel.MajorChanges.FilterOptions!);
            Assert.Single(viewModel.MajorChanges.GeographicLevels!);
            Assert.Single(viewModel.MajorChanges.LocationGroups!);
            Assert.Single(viewModel.MajorChanges.LocationOptions!);
            Assert.Single(viewModel.MajorChanges.TimePeriods!);

            Assert.NotNull(viewModel.MinorChanges);
            Assert.Single(viewModel.MinorChanges.Filters!);
            Assert.Single(viewModel.MinorChanges.FilterOptions!);
            Assert.Single(viewModel.MinorChanges.GeographicLevels!);
            Assert.Single(viewModel.MinorChanges.Indicators!);
            Assert.Single(viewModel.MinorChanges.LocationGroups!);
            Assert.Single(viewModel.MinorChanges.LocationOptions!);
            Assert.Single(viewModel.MinorChanges.TimePeriods!);
        }

        [Fact]
        public async Task OnlyFilterChanges_Returns200()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 3, indicators: 0, locations: 0, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished();

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 0, indicators: 0, locations: 0, timePeriods: 2)
                .WithVersionNumber(major: 2, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithFilterMetas(() => DataFixture.DefaultFilterMeta(options: 3)
                    .ForIndex(2, s => s.SetPublicId(oldDataSetVersion.FilterMetas[2].PublicId))
                    .GenerateList(3));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
            });

            var filterMetaChanges = DataFixture
                .DefaultFilterMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousStateId(oldDataSetVersion.FilterMetas[0].Id))
                .ForIndex(1, s => s
                    .SetCurrentStateId(dataSetVersion.FilterMetas[0].Id))
                .ForIndex(2, s => s
                    .SetPreviousStateId(oldDataSetVersion.FilterMetas[1].Id)
                    .SetCurrentStateId(dataSetVersion.FilterMetas[1].Id))
                .ForIndex(3, s => s
                    .SetPreviousStateId(oldDataSetVersion.FilterMetas[2].Id)
                    .SetCurrentStateId(dataSetVersion.FilterMetas[2].Id))
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.FilterMetaChanges.AddRange(filterMetaChanges);
            });

            var response = await GetDataSetVersionChanges(dataSet.Id, dataSetVersion.Version);

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            var majorChanges = viewModel.MajorChanges.Filters;

            Assert.NotNull(majorChanges);
            Assert.Equal(2, majorChanges.Count);

            // Deletion
            Assert.Null(majorChanges[0].CurrentState);
            Assert.NotNull(majorChanges[0].PreviousState);
            Assert.Equal(oldDataSetVersion.FilterMetas[0].PublicId, majorChanges[0].PreviousState!.Id);

            // Updated ID
            Assert.NotNull(majorChanges[1].PreviousState);
            Assert.Equal(oldDataSetVersion.FilterMetas[1].PublicId, majorChanges[1].PreviousState!.Id);

            Assert.NotNull(majorChanges[1].CurrentState);
            Assert.Equal(dataSetVersion.FilterMetas[1].PublicId, majorChanges[1].CurrentState!.Id);

            var minorChanges = viewModel.MinorChanges.Filters;

            Assert.NotNull(minorChanges);
            Assert.Equal(2, minorChanges.Count);

            // Addition
            Assert.Null(minorChanges[0].PreviousState);
            Assert.NotNull(minorChanges[0].CurrentState);
            Assert.Equal(dataSetVersion.FilterMetas[0].PublicId, minorChanges[0].CurrentState!.Id);

            // Updated label
            Assert.NotNull(minorChanges[1].PreviousState);
            Assert.Equal(dataSetVersion.FilterMetas[2].PublicId, minorChanges[1].PreviousState!.Id);

            Assert.NotNull(minorChanges[1].CurrentState);
            Assert.Equal(dataSetVersion.FilterMetas[2].PublicId, minorChanges[1].CurrentState!.Id);

            Assert.Equal(minorChanges[1].PreviousState!.Id, minorChanges[1].CurrentState!.Id);
        }

        [Fact]
        public async Task OnlyFilterOptionChanges_Returns200()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithFilterMetas(() => DataFixture.DefaultFilterMeta(options: 3).GenerateList(1));

            var oldOptionLink1 = oldDataSetVersion.FilterMetas[0].OptionLinks[0];
            var oldOptionLink2 = oldDataSetVersion.FilterMetas[0].OptionLinks[1];
            var oldOptionLink3 = oldDataSetVersion.FilterMetas[0].OptionLinks[2];

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 2, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithFilterMetas(() => DataFixture.DefaultFilterMeta(options: 0)
                    .WithOptionLinks(() => DataFixture.DefaultFilterOptionMetaLink()
                        .WithOption(() => DataFixture.DefaultFilterOptionMeta())
                        // Simulates only ID being changed - major
                        .ForIndex(1, l => l
                            .Set((_, link) => link.Option.Label = oldOptionLink3.Option.Label))
                        // Simulates only label being changed - minor
                        .ForIndex(2, l => l
                            .SetPublicId(oldOptionLink3.PublicId))
                        .GenerateList(3))
                    .GenerateList(1));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
            });

            var newOptionLink1 = dataSetVersion.FilterMetas[0].OptionLinks[0];
            var newOptionLink2 = dataSetVersion.FilterMetas[0].OptionLinks[1];
            var newOptionLink3 = dataSetVersion.FilterMetas[0].OptionLinks[2];

            var filterOptionMetaChanges = DataFixture
                .DefaultFilterOptionMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousState(oldOptionLink1))
                .ForIndex(1, s => s
                    .SetCurrentState(newOptionLink1))
                .ForIndex(2, s => s
                    .SetPreviousState(oldOptionLink2)
                    .SetCurrentState(newOptionLink2))
                .ForIndex(3, s => s
                    .SetPreviousState(oldOptionLink3)
                    .SetCurrentState(newOptionLink3))
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.FilterOptionMetaChanges.AddRange(filterOptionMetaChanges);
            });

            var response = await GetDataSetVersionChanges(dataSet.Id, dataSetVersion.Version);

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            var majorChanges = viewModel.MajorChanges.FilterOptions;

            Assert.NotNull(majorChanges);
            Assert.Equal(2, majorChanges.Count);

            Assert.Equal(oldDataSetVersion.FilterMetas[0].PublicId, majorChanges[0].Filter.Id);
            Assert.Equal(oldDataSetVersion.FilterMetas[0].Label, majorChanges[0].Filter.Label);

            var majorChange1Options = majorChanges[0].Options;

            Assert.Single(majorChange1Options);

            // Deletion
            Assert.Null(majorChange1Options[0].CurrentState);
            Assert.NotNull(majorChange1Options[0].PreviousState);
            Assert.Equal(oldOptionLink1.PublicId, majorChange1Options[0].PreviousState!.Id);

            Assert.Equal(dataSetVersion.FilterMetas[0].PublicId, majorChanges[1].Filter.Id);
            Assert.Equal(dataSetVersion.FilterMetas[0].Label, majorChanges[1].Filter.Label);

            var majorChange2Options = majorChanges[1].Options;

            Assert.Single(majorChange2Options);

            // Updated ID
            Assert.NotNull(majorChange2Options[0].PreviousState);
            Assert.Equal(oldOptionLink2.PublicId, majorChange2Options[0].PreviousState!.Id);

            Assert.NotNull(majorChange2Options[0].CurrentState);
            Assert.Equal(newOptionLink2.PublicId, majorChange2Options[0].CurrentState!.Id);

            Assert.NotEqual(majorChange2Options[0].PreviousState!.Id, majorChange2Options[0].CurrentState!.Id);
            Assert.NotEqual(majorChange2Options[0].PreviousState!.Label, majorChange2Options[0].CurrentState!.Label);

            var minorChanges = viewModel.MinorChanges.FilterOptions;

            Assert.NotNull(minorChanges);
            Assert.Single(minorChanges);

            Assert.Equal(dataSetVersion.FilterMetas[0].PublicId, minorChanges[0].Filter.Id);
            Assert.Equal(dataSetVersion.FilterMetas[0].Label, minorChanges[0].Filter.Label);

            var minorChangeOptions = minorChanges[0].Options;

            // Addition
            Assert.Null(minorChangeOptions[0].PreviousState);
            Assert.NotNull(minorChangeOptions[0].CurrentState);
            Assert.Equal(newOptionLink1.PublicId, minorChangeOptions[0].CurrentState!.Id);

            // Updated label
            Assert.NotNull(minorChangeOptions[1].PreviousState);
            Assert.Equal(oldOptionLink3.PublicId, minorChangeOptions[1].PreviousState!.Id);
            Assert.Equal(oldOptionLink3.Option.Label, minorChangeOptions[1].PreviousState!.Label);

            Assert.NotNull(minorChangeOptions[1].CurrentState);
            Assert.Equal(newOptionLink3.PublicId, minorChangeOptions[1].CurrentState!.Id);
            Assert.Equal(newOptionLink3.Option.Label, minorChangeOptions[1].CurrentState!.Label);

            Assert.Equal(minorChangeOptions[1].PreviousState!.Id, minorChangeOptions[1].CurrentState!.Id);
            Assert.NotEqual(minorChangeOptions[1].PreviousState!.Label, minorChangeOptions[1].CurrentState!.Label);
        }

        [Fact]
        public async Task OnlyGeographicLevelChanges_Returns200()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithGeographicLevelMeta(() => DataFixture.DefaultGeographicLevelMeta()
                    .WithLevels([GeographicLevel.Country, GeographicLevel.LocalAuthorityDistrict]));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 2, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithGeographicLevelMeta(() => DataFixture.DefaultGeographicLevelMeta()
                    .WithLevels(
                    [
                        GeographicLevel.Country,
                        GeographicLevel.Region,
                        GeographicLevel.LocalAuthority
                    ])
                );

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
            });

            GeographicLevelMetaChange geographicLevelMetaChange = DataFixture
                .DefaultGeographicLevelMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .WithPreviousStateId(oldDataSetVersion.GeographicLevelMeta!.Id)
                .WithCurrentStateId(dataSetVersion.GeographicLevelMeta!.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.GeographicLevelMetaChanges.Add(geographicLevelMetaChange);
            });

            var response = await GetDataSetVersionChanges(dataSet.Id, dataSetVersion.Version);

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            var majorChanges = viewModel.MajorChanges.GeographicLevels;

            Assert.NotNull(majorChanges);
            Assert.Single(majorChanges);

            // Deletion
            Assert.Null(majorChanges[0].CurrentState);
            Assert.NotNull(majorChanges[0].PreviousState);
            Assert.Equal(GeographicLevel.LocalAuthorityDistrict, majorChanges[0].PreviousState!.Level);

            var minorChanges = viewModel.MinorChanges.GeographicLevels;

            Assert.NotNull(minorChanges);
            Assert.Equal(2, minorChanges.Count);

            // Addition
            Assert.Null(minorChanges[0].PreviousState);
            Assert.NotNull(minorChanges[0].CurrentState);
            Assert.Equal(GeographicLevel.Region, minorChanges[0].CurrentState!.Level);

            // Addition
            Assert.Null(minorChanges[1].PreviousState);
            Assert.NotNull(minorChanges[1].CurrentState);
            Assert.Equal(GeographicLevel.LocalAuthority, minorChanges[1].CurrentState!.Level);
        }

        [Fact]
        public async Task OnlyLocationGroupChanges_Returns200()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithLocationMetas(() => DataFixture.DefaultLocationMeta(options: 1)
                    .ForIndex(0, s => s.SetLevel(GeographicLevel.MayoralCombinedAuthority))
                    .ForIndex(1, s => s.SetLevel(GeographicLevel.LocalAuthorityDistrict))
                    .GenerateList());

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 2, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithLocationMetas(() => DataFixture.DefaultLocationMeta(options: 1)
                    .ForIndex(0, s => s.SetLevel(GeographicLevel.Country))
                    .ForIndex(1, s => s.SetLevel(GeographicLevel.LocalAuthority))
                    .GenerateList());

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
            });

            var locationMetaChanges = DataFixture
                .DefaultLocationMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousStateId(oldDataSetVersion.LocationMetas[0].Id))
                .ForIndex(1, s => s
                    .SetCurrentStateId(dataSetVersion.LocationMetas[0].Id))
                .ForIndex(2, s => s
                    .SetPreviousStateId(oldDataSetVersion.LocationMetas[1].Id)
                    .SetCurrentStateId(dataSetVersion.LocationMetas[1].Id))
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.LocationMetaChanges.AddRange(locationMetaChanges);
            });

            var response = await GetDataSetVersionChanges(dataSet.Id, dataSetVersion.Version);

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            var majorChanges = viewModel.MajorChanges.LocationGroups;

            Assert.NotNull(majorChanges);
            Assert.Equal(2, majorChanges.Count);

            // Deletion
            Assert.Null(majorChanges[0].CurrentState);
            Assert.NotNull(majorChanges[0].PreviousState);
            Assert.Equal(oldDataSetVersion.LocationMetas[0].Level, majorChanges[0].PreviousState!.Level);

            // Updated level
            Assert.NotNull(majorChanges[1].PreviousState);
            Assert.Equal(oldDataSetVersion.LocationMetas[1].Level, majorChanges[1].PreviousState!.Level);

            Assert.NotNull(majorChanges[1].CurrentState);
            Assert.Equal(dataSetVersion.LocationMetas[1].Level, majorChanges[1].CurrentState!.Level);

            var minorChanges = viewModel.MinorChanges.LocationGroups;

            Assert.NotNull(minorChanges);
            Assert.Single(minorChanges);

            // Addition
            Assert.Null(minorChanges[0].PreviousState);
            Assert.NotNull(minorChanges[0].CurrentState);
            Assert.Equal(dataSetVersion.LocationMetas[0].Level, minorChanges[0].CurrentState!.Level);
        }

        [Fact]
        public async Task OnlyLocationOptionChanges_Returns200()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithLocationMetas(() => DataFixture.DefaultLocationMeta(options: 3).GenerateList(1));

            var oldOptionLink1 = oldDataSetVersion.LocationMetas[0].OptionLinks[0];
            var oldOptionLink2 = oldDataSetVersion.LocationMetas[0].OptionLinks[1];
            var oldOptionLink3 = oldDataSetVersion.LocationMetas[0].OptionLinks[2];

            var oldOption1 = (oldOptionLink1.Option as LocationCodedOptionMeta)!;
            var oldOption2 = (oldOptionLink2.Option as LocationCodedOptionMeta)!;
            var oldOption3 = (oldOptionLink3.Option as LocationCodedOptionMeta)!;

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 2, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithLocationMetas(() => DataFixture.DefaultLocationMeta()
                    .WithOptionLinks(() => DataFixture.DefaultLocationOptionMetaLink()
                        .WithOption(() => DataFixture.DefaultLocationCodedOptionMeta())
                        // Simulates only code being changed - major
                        .ForIndex(1, l => l
                            .Set((_, link) => link.PublicId = oldOptionLink2.PublicId)
                            .Set((_, link) => link.Option.Label = oldOption2.Label))
                        // Simulates only the label being changed - minor
                        .ForIndex(2, l => l
                            .SetPublicId(oldOptionLink3.PublicId)
                            .Set((_, link) => (link.Option as LocationCodedOptionMeta)!.Code = oldOption3.Code))
                        .GenerateList(3))
                    .GenerateList(1));

            var newOptionLink1 = dataSetVersion.LocationMetas[0].OptionLinks[0];
            var newOptionLink2 = dataSetVersion.LocationMetas[0].OptionLinks[1];
            var newOptionLink3 = dataSetVersion.LocationMetas[0].OptionLinks[2];

            var newOption1 = (newOptionLink1.Option as LocationCodedOptionMeta)!;
            var newOption2 = (newOptionLink2.Option as LocationCodedOptionMeta)!;
            var newOption3 = (newOptionLink3.Option as LocationCodedOptionMeta)!;

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
            });

            var locationOptionMetaChanges = DataFixture
                .DefaultLocationOptionMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousState(oldOptionLink1))
                .ForIndex(1, s => s
                    .SetCurrentState(newOptionLink1))
                .ForIndex(2, s => s
                    .SetPreviousState(oldOptionLink2)
                    .SetCurrentState(newOptionLink2))
                .ForIndex(3, s => s
                    .SetPreviousState(oldOptionLink3)
                    .SetCurrentState(newOptionLink3))
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.LocationOptionMetaChanges.AddRange(locationOptionMetaChanges);
            });

            var response = await GetDataSetVersionChanges(dataSet.Id, dataSetVersion.Version);

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            var majorChanges = viewModel.MajorChanges.LocationOptions;

            Assert.NotNull(majorChanges);
            Assert.Single(majorChanges);

            Assert.Equal(dataSetVersion.LocationMetas[0].Level, majorChanges[0].Group.Level);
            Assert.Equal(dataSetVersion.LocationMetas[0].Level.GetEnumLabel(), majorChanges[0].Group.Label);

            var majorChangeOptions = majorChanges[0].Options;

            // Deletion
            var majorChange1Previous = Assert.IsType<LocationCodedOptionViewModel>(majorChangeOptions[0].PreviousState);

            Assert.Equal(oldOptionLink1.PublicId, majorChange1Previous.Id);
            Assert.Equal(oldOption1.Label, majorChange1Previous.Label);
            Assert.Equal(oldOption1.Code, majorChange1Previous.Code);

            // Updated ID and code
            var majorChange2Previous = Assert.IsType<LocationCodedOptionViewModel>(majorChangeOptions[1].PreviousState);
            var majorChange2Current = Assert.IsType<LocationCodedOptionViewModel>(majorChangeOptions[1].CurrentState);

            Assert.Equal(oldOptionLink2.PublicId, majorChange2Previous.Id);
            Assert.Equal(oldOption2.Label, majorChange2Previous.Label);
            Assert.Equal(oldOption2.Code, majorChange2Previous.Code);

            Assert.Equal(newOptionLink2.PublicId, majorChange2Current.Id);
            Assert.Equal(newOption2.Label, majorChange2Current.Label);
            Assert.Equal(newOption2.Code, majorChange2Current.Code);

            Assert.Equal(majorChange2Previous.Id, majorChange2Current.Id);
            Assert.Equal(majorChange2Previous.Label, majorChange2Current.Label);
            Assert.NotEqual(majorChange2Previous.Code, majorChange2Current.Code);

            var minorChanges = viewModel.MinorChanges.LocationOptions;

            Assert.NotNull(minorChanges);
            Assert.Single(minorChanges);

            Assert.Equal(dataSetVersion.LocationMetas[0].Level, minorChanges[0].Group.Level);
            Assert.Equal(dataSetVersion.LocationMetas[0].Level.GetEnumLabel(), minorChanges[0].Group.Label);

            var minorChangeOptions = minorChanges[0].Options;

            // Addition
            var minorChange1Current = Assert.IsType<LocationCodedOptionViewModel>(minorChangeOptions[0].CurrentState);

            Assert.Equal(newOptionLink1.PublicId, minorChange1Current.Id);
            Assert.Equal(newOption1.Label, minorChange1Current.Label);
            Assert.Equal(newOption1.Code, minorChange1Current.Code);

            // Updated label
            var minorChange2Previous = Assert.IsType<LocationCodedOptionViewModel>(minorChangeOptions[1].PreviousState);
            var minorChange2Current = Assert.IsType<LocationCodedOptionViewModel>(minorChangeOptions[1].CurrentState);

            Assert.Equal(oldOptionLink3.PublicId, minorChange2Previous.Id);
            Assert.Equal(oldOption3.Label, minorChange2Previous.Label);
            Assert.Equal(oldOption3.Code, minorChange2Previous.Code);

            Assert.Equal(newOptionLink3.PublicId, minorChange2Current.Id);
            Assert.Equal(newOption3.Label, minorChange2Current.Label);
            Assert.Equal(newOption3.Code, minorChange2Current.Code);

            Assert.Equal(minorChange2Previous.Id, minorChange2Current.Id);
            Assert.NotEqual(minorChange2Previous.Label, minorChange2Current.Label);
            Assert.Equal(minorChange2Previous.Code, minorChange2Current.Code);
        }

        [Fact]
        public async Task OnlyTimePeriodChanges_Returns200()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithTimePeriodMetas(() => DataFixture.DefaultTimePeriodMeta()
                    .WithCode(TimeIdentifier.CalendarYear)
                    .ForIndex(0, s => s.SetPeriod("2020"))
                    .ForIndex(1, s => s.SetPeriod("2021"))
                    .ForIndex(2, s => s.SetPeriod("2022"))
                    .GenerateList());

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 2, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithTimePeriodMetas(() => DataFixture.DefaultTimePeriodMeta()
                    .WithCode(TimeIdentifier.CalendarYear)
                    .ForIndex(0, s => s.SetPeriod("2021"))
                    .ForIndex(1, s => s.SetPeriod("2022"))
                    .ForIndex(2, s => s.SetPeriod("2023"))
                    .GenerateList());

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
            });

            var timePeriodMetaChanges = DataFixture
                .DefaultTimePeriodMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s
                    .SetPreviousStateId(oldDataSetVersion.TimePeriodMetas[0].Id))
                .ForIndex(1, s => s
                    .SetCurrentStateId(dataSetVersion.TimePeriodMetas[2].Id))
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.TimePeriodMetaChanges.AddRange(timePeriodMetaChanges);
            });

            var response = await GetDataSetVersionChanges(dataSet.Id, dataSetVersion.Version);

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            var majorChanges = viewModel.MajorChanges.TimePeriods;

            Assert.NotNull(majorChanges);
            Assert.Single(majorChanges);

            // Deletion
            Assert.Null(majorChanges[0].CurrentState);
            Assert.NotNull(majorChanges[0].PreviousState);
            Assert.Equal(TimeIdentifier.CalendarYear, majorChanges[0].PreviousState!.Code);
            Assert.Equal("2020", majorChanges[0].PreviousState!.Period);

            var minorChanges = viewModel.MinorChanges.TimePeriods;

            Assert.NotNull(minorChanges);
            Assert.Single(minorChanges);

            // Addition
            Assert.Null(minorChanges[0].PreviousState);
            Assert.NotNull(minorChanges[0].CurrentState);
            Assert.Equal(TimeIdentifier.CalendarYear, minorChanges[0].CurrentState!.Code);
            Assert.Equal("2023", minorChanges[0].CurrentState!.Period);
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

            var response = await GetDataSetVersionChanges(dataSet.Id, dataSetVersion.Version);

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

            var response = await GetDataSetVersionChanges(dataSet2.Id, dataSetVersion.Version);

            response.AssertNotFound();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetDataSetVersionChanges(dataSet.Id, "1.0");

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

            var response = await GetDataSetVersionChanges(Guid.NewGuid(), dataSetVersion.Version);

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSetVersionChanges(
            Guid dataSetId,
            string dataSetVersion,
            IContentApiClient? contentApiClient = null)
        {
            var client = BuildApp(contentApiClient).CreateClient();

            var uri = new Uri($"{BaseUrl}/{dataSetId}/versions/{dataSetVersion}/changes", UriKind.Relative);

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
