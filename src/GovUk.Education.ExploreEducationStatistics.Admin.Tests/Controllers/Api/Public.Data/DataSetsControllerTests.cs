#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.WebUtilities;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public class DataSetsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/data-sets";

    public class ListDataSetsTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        [Fact]
        public async Task PublicationHasSingleDataSet_Success_CorrectViewModel()
        {
            Publication publication = DataFixture
                .DefaultPublication();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion draftDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

            DataSetVersion liveDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(draftDataSetVersion, liveDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await ListPublicationDataSets(publication.Id);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetViewModel>>();

            pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: 1);

            var dataSetViewModel = pagedResult.Results.First();

            Assert.NotNull(dataSetViewModel.DraftVersion);
            Assert.NotNull(dataSetViewModel.LatestLiveVersion);

            Assert.Multiple(
                () => Assert.Equal(dataSet.Id, dataSetViewModel.Id),
                () => Assert.Equal(dataSet.Title, dataSetViewModel.Title),
                () => Assert.Equal(dataSet.Summary, dataSetViewModel.Summary),
                () => Assert.Equal(dataSet.Status, dataSetViewModel.Status),
                () => Assert.Null(dataSetViewModel.SupersedingDataSetId),
                () => Assert.Equal(draftDataSetVersion.Id, dataSetViewModel.DraftVersion.Id),
                () => Assert.Equal(draftDataSetVersion.Version, dataSetViewModel.DraftVersion.Number),
                () => Assert.Equal(draftDataSetVersion.Status, dataSetViewModel.DraftVersion.Status),
                () => Assert.Equal(draftDataSetVersion.VersionType, dataSetViewModel.DraftVersion.Type),
                () => Assert.Equal(liveDataSetVersion.Id, dataSetViewModel.LatestLiveVersion.Id),
                () => Assert.Equal(liveDataSetVersion.Version, dataSetViewModel.LatestLiveVersion.Number),
                () => Assert.Equal(liveDataSetVersion.Status, dataSetViewModel.LatestLiveVersion.Status),
                () => Assert.Equal(liveDataSetVersion.VersionType, dataSetViewModel.LatestLiveVersion.Type),
                () => Assert.Equal(liveDataSetVersion.Published?.ToUnixTimeSeconds(),
                    dataSetViewModel.LatestLiveVersion?.Published.ToUnixTimeSeconds())
            );
        }

        [Theory]
        [InlineData(1, 10, 2)]
        [InlineData(1, 2, 10)]
        [InlineData(2, 2, 10)]
        public async Task PublicationHasMultipleDataSets_Success_CorrectPaging(
            int page,
            int pageSize,
            int numberOfAvailableDataSets)
        {
            Publication publication = DataFixture
                .DefaultPublication();

            var dataSets = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id)
                .GenerateList(numberOfAvailableDataSets);

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(dataSets));

            var dataSetVersions = dataSets
                .Select(ds =>
                    DataFixture
                        .DefaultDataSetVersion(
                            filters: 1,
                            indicators: 1,
                            locations: 1,
                            timePeriods: 2)
                        .WithStatusPublished()
                        .WithDataSet(ds)
                        .FinishWith(dsv => ds.LatestLiveVersion = dsv)
                        .Generate())
                .ToList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersions);
                context.DataSets.UpdateRange(dataSets);
            });

            var expectedDataSetIds = dataSets
                .OrderByDescending(ds => ds.LatestLiveVersion!.Published)
                .Select(ds => ds.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = await ListPublicationDataSets(publication.Id, page: page, pageSize: pageSize);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetViewModel>>();

            pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: numberOfAvailableDataSets,
                expectedPage: page,
                expectedPageSize: pageSize);

            Assert.Equal(expectedDataSetIds.Count, pagedResult.Results.Count);
            Assert.Equal(expectedDataSetIds, pagedResult.Results.Select(ds => ds.Id));
        }

        [Fact]
        public async Task PublicationHasMultipleDataSets_Success_CorrectOrdering()
        {
            Publication publication = DataFixture
                .DefaultPublication();

            var dataSets = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id)
                .ForIndex(3, s => s.SetTitle("Dataset B"))
                .ForIndex(5, s => s.SetTitle("Dataset A"))
                .GenerateList(6);

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(dataSets));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .ForIndex(0, s =>
                {
                    // Associate data set 0 with a live version published 3 days ago
                    s.SetDataSet(dataSets[0]);
                    s.SetStatus(DataSetVersionStatus.Deprecated);
                    s.SetPublished(DateTimeOffset.UtcNow.AddDays(-3));
                })
                .ForIndex(1, s =>
                {
                    // Associate data set 1 with a live version published 4 days ago
                    s.SetDataSet(dataSets[1]);
                    s.SetStatus(DataSetVersionStatus.Published);
                    s.SetPublished(DateTimeOffset.UtcNow.AddDays(-4));
                })
                .ForIndex(2, s =>
                {
                    // Also associate data set 1 with a draft version that should have no bearing on the ordering
                    s.SetDataSet(dataSets[1]);
                    s.SetStatusDraft();
                })
                .ForIndex(3, s =>
                {
                    // Associate dataset 2 with a live version published 1 day ago
                    s.SetDataSet(dataSets[2]);
                    s.SetStatus(DataSetVersionStatus.Deprecated);
                    s.SetPublished(DateTimeOffset.UtcNow.AddDays(-1));
                })
                .ForIndex(4, s =>
                {
                    // Associate dataset 3 with a draft version that should have no bearing on the ordering
                    s.SetDataSet(dataSets[3]);
                    s.SetStatusDraft();
                })
                .ForIndex(5, s =>
                {
                    // Associate dataset 4 with a live version published 2 days ago
                    s.SetDataSet(dataSets[4]);
                    s.SetStatus(DataSetVersionStatus.Published);
                    s.SetPublished(DateTimeOffset.UtcNow.AddDays(-2));
                })
                .ForIndex(6, s =>
                {
                    // Associate dataset 5 with a draft version that should have no bearing on the ordering
                    s.SetDataSet(dataSets[5]);
                    s.SetStatusDraft();
                })
                .FinishWith(dsv =>
                {
                    switch (dsv.Status)
                    {
                        case DataSetVersionStatus.Draft:
                            dsv.DataSet.LatestDraftVersion = dsv;
                            break;
                        case DataSetVersionStatus.Published or DataSetVersionStatus.Deprecated:
                            dsv.DataSet.LatestLiveVersion = dsv;
                            break;
                    }
                })
                .GenerateList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersions);
                context.DataSets.UpdateRange(dataSets);
            });

            var response = await ListPublicationDataSets(publication.Id);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetViewModel>>();

            pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: dataSets.Count);

            Guid[] expectedDataSetIds =
            [
                dataSets[2].Id, // Latest live version published 1 day ago
                dataSets[4].Id, // Latest live version published 2 days ago
                dataSets[0].Id, // Latest live version published 3 days ago
                dataSets[1].Id, // Latest live version published 4 days ago
                dataSets[5].Id, // Dataset has no live versions and title Dataset A
                dataSets[3].Id, // Dataset has no live versions and title Dataset B
            ];

            Assert.Equal(expectedDataSetIds, pagedResult.Results.Select(ds => ds.Id));
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Failed)]
        [InlineData(DataSetVersionStatus.Processing)]
        [InlineData(DataSetVersionStatus.Mapping)]
        [InlineData(DataSetVersionStatus.Draft)]
        public async Task PublicationHasSingleDataSetWithoutLiveVersion_LatestLiveVersionIsEmpty(
            DataSetVersionStatus dataSetVersionStatus)
        {
            Publication publication = DataFixture
                .DefaultPublication();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await ListPublicationDataSets(publication.Id);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetViewModel>>();

            pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: 1);

            var dataSetViewModel = pagedResult.Results.Single();

            Assert.Null(dataSetViewModel.LatestLiveVersion);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Deprecated)]
        [InlineData(DataSetVersionStatus.Published)]
        public async Task PublicationHasSingleDataSetWithoutDraftVersion_DraftVersionIsEmpty(
            DataSetVersionStatus dataSetVersionStatus)
        {
            Publication publication = DataFixture
                .DefaultPublication();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await ListPublicationDataSets(publication.Id);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetViewModel>>();

            pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: 1);

            var dataSetViewModel = pagedResult.Results.Single();

            Assert.Null(dataSetViewModel.DraftVersion);
        }

        [Fact]
        public async Task PublicationHasNoDataSets_ReturnsEmpty()
        {
            Publication publication = DataFixture
                .DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

            var response = await ListPublicationDataSets(publication.Id);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetViewModel>>();

            pagedResult.AssertHasPagingConsistentWithEmptyResults();
        }

        [Fact]
        public async Task NoPublication_ReturnsNotFound()
        {
            var publicationId = Guid.NewGuid();

            var response = await ListPublicationDataSets(publicationId);

            response.AssertNotFound();
        }

        [Fact]
        public async Task UserHasNoAccessToPublication_ReturnsForbidden()
        {
            Publication publication = DataFixture
                .DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

            var client = TestApp
                .SetUser(AuthenticatedUser())
                .CreateClient();

            var response = await ListPublicationDataSets(publication.Id, client: client);

            response.AssertForbidden();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task PageBelowMinimumThreshold_ReturnsValidationError(int page)
        {
            var response = await ListPublicationDataSets(publicationId: Guid.NewGuid(), page: page);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasGreaterThanOrEqualError("page", comparisonValue: 1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(9999)]
        public async Task PageAboveMinimumThreshold_Success(int page)
        {
            Publication publication = DataFixture
                .DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

            var response = await ListPublicationDataSets(publication.Id, page: page);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetViewModel>>();

            pagedResult.AssertHasPagingConsistentWithEmptyResults(expectedPage: page);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(101)]
        public async Task PageSizeOutsideAllowedRange_ReturnsValidationError(int pageSize)
        {
            var response = await ListPublicationDataSets(publicationId: Guid.NewGuid(), pageSize: pageSize);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasInclusiveBetweenError("pageSize", from: 1, to: 100);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public async Task PageSizeInAllowedRange_Success(int pageSize)
        {
            Publication publication = DataFixture
                .DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

            var response = await ListPublicationDataSets(publication.Id, pageSize: pageSize);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetViewModel>>();

            pagedResult.AssertHasPagingConsistentWithEmptyResults(expectedPageSize: pageSize);
        }

        private async Task<HttpResponseMessage> ListPublicationDataSets(
            Guid publicationId,
            int? page = null,
            int? pageSize = null,
            HttpClient? client = null)
        {
            client ??= TestApp
                .SetUser(UserWithAccessAllPublicationsClaim())
                .CreateClient();

            var queryParams = new Dictionary<string, string?>
            {
                {
                    "page", page?.ToString()
                },
                {
                    "pageSize", pageSize?.ToString()
                },
                {
                    "publicationId", publicationId.ToString()
                },
            };

            var uri = QueryHelpers.AddQueryString(BaseUrl, queryParams);

            return await client.GetAsync(uri);
        }

        private static ClaimsPrincipal UserWithAccessAllPublicationsClaim()
        {
            var claimsPrincipal = AuthenticatedUser();
            var claimsIdentity = (claimsPrincipal.Identity as ClaimsIdentity)!;
            claimsIdentity.AddClaim(SecurityClaim(SecurityClaimTypes.AccessAllPublications));
            return claimsPrincipal;
        }
    }
}
