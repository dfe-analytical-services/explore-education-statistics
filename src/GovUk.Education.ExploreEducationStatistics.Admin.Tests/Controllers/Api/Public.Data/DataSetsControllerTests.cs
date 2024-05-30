#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public class DataSetsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/data-sets";

    public class ListDataSetsTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        [Fact]
        public async Task PublicationHasSingleDataSet_Success_CorrectViewModel()
        {
            Publication publication = DataFixture.DefaultPublication();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion draftDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

            DataSetVersion liveDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(draftDataSetVersion, liveDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await ListPublicationDataSets(publication.Id);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

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
                () => Assert.Equal(draftDataSetVersion.Version, dataSetViewModel.DraftVersion.Version),
                () => Assert.Equal(draftDataSetVersion.Status, dataSetViewModel.DraftVersion.Status),
                () => Assert.Equal(draftDataSetVersion.VersionType, dataSetViewModel.DraftVersion.Type),
                () => Assert.Equal(liveDataSetVersion.Id, dataSetViewModel.LatestLiveVersion.Id),
                () => Assert.Equal(liveDataSetVersion.Version, dataSetViewModel.LatestLiveVersion.Version),
                () => Assert.Equal(liveDataSetVersion.Status, dataSetViewModel.LatestLiveVersion.Status),
                () => Assert.Equal(liveDataSetVersion.VersionType, dataSetViewModel.LatestLiveVersion.Type),
                () => Assert.Equal(liveDataSetVersion.Published.TruncateNanoseconds(),
                    dataSetViewModel.LatestLiveVersion?.Published)
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
            Publication publication = DataFixture.DefaultPublication();

            var dataSets = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id)
                .GenerateList(numberOfAvailableDataSets);

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(dataSets));

            var dataSetVersions = dataSets
                .Select(ds => DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
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

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

            pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: numberOfAvailableDataSets,
                expectedPage: page,
                expectedPageSize: pageSize);

            Assert.Equal(expectedDataSetIds.Count, pagedResult.Results.Count);
            Assert.Equal(expectedDataSetIds, pagedResult.Results.Select(ds => ds.Id));
        }

        [Fact]
        public async Task PublicationHasMultipleDataSets_Success_CorrectOrdering()
        {
            Publication publication = DataFixture.DefaultPublication();

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
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
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

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

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
            Publication publication = DataFixture.DefaultPublication();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await ListPublicationDataSets(publication.Id);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

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
            Publication publication = DataFixture.DefaultPublication();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
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

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

            pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: 1);

            var dataSetViewModel = pagedResult.Results.Single();

            Assert.Null(dataSetViewModel.DraftVersion);
        }

        [Fact]
        public async Task PublicationHasNoDataSets_ReturnsEmpty()
        {
            Publication publication = DataFixture.DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

            var response = await ListPublicationDataSets(publication.Id);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

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
            Publication publication = DataFixture.DefaultPublication();

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
            Publication publication = DataFixture.DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

            var response = await ListPublicationDataSets(publication.Id, page: page);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

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
            Publication publication = DataFixture.DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

            var response = await ListPublicationDataSets(publication.Id, pageSize: pageSize);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

            pagedResult.AssertHasPagingConsistentWithEmptyResults(expectedPageSize: pageSize);
        }

        private async Task<HttpResponseMessage> ListPublicationDataSets(
            Guid publicationId,
            int? page = null,
            int? pageSize = null,
            HttpClient? client = null)
        {
            client ??= TestApp
                .SetUser(AuthenticatedUser(SecurityClaim(SecurityClaimTypes.AccessAllPublications)))
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
    }

    public class GetDataSetTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                    .DefaultRelease(publishedVersions: 1, draftVersion: true)
                    .Generate(1)
                );

            var liveReleaseVersion = publication.ReleaseVersions.Single(rv => rv.Published is not null);

            var draftReleaseVersion = publication.ReleaseVersions.Single(rv => rv.Published is null);

            ReleaseFile liveReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile())
                .WithReleaseVersion(liveReleaseVersion);

            ReleaseFile draftReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile())
                .WithReleaseVersion(draftReleaseVersion);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.AddRange(liveReleaseFile, draftReleaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion liveDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusPublished()
                .WithTotalResults(5000)
                .WithReleaseFileId(liveReleaseFile.Id)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion draftDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusDraft()
                .WithVersionNumber(1, 1)
                .WithTotalResults(6000)
                .WithReleaseFileId(draftReleaseFile.Id)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(liveDataSetVersion, draftDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await GetDataSet(dataSet.Id);

            var viewModel = response.AssertOk<DataSetViewModel>();

            Assert.Equal(dataSet.Id, viewModel.Id);
            Assert.Equal(dataSet.Title, viewModel.Title);
            Assert.Equal(dataSet.Summary, viewModel.Summary);
            Assert.Equal(dataSet.Status, viewModel.Status);

            Assert.Equal(liveDataSetVersion.Id, viewModel.LatestLiveVersion!.Id);
            Assert.Equal(liveDataSetVersion.Version, viewModel.LatestLiveVersion.Version);
            Assert.Equal(liveDataSetVersion.Status, viewModel.LatestLiveVersion.Status);
            Assert.Equal(liveDataSetVersion.VersionType, viewModel.LatestLiveVersion.Type);
            Assert.Equal(liveDataSetVersion.TotalResults, viewModel.LatestLiveVersion.TotalResults);
            Assert.Equal(liveDataSetVersion.Published.TruncateNanoseconds(), viewModel.LatestLiveVersion.Published);
            Assert.Equal(liveReleaseFile.File.DataSetFileId, viewModel.LatestLiveVersion.File.Id);
            Assert.Equal(liveReleaseFile.Name, viewModel.LatestLiveVersion.File.Title);

            Assert.Equal(
                liveDataSetVersion.MetaSummary!.GeographicLevels.Select(l => l.GetEnumLabel()),
                viewModel.LatestLiveVersion.GeographicLevels);
            Assert.Equal(
                liveDataSetVersion.MetaSummary!.Filters,
                viewModel.LatestLiveVersion.Filters);
            Assert.Equal(
                TimePeriodRangeViewModel.Create(liveDataSetVersion.MetaSummary!.TimePeriodRange),
                viewModel.LatestLiveVersion.TimePeriods);
            Assert.Equal(
                liveDataSetVersion.MetaSummary!.Indicators,
                viewModel.LatestLiveVersion.Indicators);

            Assert.Equal(liveReleaseVersion.Id, viewModel.LatestLiveVersion.ReleaseVersion.Id);
            Assert.Equal(liveReleaseVersion.Title, viewModel.LatestLiveVersion.ReleaseVersion.Title);

            Assert.Equal(draftDataSetVersion.Id, viewModel.DraftVersion!.Id);
            Assert.Equal(draftDataSetVersion.Version, viewModel.DraftVersion.Version);
            Assert.Equal(draftDataSetVersion.Status, viewModel.DraftVersion.Status);
            Assert.Equal(draftDataSetVersion.VersionType, viewModel.DraftVersion.Type);
            Assert.Equal(draftDataSetVersion.TotalResults, viewModel.DraftVersion.TotalResults);
            Assert.Equal(draftReleaseFile.File.DataSetFileId, viewModel.DraftVersion.File.Id);
            Assert.Equal(draftReleaseFile.Name, viewModel.DraftVersion.File.Title);

            Assert.Equal(draftReleaseVersion.Id, viewModel.DraftVersion.ReleaseVersion.Id);
            Assert.Equal(draftReleaseVersion.Title, viewModel.DraftVersion.ReleaseVersion.Title);
            Assert.Equal(
                draftDataSetVersion.MetaSummary!.GeographicLevels.Select(l => l.GetEnumLabel()),
                viewModel.DraftVersion.GeographicLevels);
            Assert.Equal(
                draftDataSetVersion.MetaSummary!.Filters,
                viewModel.DraftVersion.Filters);
            Assert.Equal(
                TimePeriodRangeViewModel.Create(draftDataSetVersion.MetaSummary!.TimePeriodRange),
                viewModel.DraftVersion.TimePeriods);
            Assert.Equal(
                draftDataSetVersion.MetaSummary!.Indicators,
                viewModel.DraftVersion.Indicators);
        }

        [Fact]
        public async Task ReleaseFilesWithSameFileId_Returns200_SameDataSetFileId()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                    .DefaultRelease(publishedVersions: 1, draftVersion: true)
                    .Generate(1)
                );

            File file = DataFixture.DefaultFile();

            var liveReleaseVersion = publication.ReleaseVersions.Single(rv => rv.Published is not null);

            var draftReleaseVersion = publication.ReleaseVersions.Single(rv => rv.Published is null);

            ReleaseFile liveReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(file)
                .WithReleaseVersion(liveReleaseVersion);

            ReleaseFile draftReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(file)
                .WithReleaseVersion(draftReleaseVersion);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.AddRange(liveReleaseFile, draftReleaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion liveDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusPublished()
                .WithReleaseFileId(liveReleaseFile.Id)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion draftDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusDraft()
                .WithVersionNumber(1, 1)
                .WithReleaseFileId(draftReleaseFile.Id)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(liveDataSetVersion, draftDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await GetDataSet(dataSet.Id);

            var viewModel = response.AssertOk<DataSetViewModel>();

            Assert.Equal(dataSet.Id, viewModel.Id);

            Assert.Equal(viewModel.LatestLiveVersion!.File.Id, viewModel.DraftVersion!.File.Id);
            Assert.Equal(liveReleaseFile.File.DataSetFileId, viewModel.LatestLiveVersion!.File.Id);
            Assert.Equal(draftReleaseFile.File.DataSetFileId, viewModel.DraftVersion!.File.Id);
        }

        [Fact]
        public async Task RequestedDataSetHasNoDraftVersion_Returns200_NoDraftVersion()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                        .DefaultRelease(publishedVersions: 1)
                        .Generate(1)
                );

            var releaseVersion = publication.ReleaseVersions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile())
                .WithReleaseVersion(releaseVersion);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(releaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusPublished()
                .WithReleaseFileId(releaseFile.Id)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await GetDataSet(dataSet.Id);

            var viewModel = response.AssertOk<DataSetViewModel>();

            Assert.Equal(dataSet.Id, viewModel.Id);
            Assert.Equal(dataSet.Title, viewModel.Title);
            Assert.Equal(dataSet.Summary, viewModel.Summary);
            Assert.Equal(dataSet.Status, viewModel.Status);

            Assert.Equal(dataSetVersion.Id, viewModel.LatestLiveVersion!.Id);
            Assert.Equal(dataSetVersion.Version, viewModel.LatestLiveVersion.Version);
            Assert.Equal(dataSetVersion.Status, viewModel.LatestLiveVersion.Status);
            Assert.Equal(dataSetVersion.VersionType, viewModel.LatestLiveVersion.Type);
            Assert.Equal(releaseFile.File.DataSetFileId, viewModel.LatestLiveVersion!.File.Id);
            Assert.Equal(releaseFile.Name, viewModel.LatestLiveVersion!.File.Title);

            Assert.Null(viewModel.DraftVersion);
        }

        [Fact]
        public async Task RequestedDataSetHasNoLiveVersion_Returns200_NoLiveVersion()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1)
                );

            var releaseVersion = publication.ReleaseVersions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile())
                .WithReleaseVersion(releaseVersion);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(releaseFile);
            });

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusDraft()
                .WithReleaseFileId(releaseFile.Id)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await GetDataSet(dataSet.Id);

            var viewModel = response.AssertOk<DataSetViewModel>();

            Assert.Equal(dataSet.Id, viewModel.Id);
            Assert.Equal(dataSet.Title, viewModel.Title);
            Assert.Equal(dataSet.Summary, viewModel.Summary);
            Assert.Equal(dataSet.Status, viewModel.Status);

            Assert.Equal(dataSetVersion.Id, viewModel.DraftVersion!.Id);
            Assert.Equal(dataSetVersion.Version, viewModel.DraftVersion.Version);
            Assert.Equal(dataSetVersion.Status, viewModel.DraftVersion.Status);
            Assert.Equal(dataSetVersion.VersionType, viewModel.DraftVersion.Type);
            Assert.Equal(releaseFile.File.DataSetFileId, viewModel.DraftVersion!.File.Id);
            Assert.Equal(releaseFile.Name, viewModel.DraftVersion!.File.Title);

            Assert.Null(viewModel.LatestLiveVersion);
        }

        [Fact]
        public async Task RequestedDataSetHasNoVersions_Returns200_NoLatestLiveVersionOrDraftVersion()
        {
            Publication publication = DataFixture.DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetDataSet(dataSet.Id);

            var viewModel = response.AssertOk<DataSetSummaryViewModel>();

            Assert.Equal(dataSet.Id, viewModel.Id);
            Assert.Equal(dataSet.Title, viewModel.Title);
            Assert.Equal(dataSet.Summary, viewModel.Summary);
            Assert.Equal(dataSet.Status, viewModel.Status);

            Assert.Null(viewModel.DraftVersion);
            Assert.Null(viewModel.LatestLiveVersion);
        }

        [Fact]
        public async Task NoPermissionsToViewPublication_Returns403()
        {
            Publication publication = DataFixture.DefaultPublication();

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var client = TestApp
                .SetUser(AuthenticatedUser())
                .CreateClient();

            var response = await GetDataSet(dataSetId: dataSet.Id, client: client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1)
                );

            var releaseVersion = publication.ReleaseVersions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile())
                .WithReleaseVersion(releaseVersion);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(releaseFile);
            });

            var response = await GetDataSet(Guid.NewGuid());

            response.AssertNotFound();
        }

        [Fact]
        public async Task PublicationDoesNotExist_Returns404()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1)
                );

            File file = DataFixture
                .DefaultFile();

            var releaseVersion = publication.ReleaseVersions.Single();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(file)
                .WithReleaseVersion(releaseVersion);

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(Guid.NewGuid());

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var response = await GetDataSet(dataSet.Id);

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSet(Guid dataSetId, HttpClient? client = null)
        {
            client ??= TestApp
                .SetUser(AuthenticatedUser(SecurityClaim(SecurityClaimTypes.AccessAllPublications)))
                .CreateClient();

            var uri = new Uri($"{BaseUrl}/{dataSetId}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class CreateDataSetTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        public static IEnumerable<object[]> AllDataSetVersionStatuses =>
            EnumUtil.GetEnums<DataSetVersionStatus>()
            .Select(e => new object[] { e });

        [Fact]
        public async Task Success()
        {
            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(
                    DataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1)
                );

            var draftReleaseVersion = publication.ReleaseVersions.Single();

            ReleaseFile draftReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture.DefaultFile())
                .WithReleaseVersion(draftReleaseVersion);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(draftReleaseFile);
            });

            DataSet? dataSet = null;
            DataSetVersion? dataSetVersion = null;

            var processorClient = new Mock<IProcessorClient>();
            processorClient
                .Setup(c => c.CreateInitialDataSetVersion(
                    draftReleaseFile.Id,
                    It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    dataSet = DataFixture
                        .DefaultDataSet()
                        .WithStatusPublished()
                        .WithPublicationId(publication.Id);

                    await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                    dataSetVersion = DataFixture
                        .DefaultDataSetVersion()
                        .WithStatusProcessing()
                        .WithReleaseFileId(draftReleaseFile.Id)
                        .WithDataSet(dataSet)
                        .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

                    await TestApp.AddTestData<PublicDataDbContext>(context =>
                    {
                        context.DataSetVersions.Add(dataSetVersion);
                        context.DataSets.Update(dataSet);
                    });

                    return new CreateInitialDataSetVersionResponseViewModel
                    {
                        DataSetId = dataSet!.Id,
                        DataSetVersionId = dataSetVersion!.Id,
                        InstanceId = Guid.NewGuid()
                    };
                });

            var client = BuildApp(processorClient.Object).CreateClient();

            var response = await CreateDataSetVersion(draftReleaseFile.Id, client);

            var content = response.AssertOk<DataSetViewModel>();

            Assert.NotNull(content);
            Assert.Equal(dataSet!.Id, content.Id);
            Assert.Equal(dataSet.Title, content.Title);
            Assert.Equal(dataSet.Status, content.Status);
            Assert.Equal(dataSet.Summary, content.Summary);
            Assert.Null(dataSet.LatestLiveVersion);
            Assert.Equal(dataSetVersion!.Id, content.DraftVersion!.Id);
            Assert.Equal(dataSetVersion.Version, content.DraftVersion!.Version);
            Assert.Equal(dataSetVersion.Status, content.DraftVersion!.Status);
            Assert.Equal(dataSetVersion.VersionType, content.DraftVersion!.Type);
            Assert.Equal(draftReleaseFile.File.DataSetFileId, content.DraftVersion!.File.Id);
            Assert.Equal(draftReleaseFile.Name, content.DraftVersion!.File.Title);
            Assert.Equal(draftReleaseVersion.Id, content.DraftVersion!.ReleaseVersion.Id);
            Assert.Equal(draftReleaseVersion.Title, content.DraftVersion!.ReleaseVersion.Title);
            Assert.Null(content.DraftVersion!.GeographicLevels);
            Assert.Null(content.DraftVersion!.TimePeriods);
            Assert.Null(content.DraftVersion!.Filters);
            Assert.Null(content.DraftVersion!.Indicators);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: AuthenticatedUser()).CreateClient();

            var response = await CreateDataSetVersion(Guid.NewGuid(), client);

            response.AssertForbidden();
        }

        [Theory]
        [MemberData(nameof(AllDataSetVersionStatuses))]
        public async Task DataSetVersionExistsForReleaseFile_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            var releaseFileId = Guid.NewGuid();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatus(dataSetVersionStatus)
                .WithReleaseFileId(releaseFileId)
                .WithDataSet(dataSet);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var response = await CreateDataSetVersion(releaseFileId);

            var validationProblem = response.AssertValidationProblem();

            var error = validationProblem.AssertHasError("releaseFileId", ValidationMessages.FileHasApiDataSetVersion.Code);

            var errorDetail = error.GetDetail<Dictionary<string, JsonElement>>();

            Assert.Equal(releaseFileId.ToString(), errorDetail["value"].GetString());
        }

        [Fact]
        public async Task ReleaseFileIdIsEmpty_Returns400()
        {
            var response = await CreateDataSetVersion(Guid.Empty);

            var validationProblem = response.AssertValidationProblem();

            var error = validationProblem.AssertHasNotEmptyError("releaseFileId");
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

        private async Task<HttpResponseMessage> CreateDataSetVersion(
            Guid releaseFileId,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var request = new DataSetVersionCreateRequest
            {
                ReleaseFileId = releaseFileId
            };

            return await client.PostAsJsonAsync(BaseUrl, request);
        }
    }
}
