using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;
using Microsoft.AspNetCore.WebUtilities;
using Moq;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

// ReSharper disable once ClassNeverInstantiated.Global
public class DataSetVersionsControllerTestsFixture()
    : OptimisedPublicApiCollectionFixture(
        capabilities: [PublicApiIntegrationTestCapability.UserAuth, PublicApiIntegrationTestCapability.Postgres]
    );

[CollectionDefinition(nameof(DataSetVersionsControllerTestsFixture))]
public class DataSetVersionsControllerTestsCollection : ICollectionFixture<DataSetVersionsControllerTestsFixture>;

[Collection(nameof(DataSetVersionsControllerTestsFixture))]
public abstract class DataSetVersionsControllerTests(DataSetVersionsControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private const string BaseUrl = "v1/data-sets";

    private static readonly DataFixture DataFixture = new();

    public abstract class ListDataSetVersionsTests(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        public class PublicDataSetVersionsTests(DataSetVersionsControllerTestsFixture fixture)
            : ListDataSetVersionsTests(fixture)
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
                int numberOfAvailableDataSetVersions
            )
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                var dataSetVersions = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .GenerateList(numberOfAvailableDataSetVersions);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.AddRange(dataSetVersions));

                var pagedDataSetVersions = dataSetVersions
                    .OrderByDescending(dsv => dsv.VersionMajor)
                    .ThenByDescending(dsv => dsv.VersionMinor)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = await ListDataSetVersions(dataSetId: dataSet.Id, page: page, pageSize: pageSize);

                var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

                Assert.NotNull(viewModel);
                Assert.Equal(page, viewModel.Paging.Page);
                Assert.Equal(pageSize, viewModel.Paging.PageSize);
                Assert.Equal(numberOfAvailableDataSetVersions, viewModel.Paging.TotalResults);
                Assert.Equal(pagedDataSetVersions.Count, viewModel.Results.Count);
            }

            [Fact]
            public async Task MultipleAvailableVersionsForRequestedDataSet_Returns200_OrderedCorrectly()
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

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

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context =>
                    {
                        context.DataSetVersions.AddRange(dataSetVersions);
                    });

                var page1Response = await ListDataSetVersions(dataSetId: dataSet.Id, page: 1, pageSize: 3);

                var page1ViewModel = page1Response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

                Assert.Equal(3, page1ViewModel.Results.Count);
                Assert.Equal("3.1", page1ViewModel.Results[0].Version);
                Assert.Equal("3.0", page1ViewModel.Results[1].Version);
                Assert.Equal("2.1", page1ViewModel.Results[2].Version);

                var page2Response = await ListDataSetVersions(dataSetId: dataSet.Id, page: 2, pageSize: 3);

                var page2ViewModel = page2Response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

                Assert.Equal(3, page2ViewModel.Results.Count);
                Assert.Equal("2.0", page2ViewModel.Results[0].Version);
                Assert.Equal("1.1", page2ViewModel.Results[1].Version);
                Assert.Equal("1.0", page2ViewModel.Results[2].Version);
            }

            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusViewTheoryData.AvailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData)
            )]
            public async Task DataSetVersionIsAvailable_Returns200_CorrectViewModel(
                DataSetVersionStatus dataSetVersionStatus
            )
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                var dataSetVersionGenerator = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatus(dataSetVersionStatus)
                    .WithPublished(DateTimeOffset.UtcNow)
                    .WithDataSetId(dataSet.Id);

                DataSetVersion dataSetVersion =
                    dataSetVersionStatus == DataSetVersionStatus.Withdrawn
                        ? dataSetVersionGenerator.WithWithdrawn(DateTimeOffset.UtcNow)
                        : dataSetVersionGenerator;

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await ListDataSetVersions(dataSetId: dataSet.Id, page: 1, pageSize: 1);

                var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

                Assert.NotNull(viewModel);
                Assert.Equal(1, viewModel.Paging.Page);
                Assert.Equal(1, viewModel.Paging.PageSize);
                Assert.Equal(1, viewModel.Paging.TotalResults);

                var result = Assert.Single(viewModel.Results);

                Assert.Equal(dataSetVersion.PublicVersion, result.Version);
                Assert.Equal(dataSetVersion.VersionType, result.Type);
                Assert.Equal(dataSetVersion.Status, result.Status);
                Assert.Equal(dataSetVersion.Published.TruncateNanoseconds(), result.Published);
                if (dataSetVersionStatus == DataSetVersionStatus.Withdrawn)
                {
                    Assert.Equal(dataSetVersion.Withdrawn.TruncateNanoseconds(), result.Withdrawn);
                }

                Assert.Equal(dataSetVersion.Notes, result.Notes);
                Assert.Equal(dataSetVersion.TotalResults, result.TotalResults);

                Assert.Equal(dataSetVersion.Release.DataSetFileId, result.File.Id);

                Assert.Equal(dataSetVersion.Release.Title, result.Release.Title);
                Assert.Equal(dataSetVersion.Release.Slug, result.Release.Slug);

                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(
                        dataSetVersion.MetaSummary!.TimePeriodRange.Start.Period,
                        dataSetVersion.MetaSummary.TimePeriodRange.Start.Code
                    ),
                    result.TimePeriods.Start
                );
                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(
                        dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                        dataSetVersion.MetaSummary.TimePeriodRange.End.Code
                    ),
                    result.TimePeriods.End
                );
                Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, result.GeographicLevels);
                Assert.Equal(dataSetVersion.MetaSummary.Filters, result.Filters);
                Assert.Equal(dataSetVersion.MetaSummary.Indicators, result.Indicators);
            }

            [Fact]
            public async Task AvailableVersionForOtherDataSet_Returns200_OnlyVersionForRequestedDataSet()
            {
                DataSet dataSet1 = DataFixture.DefaultDataSet().WithStatusPublished();

                DataSet dataSet2 = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSets.AddRange(dataSet1, dataSet2));

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

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.AddRange(dataSet1Version, dataSet2Version));

                var response = await ListDataSetVersions(dataSetId: dataSet1.Id, page: 1, pageSize: 1);

                var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

                Assert.NotNull(viewModel);
                Assert.Equal(1, viewModel.Paging.Page);
                Assert.Equal(1, viewModel.Paging.PageSize);
                Assert.Equal(1, viewModel.Paging.TotalResults);
                var result = Assert.Single(viewModel.Results);
                Assert.Equal(dataSet1Version.PublicVersion, result.Version);
            }

            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData)
            )]
            public async Task NoAvailableDataSetVersions_Returns200_EmptyList(DataSetVersionStatus dsvStatus)
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                var dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithDataSetId(dataSet.Id)
                    .WithStatus(dsvStatus)
                    .WithVersionNumber(1, 0)
                    .Generate();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await ListDataSetVersions(dataSetId: dataSet.Id, page: 1, pageSize: 10);

                var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

                Assert.NotNull(viewModel);
                Assert.Equal(0, viewModel.Paging.TotalResults);
            }

            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData)
            )]
            public async Task DataSetVersionUnavailable_Returns200_EmptyList(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSetId(dataSet.Id);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await ListDataSetVersions(dataSetId: dataSet.Id, page: 1, pageSize: 1);

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
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                var response = await ListDataSetVersions(dataSetId: dataSet.Id, page: 1, pageSize: 1);

                var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

                Assert.NotNull(viewModel);
                Assert.Equal(1, viewModel.Paging.Page);
                Assert.Equal(1, viewModel.Paging.PageSize);
                Assert.Equal(0, viewModel.Paging.TotalResults);
                Assert.Empty(viewModel.Results);
            }

            [Fact]
            public async Task PageTooBig_Returns200_EmptyList()
            {
                const int page = 2;
                const int pageSize = 2;
                const int numberOfDataSetVersions = 2;

                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                var dataSetVersions = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .GenerateList(numberOfDataSetVersions);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.AddRange(dataSetVersions));

                var response = await ListDataSetVersions(dataSetId: dataSet.Id, page: page, pageSize: pageSize);

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
                var response = await ListDataSetVersions(dataSetId: Guid.NewGuid(), page: page, pageSize: 1);

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
                var response = await ListDataSetVersions(dataSetId: Guid.NewGuid(), page: 1, pageSize: pageSize);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasInclusiveBetweenError("pageSize", from: 1, to: 20);
            }

            [Theory]
            [MemberData(
                nameof(DataSetStatusTheoryData.UnavailableStatuses),
                MemberType = typeof(DataSetStatusTheoryData)
            )]
            public async Task UnavailableDataSet_Returns503(DataSetStatus status)
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatus(status);

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                var response = await ListDataSetVersions(dataSetId: dataSet.Id);

                response.AssertForbidden();
            }

            [Fact]
            public async Task InvalidDataSetId_Returns404()
            {
                var client = fixture.CreateClient();

                var query = new Dictionary<string, string?> { { "page", "1" }, { "pageSize", "1" } };

                var uri = QueryHelpers.AddQueryString($"{BaseUrl}/not_a_valid_guid/versions", query);

                var response = await client.GetAsync(uri);

                response.AssertNotFound();
            }
        }

        public class PreviewTokenTests(DataSetVersionsControllerTestsFixture fixture)
            : ListDataSetVersionsTests(fixture)
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task PreviewTokenForDraftVersion_IncludesDraftVersionIfValid(bool tokenValid)
            {
                // Arrange
                var (dataSet, previewToken) = await SetUpData(tokenValid);

                //Act
                var response = await ListDataSetVersions(
                    dataSetId: dataSet.Id,
                    page: 1,
                    pageSize: 10,
                    previewTokenId: previewToken.Id
                );
                var viewModel = response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);

                // Assert
                Assert.NotNull(viewModel);
                Assert.Equal(tokenValid ? 2 : 1, viewModel.Results.Count);
            }

            private async Task<(DataSet, PreviewToken)> SetUpData(bool validToken)
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                var (publishedVersion, draftVersion) = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithDataSetId(dataSet.Id)
                    .ForIndex(0, v => v.SetVersionNumber(1, 0).SetStatusPublished())
                    .ForIndex(
                        1,
                        v =>
                            v.SetVersionNumber(1, 1)
                                .SetStatusDraft()
                                .SetPreviewTokens(() => [DataFixture.DefaultPreviewToken(expired: !validToken)])
                    )
                    .GenerateTuple2();
                dataSet.LatestDraftVersion = draftVersion;

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context =>
                    {
                        context.DataSetVersions.AddRange(publishedVersion, draftVersion);
                        context.DataSets.Update(dataSet);
                    });

                return (dataSet, draftVersion.PreviewTokens[0]);
            }
        }

        public class AnalyticsTests(DataSetVersionsControllerTestsFixture fixture) : ListDataSetVersionsTests(fixture)
        {
            [Fact]
            public async Task AnalyticsRequestCaptured()
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatus(DataSetVersionStatus.Published)
                    .WithPublished(DateTimeOffset.UtcNow)
                    .WithDataSetId(dataSet.Id);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var analyticsServiceMock = fixture.GetAnalyticsServiceMock();

                analyticsServiceMock
                    .Setup(s =>
                        s.CaptureDataSetCall(
                            dataSet.Id,
                            DataSetCallType.GetVersions,
                            new PaginationParameters(2, 10),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .Returns(Task.CompletedTask);

                var response = await ListDataSetVersions(dataSetId: dataSet.Id, page: 2, pageSize: 10);

                MockUtils.VerifyAllMocks(analyticsServiceMock);

                response.AssertOk<DataSetVersionPaginatedListViewModel>(useSystemJson: true);
            }
        }

        private async Task<HttpResponseMessage> ListDataSetVersions(
            Guid dataSetId,
            int? page = null,
            int? pageSize = null,
            Guid? previewTokenId = null,
            string? requestSource = null
        )
        {
            var query = new Dictionary<string, string?>
            {
                { "page", page.ToString() },
                { "pageSize", pageSize.ToString() },
            };

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSetId}/versions", query);

            var client = fixture
                .CreateClient()
                .WithPreviewTokenHeader(previewTokenId)
                .WithRequestSourceHeader(requestSource);

            return await client.GetAsync(uri);
        }
    }

    public abstract class GetDataSetVersionTests(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        public class ControllerTests(DataSetVersionsControllerTestsFixture fixture) : GetDataSetVersionTests(fixture)
        {
            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusViewTheoryData.AvailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData)
            )]
            public async Task VersionIsAvailable_Returns200(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                var dataSetVersionGenerator = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatus(dataSetVersionStatus)
                    .WithPublished(DateTimeOffset.UtcNow)
                    .WithDataSetId(dataSet.Id);

                DataSetVersion dataSetVersion =
                    dataSetVersionStatus == DataSetVersionStatus.Withdrawn
                        ? dataSetVersionGenerator.WithWithdrawn(DateTimeOffset.UtcNow)
                        : dataSetVersionGenerator;

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await GetDataSetVersion(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion
                );

                var viewModel = response.AssertOk<DataSetVersionViewModel>(useSystemJson: true);

                Assert.NotNull(viewModel);
                Assert.Equal(dataSetVersion.PublicVersion, viewModel.Version);
                Assert.Equal(dataSetVersion.VersionType, viewModel.Type);
                Assert.Equal(dataSetVersion.Status, viewModel.Status);
                Assert.Equal(dataSetVersion.Published.TruncateNanoseconds(), viewModel.Published);
                if (dataSetVersionStatus == DataSetVersionStatus.Withdrawn)
                {
                    Assert.Equal(dataSetVersion.Withdrawn.TruncateNanoseconds(), viewModel.Withdrawn);
                }

                Assert.Equal(dataSetVersion.Notes, viewModel.Notes);
                Assert.Equal(dataSetVersion.TotalResults, viewModel.TotalResults);

                Assert.Equal(dataSetVersion.Release.DataSetFileId, viewModel.File.Id);

                Assert.Equal(dataSetVersion.Release.Title, viewModel.Release.Title);
                Assert.Equal(dataSetVersion.Release.Slug, viewModel.Release.Slug);

                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(
                        dataSetVersion.MetaSummary!.TimePeriodRange.Start.Period,
                        dataSetVersion.MetaSummary.TimePeriodRange.Start.Code
                    ),
                    viewModel.TimePeriods.Start
                );
                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(
                        dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                        dataSetVersion.MetaSummary.TimePeriodRange.End.Code
                    ),
                    viewModel.TimePeriods.End
                );
                Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, viewModel.GeographicLevels);
                Assert.Equal(dataSetVersion.MetaSummary.Filters, viewModel.Filters);
                Assert.Equal(dataSetVersion.MetaSummary.Indicators, viewModel.Indicators);
            }

            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData)
            )]
            public async Task VersionNotAvailable_Returns403(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSetId(dataSet.Id);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await GetDataSetVersion(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion
                );

                response.AssertForbidden();
            }

            [Fact]
            public async Task VersionExistsForOtherDataSet_Returns404()
            {
                DataSet dataSet1 = DataFixture.DefaultDataSet().WithStatusPublished();

                DataSet dataSet2 = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSets.AddRange(dataSet1, dataSet2));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatusPublished()
                    .WithDataSetId(dataSet1.Id);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await GetDataSetVersion(
                    dataSetId: dataSet2.Id,
                    dataSetVersion: dataSetVersion.PublicVersion
                );

                response.AssertNotFound();
            }

            [Fact]
            public async Task VersionDoesNotExist_Returns404()
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                var response = await GetDataSetVersion(dataSetId: dataSet.Id, dataSetVersion: "1.0");

                response.AssertNotFound();
            }

            [Fact]
            public async Task DataSetDoesNotExist_Returns404()
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await GetDataSetVersion(
                    dataSetId: Guid.NewGuid(),
                    dataSetVersion: dataSetVersion.PublicVersion
                );

                response.AssertNotFound();
            }

            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusViewTheoryData.AvailableStatusesIncludingDraft),
                MemberType = typeof(DataSetVersionStatusViewTheoryData)
            )]
            public async Task PreviewTokenIsActive_Returns200(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()]);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await GetDataSetVersion(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id
                );

                response.AssertOk<DataSetVersionViewModel>(useSystemJson: true);
            }

            [Fact]
            public async Task PreviewTokenIsExpired_Returns403()
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken(expired: true)]);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await GetDataSetVersion(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id
                );

                response.AssertForbidden();
            }

            [Fact]
            public async Task PreviewTokenIsForWrongDataSetVersion_Returns403()
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                var (dataSetVersion1, dataSetVersion2) = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .GenerateTuple2();

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2));

                var response = await GetDataSetVersion(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion1.PublicVersion,
                    previewTokenId: dataSetVersion2.PreviewTokens[0].Id
                );

                response.AssertForbidden();
            }

            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusViewTheoryData.UnavailableStatusesExceptDraft),
                MemberType = typeof(DataSetVersionStatusViewTheoryData)
            )]
            public async Task PreviewTokenIsForUnavailableDataSetVersion_Returns403(
                DataSetVersionStatus dataSetVersionStatus
            )
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()]);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.AddRange(dataSetVersion));

                var response = await GetDataSetVersion(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id
                );

                response.AssertForbidden();
            }

            [Fact]
            public async Task WildCardSpecified_RequestedPublished_Returns200()
            {
                var (dataSet, dataSetVersions) = await CommonTestDataUtil.SetupDataSetWithSpecifiedVersionStatuses(
                    DataSetVersionStatus.Published,
                    fixture.GetPublicDataDbContext()
                );

                var dataSetVersion = dataSetVersions.First(dsv => dsv.PublicVersion == "2.1");

                var response = await GetDataSetVersion(dataSetId: dataSet.Id, dataSetVersion: "2.*");

                var viewModel = response.AssertOk<DataSetVersionViewModel>(useSystemJson: true);

                Assert.NotNull(viewModel);
                Assert.Equal(dataSetVersion.PublicVersion, viewModel.Version);
                Assert.Equal(dataSetVersion.VersionType, viewModel.Type);
                Assert.Equal(dataSetVersion.Status, viewModel.Status);
                Assert.Equal(dataSetVersion.Published.TruncateNanoseconds(), viewModel.Published);

                Assert.Equal(dataSetVersion.Notes, viewModel.Notes);
                Assert.Equal(dataSetVersion.TotalResults, viewModel.TotalResults);

                Assert.Equal(dataSetVersion.Release.DataSetFileId, viewModel.File.Id);

                Assert.Equal(dataSetVersion.Release.Title, viewModel.Release.Title);
                Assert.Equal(dataSetVersion.Release.Slug, viewModel.Release.Slug);

                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(
                        dataSetVersion.MetaSummary!.TimePeriodRange.Start.Period,
                        dataSetVersion.MetaSummary.TimePeriodRange.Start.Code
                    ),
                    viewModel.TimePeriods.Start
                );
                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(
                        dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                        dataSetVersion.MetaSummary.TimePeriodRange.End.Code
                    ),
                    viewModel.TimePeriods.End
                );
                Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, viewModel.GeographicLevels);
                Assert.Equal(dataSetVersion.MetaSummary.Filters, viewModel.Filters);
                Assert.Equal(dataSetVersion.MetaSummary.Indicators, viewModel.Indicators);
            }

            [Theory]
            [MemberData(
                nameof(DataSetVersionStatusViewTheoryData.NonPublishedStatus),
                MemberType = typeof(DataSetVersionStatusViewTheoryData)
            )]
            public async Task WildCardSpecified_RequestedNonPublished_Returns404(
                DataSetVersionStatus dataSetVersionStatus
            )
            {
                var (dataSet, dataSetVersions) = await CommonTestDataUtil.SetupDataSetWithSpecifiedVersionStatuses(
                    dataSetVersionStatus,
                    fixture.GetPublicDataDbContext()
                );

                var response = await GetDataSetVersion(
                    dataSetId: dataSet.Id,
                    dataSetVersion: "2.*",
                    previewTokenId: dataSetVersions.Last().PreviewTokens[0].Id
                );

                response.AssertNotFound();
            }
        }

        public class AnalyticsTests(DataSetVersionsControllerTestsFixture fixture) : GetDataSetVersionTests(fixture)
        {
            [Fact]
            public async Task AnalyticsRequestCaptured()
            {
                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatus(DataSetVersionStatus.Published)
                    .WithPublished(DateTimeOffset.UtcNow)
                    .WithDataSetId(dataSet.Id);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

                var analyticsServiceMock = fixture.GetAnalyticsServiceMock();

                analyticsServiceMock
                    .Setup(s =>
                        s.CaptureDataSetVersionCall(
                            dataSetVersion.Id,
                            DataSetVersionCallType.GetSummary,
                            "1.0",
                            null,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .Returns(Task.CompletedTask);

                var response = await GetDataSetVersion(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion
                );

                MockUtils.VerifyAllMocks(analyticsServiceMock);

                response.AssertOk<DataSetVersionViewModel>(useSystemJson: true);
            }
        }

        private async Task<HttpResponseMessage> GetDataSetVersion(
            Guid dataSetId,
            string dataSetVersion,
            Guid? previewTokenId = null,
            string? requestSource = null
        )
        {
            var client = fixture
                .CreateClient()
                .WithPreviewTokenHeader(previewTokenId)
                .WithRequestSourceHeader(requestSource);

            var uri = new Uri($"{BaseUrl}/{dataSetId}/versions/{dataSetVersion}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class GetDataSetVersionChangesTests(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        [Fact]
        public async Task VersionAvailable_Returns200_AllChanges()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 3, indicators: 4, locations: 0, timePeriods: 3)
                .WithVersionNumber(1, 0)
                .WithDataSetId(dataSet.Id)
                .WithLocationMetas(
                    DataFixture
                        .DefaultLocationMeta(options: 3)
                        .ForIndex(0, s => s.SetLevel(GeographicLevel.Country))
                        .ForIndex(1, s => s.SetLevel(GeographicLevel.Region))
                        .ForIndex(2, s => s.SetLevel(GeographicLevel.LocalAuthorityDistrict))
                        .GenerateList
                )
                .WithGeographicLevelMeta();

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 3, indicators: 4, locations: 0, timePeriods: 3)
                .WithVersionNumber(2, 0)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .WithLocationMetas(
                    DataFixture
                        .DefaultLocationMeta(options: 3)
                        .ForIndex(0, s => s.SetLevel(GeographicLevel.Country))
                        .ForIndex(1, s => s.SetLevel(GeographicLevel.Region))
                        .ForIndex(2, s => s.SetLevel(GeographicLevel.OpportunityArea))
                        .GenerateList
                )
                .WithGeographicLevelMeta();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
                });

            var filterMetaChanges = DataFixture
                .DefaultFilterMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.FilterMetas[0].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.FilterMetas[0].Id))
                .GenerateList();

            var filterOptionMetaChanges = DataFixture
                .DefaultFilterOptionMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousState(oldDataSetVersion.FilterMetas[0].OptionLinks[0]))
                .ForIndex(1, s => s.SetCurrentState(dataSetVersion.FilterMetas[0].OptionLinks[0]))
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
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.IndicatorMetas[0].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.IndicatorMetas[0].Id))
                .GenerateList();

            var locationMetaChanges = DataFixture
                .DefaultLocationMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.LocationMetas[2].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.LocationMetas[2].Id))
                .GenerateList();

            var locationOptionMetaChanges = DataFixture
                .DefaultLocationOptionMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousState(oldDataSetVersion.LocationMetas[1].OptionLinks[0]))
                .ForIndex(1, s => s.SetCurrentState(dataSetVersion.LocationMetas[1].OptionLinks[0]))
                .GenerateList();

            var timePeriodMetaChanges = DataFixture
                .DefaultTimePeriodMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.TimePeriodMetas[0].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.TimePeriodMetas[0].Id))
                .GenerateList();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.FilterMetaChanges.AddRange(filterMetaChanges);
                    context.FilterOptionMetaChanges.AddRange(filterOptionMetaChanges);
                    context.GeographicLevelMetaChanges.AddRange(geographicLevelMetaChanges);
                    context.IndicatorMetaChanges.AddRange(indicatorMetaChanges);
                    context.LocationMetaChanges.AddRange(locationMetaChanges);
                    context.LocationOptionMetaChanges.AddRange(locationOptionMetaChanges);
                    context.TimePeriodMetaChanges.AddRange(timePeriodMetaChanges);
                });

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion
            );

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetChanges_PatchHistory_Returns200_AllChanges(bool includePatchHistory)
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            var (firstDataSetVersion, oldDataSetVersion, dataSetVersion) = DataFixture
                .DefaultDataSetVersion(filters: 3, indicators: 4, locations: 0, timePeriods: 3)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .ForIndex(0, s => s.SetVersionNumber(2, 1))
                .ForIndex(0, s => s.SetGeographicLevelMeta())
                .ForIndex(
                    0,
                    s =>
                        s.SetLocationMetas(
                            DataFixture
                                .DefaultLocationMeta(options: 3)
                                .ForIndex(0, ss => ss.SetLevel(GeographicLevel.Country))
                                .ForIndex(1, ss => ss.SetLevel(GeographicLevel.Region))
                                .ForIndex(2, ss => ss.SetLevel(GeographicLevel.OpportunityArea))
                                .GenerateList
                        )
                )
                .ForIndex(1, s => s.SetVersionNumber(2, 1, 1))
                .ForIndex(1, s => s.SetGeographicLevelMeta())
                .ForIndex(
                    1,
                    s =>
                        s.SetLocationMetas(
                            DataFixture
                                .DefaultLocationMeta(options: 3)
                                .ForIndex(0, ss => ss.SetLevel(GeographicLevel.Country))
                                .ForIndex(1, ss => ss.SetLevel(GeographicLevel.Region))
                                .ForIndex(2, ss => ss.SetLevel(GeographicLevel.OpportunityArea))
                                .GenerateList
                        )
                )
                .ForIndex(2, s => s.SetVersionNumber(2, 1, 2))
                .ForIndex(2, s => s.SetGeographicLevelMeta())
                .ForIndex(
                    2,
                    s =>
                        s.SetLocationMetas(
                            DataFixture
                                .DefaultLocationMeta(options: 3)
                                .ForIndex(0, ss => ss.SetLevel(GeographicLevel.Country))
                                .ForIndex(1, ss => ss.SetLevel(GeographicLevel.Region))
                                .ForIndex(2, ss => ss.SetLevel(GeographicLevel.OpportunityArea))
                                .GenerateList
                        )
                )
                .GenerateTuple3();

            await SetUpHistoryPatchData(firstDataSetVersion, oldDataSetVersion, dataSetVersion);

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion,
                includePatchHistory: includePatchHistory
            );

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            Assert.NotNull(viewModel.PatchHistory);

            if (!includePatchHistory)
            {
                Assert.Empty(viewModel.PatchHistory);
                return;
            }

            Assert.Equal(2, viewModel.PatchHistory.Count);

            Assert.Equal("2.1", viewModel.PatchHistory[0].VersionNumber);
            Assert.Equal("2.1.1", viewModel.PatchHistory[1].VersionNumber);
            Assert.Single(viewModel.PatchHistory[0].MinorChanges.LocationGroups!);
            Assert.Single(viewModel.PatchHistory[0].MinorChanges.LocationGroups!);
            Assert.Single(viewModel.PatchHistory[0].MinorChanges.LocationOptions!);
            Assert.Single(viewModel.PatchHistory[1].MinorChanges.LocationOptions!);
            Assert.Single(viewModel.PatchHistory[0].MinorChanges.Filters!);
            Assert.Single(viewModel.PatchHistory[1].MinorChanges.Filters!);
            Assert.Single(viewModel.PatchHistory[0].MinorChanges.FilterOptions!);
            Assert.Single(viewModel.PatchHistory[1].MinorChanges.FilterOptions!);
            Assert.Single(viewModel.PatchHistory[0].MinorChanges.Indicators!);
            Assert.Single(viewModel.PatchHistory[1].MinorChanges.Indicators!);
            Assert.Single(viewModel.PatchHistory[0].MinorChanges.TimePeriods!);
            Assert.Single(viewModel.PatchHistory[1].MinorChanges.TimePeriods!);
        }

        [Fact]
        public async Task GetChanges_PatchHistory_Returns403_UnViewableDataSetVersion()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion firstDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 3, indicators: 4, locations: 0, timePeriods: 3)
                .WithVersionNumber(2, 1)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithLocationMetas(
                    DataFixture
                        .DefaultLocationMeta(options: 3)
                        .ForIndex(0, s => s.SetLevel(GeographicLevel.Country))
                        .ForIndex(1, s => s.SetLevel(GeographicLevel.Region))
                        .ForIndex(2, s => s.SetLevel(GeographicLevel.LocalAuthorityDistrict))
                        .GenerateList
                )
                .WithGeographicLevelMeta();

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 3, indicators: 4, locations: 0, timePeriods: 3)
                .WithVersionNumber(2, 1, 1)
                .WithDataSetId(dataSet.Id)
                .WithStatusFailed() // set this version to an unviewable dataSetVersion based on `ViewDataSetVersionAuthorizationHandler` to break authorization
                .WithLocationMetas(
                    DataFixture
                        .DefaultLocationMeta(options: 3)
                        .ForIndex(0, s => s.SetLevel(GeographicLevel.Country))
                        .ForIndex(1, s => s.SetLevel(GeographicLevel.Region))
                        .ForIndex(2, s => s.SetLevel(GeographicLevel.LocalAuthorityDistrict))
                        .GenerateList
                )
                .WithGeographicLevelMeta();

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 3, indicators: 4, locations: 0, timePeriods: 3)
                .WithVersionNumber(2, 1, 2)
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id)
                .WithLocationMetas(
                    DataFixture
                        .DefaultLocationMeta(options: 3)
                        .ForIndex(0, s => s.SetLevel(GeographicLevel.Country))
                        .ForIndex(1, s => s.SetLevel(GeographicLevel.Region))
                        .ForIndex(2, s => s.SetLevel(GeographicLevel.OpportunityArea))
                        .GenerateList
                )
                .WithGeographicLevelMeta();

            await SetUpHistoryPatchData(firstDataSetVersion, oldDataSetVersion, dataSetVersion);

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion,
                includePatchHistory: true
            );

            response.AssertForbidden();
        }

        private async Task SetUpHistoryPatchData(
            DataSetVersion firstDataSetVersion,
            DataSetVersion oldDataSetVersion,
            DataSetVersion dataSetVersion
        )
        {
            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(firstDataSetVersion, oldDataSetVersion, dataSetVersion);
                });

            var filterMetaChanges = DataFixture
                .DefaultFilterMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.FilterMetas[0].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.FilterMetas[0].Id))
                .GenerateList();
            filterMetaChanges.AddRange(
                DataFixture
                    .DefaultFilterMetaChange()
                    .WithDataSetVersionId(firstDataSetVersion.Id)
                    .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.FilterMetas[0].Id))
                    .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.FilterMetas[0].Id))
                    .GenerateList()
            );
            filterMetaChanges.AddRange(
                DataFixture
                    .DefaultFilterMetaChange()
                    .WithDataSetVersionId(oldDataSetVersion.Id)
                    .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.FilterMetas[0].Id))
                    .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.FilterMetas[0].Id))
                    .GenerateList()
            );

            var filterOptionMetaChanges = DataFixture
                .DefaultFilterOptionMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousState(oldDataSetVersion.FilterMetas[0].OptionLinks[0]))
                .ForIndex(1, s => s.SetCurrentState(dataSetVersion.FilterMetas[0].OptionLinks[0]))
                .GenerateList();
            filterOptionMetaChanges.AddRange(
                DataFixture
                    .DefaultFilterOptionMetaChange()
                    .WithDataSetVersionId(oldDataSetVersion.Id)
                    .ForIndex(0, s => s.SetPreviousState(oldDataSetVersion.FilterMetas[0].OptionLinks[0]))
                    .ForIndex(1, s => s.SetCurrentState(dataSetVersion.FilterMetas[0].OptionLinks[0]))
                    .GenerateList()
            );
            filterOptionMetaChanges.AddRange(
                DataFixture
                    .DefaultFilterOptionMetaChange()
                    .WithDataSetVersionId(firstDataSetVersion.Id)
                    .ForIndex(0, s => s.SetPreviousState(oldDataSetVersion.FilterMetas[0].OptionLinks[0]))
                    .ForIndex(1, s => s.SetCurrentState(dataSetVersion.FilterMetas[0].OptionLinks[0]))
                    .GenerateList()
            );

            var geographicLevelMetaChanges = DataFixture
                .DefaultGeographicLevelMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .WithPreviousStateId(oldDataSetVersion.GeographicLevelMeta!.Id)
                .WithCurrentStateId(dataSetVersion.GeographicLevelMeta!.Id)
                .GenerateList(1);
            geographicLevelMetaChanges.AddRange(
                DataFixture
                    .DefaultGeographicLevelMetaChange()
                    .WithDataSetVersionId(oldDataSetVersion.Id)
                    .WithPreviousStateId(oldDataSetVersion.GeographicLevelMeta!.Id)
                    .WithCurrentStateId(dataSetVersion.GeographicLevelMeta!.Id)
                    .GenerateList(1)
            );
            geographicLevelMetaChanges.AddRange(
                DataFixture
                    .DefaultGeographicLevelMetaChange()
                    .WithDataSetVersionId(firstDataSetVersion.Id)
                    .WithPreviousStateId(oldDataSetVersion.GeographicLevelMeta!.Id)
                    .WithCurrentStateId(dataSetVersion.GeographicLevelMeta!.Id)
                    .GenerateList(1)
            );

            var indicatorMetaChanges = DataFixture
                .DefaultIndicatorMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.IndicatorMetas[0].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.IndicatorMetas[0].Id))
                .GenerateList();
            indicatorMetaChanges.AddRange(
                DataFixture
                    .DefaultIndicatorMetaChange()
                    .WithDataSetVersionId(oldDataSetVersion.Id)
                    .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.IndicatorMetas[0].Id))
                    .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.IndicatorMetas[0].Id))
                    .GenerateList()
            );
            indicatorMetaChanges.AddRange(
                DataFixture
                    .DefaultIndicatorMetaChange()
                    .WithDataSetVersionId(firstDataSetVersion.Id)
                    .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.IndicatorMetas[0].Id))
                    .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.IndicatorMetas[0].Id))
                    .GenerateList()
            );

            var locationMetaChanges = DataFixture
                .DefaultLocationMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.LocationMetas[2].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.LocationMetas[2].Id))
                .GenerateList();
            locationMetaChanges.Add(
                DataFixture
                    .DefaultLocationMetaChange()
                    .WithDataSetVersionId(firstDataSetVersion.Id)
                    .ForIndex(
                        0,
                        s =>
                            s.SetPreviousStateId(firstDataSetVersion.LocationMetas[1].Id)
                                .SetCurrentStateId(oldDataSetVersion.LocationMetas[1].Id)
                    )
                    .Generate()
            );
            locationMetaChanges.Add(
                DataFixture
                    .DefaultLocationMetaChange()
                    .WithDataSetVersionId(dataSetVersion.Id)
                    .ForIndex(
                        0,
                        s =>
                            s.SetPreviousStateId(firstDataSetVersion.LocationMetas[1].Id)
                                .SetCurrentStateId(oldDataSetVersion.LocationMetas[1].Id)
                    )
                    .Generate()
            );

            var locationOptionMetaChanges = DataFixture
                .DefaultLocationOptionMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousState(oldDataSetVersion.LocationMetas[1].OptionLinks[0]))
                .ForIndex(1, s => s.SetCurrentState(dataSetVersion.LocationMetas[1].OptionLinks[0]))
                .GenerateList();
            locationOptionMetaChanges.AddRange(
                DataFixture
                    .DefaultLocationOptionMetaChange()
                    .WithDataSetVersionId(oldDataSetVersion.Id)
                    .ForIndex(0, s => s.SetPreviousState(oldDataSetVersion.LocationMetas[1].OptionLinks[0]))
                    .ForIndex(1, s => s.SetCurrentState(dataSetVersion.LocationMetas[1].OptionLinks[0]))
                    .GenerateList()
            );
            locationOptionMetaChanges.AddRange(
                DataFixture
                    .DefaultLocationOptionMetaChange()
                    .WithDataSetVersionId(firstDataSetVersion.Id)
                    .ForIndex(0, s => s.SetPreviousState(oldDataSetVersion.LocationMetas[1].OptionLinks[0]))
                    .ForIndex(1, s => s.SetCurrentState(dataSetVersion.LocationMetas[1].OptionLinks[0]))
                    .GenerateList()
            );

            var timePeriodMetaChanges = DataFixture
                .DefaultTimePeriodMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.TimePeriodMetas[0].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.TimePeriodMetas[0].Id))
                .GenerateList();
            timePeriodMetaChanges.AddRange(
                DataFixture
                    .DefaultTimePeriodMetaChange()
                    .WithDataSetVersionId(oldDataSetVersion.Id)
                    .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.TimePeriodMetas[0].Id))
                    .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.TimePeriodMetas[0].Id))
                    .GenerateList()
            );
            timePeriodMetaChanges.AddRange(
                DataFixture
                    .DefaultTimePeriodMetaChange()
                    .WithDataSetVersionId(firstDataSetVersion.Id)
                    .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.TimePeriodMetas[0].Id))
                    .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.TimePeriodMetas[0].Id))
                    .GenerateList()
            );

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.FilterMetaChanges.AddRange(filterMetaChanges);
                    context.FilterOptionMetaChanges.AddRange(filterOptionMetaChanges);
                    context.GeographicLevelMetaChanges.AddRange(geographicLevelMetaChanges);
                    context.IndicatorMetaChanges.AddRange(indicatorMetaChanges);
                    context.LocationMetaChanges.AddRange(locationMetaChanges);
                    context.LocationOptionMetaChanges.AddRange(locationOptionMetaChanges);
                    context.TimePeriodMetaChanges.AddRange(timePeriodMetaChanges);
                });
        }

        [Fact]
        public async Task OnlyFilterChanges_Returns200()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

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
                .WithFilterMetas(() =>
                    DataFixture
                        .DefaultFilterMeta(options: 3)
                        .ForIndex(2, s => s.SetPublicId(oldDataSetVersion.FilterMetas[2].PublicId))
                        .GenerateList(3)
                );

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
                });

            var filterMetaChanges = DataFixture
                .DefaultFilterMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.FilterMetas[0].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.FilterMetas[0].Id))
                .ForIndex(
                    2,
                    s =>
                        s.SetPreviousStateId(oldDataSetVersion.FilterMetas[1].Id)
                            .SetCurrentStateId(dataSetVersion.FilterMetas[1].Id)
                )
                .ForIndex(
                    3,
                    s =>
                        s.SetPreviousStateId(oldDataSetVersion.FilterMetas[2].Id)
                            .SetCurrentStateId(dataSetVersion.FilterMetas[2].Id)
                )
                .GenerateList();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.FilterMetaChanges.AddRange(filterMetaChanges);
                });

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion
            );

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
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

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
                .WithFilterMetas(() =>
                    DataFixture
                        .DefaultFilterMeta(options: 0)
                        .WithOptionLinks(() =>
                            DataFixture
                                .DefaultFilterOptionMetaLink()
                                .WithOption(() => DataFixture.DefaultFilterOptionMeta())
                                // Simulates only ID being changed - major
                                .ForIndex(1, l => l.Set((_, link) => link.Option.Label = oldOptionLink3.Option.Label))
                                // Simulates only label being changed - minor
                                .ForIndex(2, l => l.SetPublicId(oldOptionLink3.PublicId))
                                .GenerateList(3)
                        )
                        .GenerateList(1)
                );

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
                });

            var newOptionLink1 = dataSetVersion.FilterMetas[0].OptionLinks[0];
            var newOptionLink2 = dataSetVersion.FilterMetas[0].OptionLinks[1];
            var newOptionLink3 = dataSetVersion.FilterMetas[0].OptionLinks[2];

            var filterOptionMetaChanges = DataFixture
                .DefaultFilterOptionMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousState(oldOptionLink1))
                .ForIndex(1, s => s.SetCurrentState(newOptionLink1))
                .ForIndex(2, s => s.SetPreviousState(oldOptionLink2).SetCurrentState(newOptionLink2))
                .ForIndex(3, s => s.SetPreviousState(oldOptionLink3).SetCurrentState(newOptionLink3))
                .GenerateList();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.FilterOptionMetaChanges.AddRange(filterOptionMetaChanges);
                });

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion
            );

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
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithGeographicLevelMeta(() =>
                    DataFixture
                        .DefaultGeographicLevelMeta()
                        .WithLevels([GeographicLevel.Country, GeographicLevel.LocalAuthorityDistrict])
                );

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 2, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithGeographicLevelMeta(() =>
                    DataFixture
                        .DefaultGeographicLevelMeta()
                        .WithLevels([GeographicLevel.Country, GeographicLevel.Region, GeographicLevel.LocalAuthority])
                );

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
                });

            GeographicLevelMetaChange geographicLevelMetaChange = DataFixture
                .DefaultGeographicLevelMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .WithPreviousStateId(oldDataSetVersion.GeographicLevelMeta!.Id)
                .WithCurrentStateId(dataSetVersion.GeographicLevelMeta!.Id);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.GeographicLevelMetaChanges.Add(geographicLevelMetaChange);
                });

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion
            );

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            var majorChanges = viewModel.MajorChanges.GeographicLevels;

            Assert.NotNull(majorChanges);
            Assert.Single(majorChanges);

            // Deletion
            Assert.Null(majorChanges[0].CurrentState);
            Assert.NotNull(majorChanges[0].PreviousState);
            Assert.Equal(GeographicLevel.LocalAuthorityDistrict, majorChanges[0].PreviousState!.Code);

            var minorChanges = viewModel.MinorChanges.GeographicLevels;

            Assert.NotNull(minorChanges);
            Assert.Equal(2, minorChanges.Count);

            // Addition
            Assert.Null(minorChanges[0].PreviousState);
            Assert.NotNull(minorChanges[0].CurrentState);
            Assert.Equal(GeographicLevel.LocalAuthority, minorChanges[0].CurrentState!.Code);

            // Addition
            Assert.Null(minorChanges[1].PreviousState);
            Assert.NotNull(minorChanges[1].CurrentState);
            Assert.Equal(GeographicLevel.Region, minorChanges[1].CurrentState!.Code);
        }

        [Fact]
        public async Task OnlyLocationGroupChanges_Returns200()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithLocationMetas(() =>
                    DataFixture
                        .DefaultLocationMeta(options: 1)
                        .ForIndex(0, s => s.SetLevel(GeographicLevel.MayoralCombinedAuthority))
                        .ForIndex(1, s => s.SetLevel(GeographicLevel.LocalAuthorityDistrict))
                        .GenerateList()
                );

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 2, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithLocationMetas(() =>
                    DataFixture
                        .DefaultLocationMeta(options: 1)
                        .ForIndex(0, s => s.SetLevel(GeographicLevel.Country))
                        .ForIndex(1, s => s.SetLevel(GeographicLevel.LocalAuthority))
                        .GenerateList()
                );

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
                });

            var locationMetaChanges = DataFixture
                .DefaultLocationMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.LocationMetas[0].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.LocationMetas[0].Id))
                .ForIndex(
                    2,
                    s =>
                        s.SetPreviousStateId(oldDataSetVersion.LocationMetas[1].Id)
                            .SetCurrentStateId(dataSetVersion.LocationMetas[1].Id)
                )
                .GenerateList();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.LocationMetaChanges.AddRange(locationMetaChanges);
                });

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion
            );

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            var majorChanges = viewModel.MajorChanges.LocationGroups;

            Assert.NotNull(majorChanges);
            Assert.Equal(2, majorChanges.Count);

            // Deletion
            Assert.Null(majorChanges[0].CurrentState);
            Assert.NotNull(majorChanges[0].PreviousState);
            Assert.Equal(oldDataSetVersion.LocationMetas[0].Level, majorChanges[0].PreviousState!.Level.Code);

            // Updated level
            Assert.NotNull(majorChanges[1].PreviousState);
            Assert.Equal(oldDataSetVersion.LocationMetas[1].Level, majorChanges[1].PreviousState!.Level.Code);

            Assert.NotNull(majorChanges[1].CurrentState);
            Assert.Equal(dataSetVersion.LocationMetas[1].Level, majorChanges[1].CurrentState!.Level.Code);

            var minorChanges = viewModel.MinorChanges.LocationGroups;

            Assert.NotNull(minorChanges);
            Assert.Single(minorChanges);

            // Addition
            Assert.Null(minorChanges[0].PreviousState);
            Assert.NotNull(minorChanges[0].CurrentState);
            Assert.Equal(dataSetVersion.LocationMetas[0].Level, minorChanges[0].CurrentState!.Level.Code);
        }

        [Fact]
        public async Task OnlyLocationOptionChanges_Returns200()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

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
                .WithLocationMetas(() =>
                    DataFixture
                        .DefaultLocationMeta()
                        .WithOptionLinks(() =>
                            DataFixture
                                .DefaultLocationOptionMetaLink()
                                .WithOption(() => DataFixture.DefaultLocationCodedOptionMeta())
                                // Simulates only code being changed - major
                                .ForIndex(
                                    1,
                                    l =>
                                        l.Set((_, link) => link.PublicId = oldOptionLink2.PublicId)
                                            .Set((_, link) => link.Option.Label = oldOption2.Label)
                                )
                                // Simulates only the label being changed - minor
                                .ForIndex(
                                    2,
                                    l =>
                                        l.SetPublicId(oldOptionLink3.PublicId)
                                            .Set(
                                                (_, link) =>
                                                    (link.Option as LocationCodedOptionMeta)!.Code = oldOption3.Code
                                            )
                                )
                                .GenerateList(3)
                        )
                        .GenerateList(1)
                );

            var newOptionLink1 = dataSetVersion.LocationMetas[0].OptionLinks[0];
            var newOptionLink2 = dataSetVersion.LocationMetas[0].OptionLinks[1];
            var newOptionLink3 = dataSetVersion.LocationMetas[0].OptionLinks[2];

            var newOption1 = (newOptionLink1.Option as LocationCodedOptionMeta)!;
            var newOption2 = (newOptionLink2.Option as LocationCodedOptionMeta)!;
            var newOption3 = (newOptionLink3.Option as LocationCodedOptionMeta)!;

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
                });

            var locationOptionMetaChanges = DataFixture
                .DefaultLocationOptionMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousState(oldOptionLink1))
                .ForIndex(1, s => s.SetCurrentState(newOptionLink1))
                .ForIndex(2, s => s.SetPreviousState(oldOptionLink2).SetCurrentState(newOptionLink2))
                .ForIndex(3, s => s.SetPreviousState(oldOptionLink3).SetCurrentState(newOptionLink3))
                .GenerateList();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.LocationOptionMetaChanges.AddRange(locationOptionMetaChanges);
                });

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion
            );

            var viewModel = response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);

            var majorChanges = viewModel.MajorChanges.LocationOptions;

            Assert.NotNull(majorChanges);
            Assert.Single(majorChanges);

            Assert.Equal(dataSetVersion.LocationMetas[0].Level, majorChanges[0].Level.Code);

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

            Assert.Equal(dataSetVersion.LocationMetas[0].Level, minorChanges[0].Level.Code);

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
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion oldDataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithTimePeriodMetas(() =>
                    DataFixture
                        .DefaultTimePeriodMeta()
                        .WithCode(TimeIdentifier.CalendarYear)
                        .ForIndex(0, s => s.SetPeriod("2020"))
                        .ForIndex(1, s => s.SetPeriod("2021"))
                        .ForIndex(2, s => s.SetPeriod("2022"))
                        .GenerateList()
                );

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 2, minor: 0)
                .WithDataSetId(dataSet.Id)
                .WithStatusPublished()
                .WithTimePeriodMetas(() =>
                    DataFixture
                        .DefaultTimePeriodMeta()
                        .WithCode(TimeIdentifier.CalendarYear)
                        .ForIndex(0, s => s.SetPeriod("2021"))
                        .ForIndex(1, s => s.SetPeriod("2022"))
                        .ForIndex(2, s => s.SetPeriod("2023"))
                        .GenerateList()
                );

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
                });

            var timePeriodMetaChanges = DataFixture
                .DefaultTimePeriodMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .ForIndex(0, s => s.SetPreviousStateId(oldDataSetVersion.TimePeriodMetas[0].Id))
                .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.TimePeriodMetas[2].Id))
                .GenerateList();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.TimePeriodMetaChanges.AddRange(timePeriodMetaChanges);
                });

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion
            );

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
        [MemberData(
            nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
            MemberType = typeof(DataSetVersionStatusViewTheoryData)
        )]
        public async Task VersionNotAvailable_Returns403(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task VersionExistsForOtherDataSet_Returns404()
        {
            DataSet dataSet1 = DataFixture.DefaultDataSet().WithStatusPublished();

            DataSet dataSet2 = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.AddRange(dataSet1, dataSet2));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatusPublished()
                .WithDataSetId(dataSet1.Id);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet2.Id,
                dataSetVersion: dataSetVersion.PublicVersion
            );

            response.AssertNotFound();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            var response = await GetDataSetVersionChanges(dataSetId: dataSet.Id, dataSetVersion: "1.0");

            response.AssertNotFound();
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatusPublished()
                .WithDataSetId(dataSet.Id);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersionChanges(
                dataSetId: Guid.NewGuid(),
                dataSetVersion: dataSetVersion.PublicVersion
            );

            response.AssertNotFound();
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusViewTheoryData.AvailableStatusesIncludingDraft),
            MemberType = typeof(DataSetVersionStatusViewTheoryData)
        )]
        public async Task PreviewTokenIsActive_Returns200(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()]);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion,
                previewTokenId: dataSetVersion.PreviewTokens[0].Id
            );

            response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);
        }

        [Fact]
        public async Task PreviewTokenIsExpired_Returns403()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatus(DataSetVersionStatus.Draft)
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken(expired: true)]);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion,
                previewTokenId: dataSetVersion.PreviewTokens[0].Id
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task PreviewTokenIsForWrongDataSetVersion_Returns403()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            var (dataSetVersion1, dataSetVersion2) = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatus(DataSetVersionStatus.Draft)
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                .GenerateTuple2();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2));

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion1.PublicVersion,
                previewTokenId: dataSetVersion2.PreviewTokens[0].Id
            );

            response.AssertForbidden();
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusViewTheoryData.UnavailableStatusesExceptDraft),
            MemberType = typeof(DataSetVersionStatusViewTheoryData)
        )]
        public async Task PreviewTokenIsForUnavailableDataSetVersion_Returns403(
            DataSetVersionStatus dataSetVersionStatus
        )
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()]);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSetVersions.AddRange(dataSetVersion));

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion,
                previewTokenId: dataSetVersion.PreviewTokens[0].Id
            );

            response.AssertForbidden();
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
            MemberType = typeof(DataSetVersionStatusViewTheoryData)
        )]
        public async Task VersionNotAvailable_UserMissingAdminAccessReadRole_Returns403(
            DataSetVersionStatus dataSetVersionStatus
        )
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

            var userWithIncorrectRole = DataFixture.UnsupportedRoleUser();

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion,
                user: userWithIncorrectRole
            );

            response.AssertForbidden();
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
            MemberType = typeof(DataSetVersionStatusViewTheoryData)
        )]
        public async Task VersionNotAvailable_UserHasAdminAccessReadRole_Returns200(
            DataSetVersionStatus dataSetVersionStatus
        )
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

            var userWithCorrectRole = DataFixture.AdminAccessUser();

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion,
                user: userWithCorrectRole
            );

            response.AssertOk();
        }

        [Fact]
        public async Task WildCardSpecified_RequestedPublished_Returns200()
        {
            var (dataSet, dataSetVersions) = await CommonTestDataUtil.SetupDataSetWithSpecifiedVersionStatuses(
                DataSetVersionStatus.Published,
                fixture.GetPublicDataDbContext()
            );

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: "2.*",
                previewTokenId: dataSetVersions.Last().PreviewTokens[0].Id
            );

            response.AssertOk();
        }

        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusViewTheoryData.NonPublishedStatus),
            MemberType = typeof(DataSetVersionStatusViewTheoryData)
        )]
        public async Task WildCardSpecified_RequestedNonPublished_Returns404(DataSetVersionStatus dataSetVersionStatus)
        {
            var (dataSet, dataSetVersions) = await CommonTestDataUtil.SetupDataSetWithSpecifiedVersionStatuses(
                dataSetVersionStatus,
                fixture.GetPublicDataDbContext()
            );

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: "2.*",
                previewTokenId: dataSetVersions.Last().PreviewTokens[0].Id
            );

            response.AssertNotFound();
        }

        [Fact]
        public async Task AnalyticsRequestCaptured()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

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
                .WithFilterMetas(() =>
                    DataFixture
                        .DefaultFilterMeta(options: 3)
                        .ForIndex(2, s => s.SetPublicId(oldDataSetVersion.FilterMetas[2].PublicId))
                        .GenerateList(3)
                );

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(oldDataSetVersion, dataSetVersion);
                });

            var filterMetaChanges = DataFixture
                .DefaultFilterMetaChange()
                .WithDataSetVersionId(dataSetVersion.Id)
                .WithPreviousStateId(oldDataSetVersion.FilterMetas[0].Id)
                .GenerateList(1);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.FilterMetaChanges.AddRange(filterMetaChanges);
                });

            var analyticsServiceMock = fixture.GetAnalyticsServiceMock();

            analyticsServiceMock
                .Setup(s =>
                    s.CaptureDataSetVersionCall(
                        dataSetVersion.Id,
                        DataSetVersionCallType.GetChanges,
                        "2.0",
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var response = await GetDataSetVersionChanges(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion
            );

            MockUtils.VerifyAllMocks(analyticsServiceMock);

            response.AssertOk<DataSetVersionChangesViewModel>(useSystemJson: true);
        }

        private async Task<HttpResponseMessage> GetDataSetVersionChanges(
            Guid dataSetId,
            string dataSetVersion,
            Guid? previewTokenId = null,
            ClaimsPrincipal? user = null,
            string? requestSource = null,
            bool includePatchHistory = false
        )
        {
            var client = fixture
                .CreateClient(user)
                .WithPreviewTokenHeader(previewTokenId)
                .WithRequestSourceHeader(requestSource);

            var uri = new Uri(
                $"{BaseUrl}/{dataSetId}/versions/{dataSetVersion}/changes?includePatchHistory={includePatchHistory}",
                UriKind.Relative
            );

            return await client.GetAsync(uri);
        }
    }
}
