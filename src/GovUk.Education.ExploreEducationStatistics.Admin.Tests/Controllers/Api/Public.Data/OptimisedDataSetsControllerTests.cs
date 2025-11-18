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
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

// ReSharper disable once ClassNeverInstantiated.Global
public class OptimisedDataSetsControllerTestsFixture : OptimisedHttpClientWithPsqlFixture;

[CollectionDefinition(nameof(OptimisedDataSetsControllerTestsFixture))]
public class OptimisedDataSetsControllerTestsCollection
    : ICollectionFixture<OptimisedDataSetsControllerTestsFixture>;

[Collection(nameof(OptimisedDataSetsControllerTestsFixture))]
public class OptimisedDataSetsControllerTests
{
    private static readonly DataFixture _dataFixture = new(new Random().Next());
    private const string BaseUrl = "api/public-data/data-sets";

    public class ListDataSetsTests(OptimisedDataSetsControllerTestsFixture fixture)
        : OptimisedDataSetsControllerTests
    {
        [Fact]
        public async Task PublicationHasSingleDataSet_Success_CorrectViewModel()
        {
            // Generate 4 Releases - one to contain the currently live data set version, one for the
            // current draft version, one for a published but not latest version, and one with no
            // data set versions at all. We use the Releases unconnected to the current live and
            // current draft data set versions to ensure that there are no unexpected side effects
            // of having extra ReleaseVersions present on the Publication.
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 1, draftVersion: true)
                        .Generate(4)
                );

            var releaseIds = publication
                .ReleaseVersions.Select(rv => rv.ReleaseId)
                .Distinct()
                .ToList();

            var oldPublishedReleaseVersion = publication.ReleaseVersions.First(rv =>
                rv.Published is not null && rv.ReleaseId == releaseIds[0]
            );

            var liveReleaseVersion = publication.ReleaseVersions.First(rv =>
                rv.Published is not null && rv.ReleaseId == releaseIds[1]
            );

            var draftReleaseVersion = publication.ReleaseVersions.First(rv =>
                rv.Published is null && rv.ReleaseId == releaseIds[2]
            );

            ReleaseFile oldPublishedReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(_dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(oldPublishedReleaseVersion);

            ReleaseFile liveReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(_dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(liveReleaseVersion);

            ReleaseFile draftReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(_dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(draftReleaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.AddRange(
                        oldPublishedReleaseFile,
                        liveReleaseFile,
                        draftReleaseFile
                    );
                });

            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion oldPublishedDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(oldPublishedReleaseFile.Id)
                )
                .WithVersionNumber(major: 1, minor: 0);

            DataSetVersion liveDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(liveReleaseFile.Id)
                )
                .WithVersionNumber(major: 2, minor: 0)
                .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

            DataSetVersion draftDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(draftReleaseFile.Id)
                )
                .WithVersionNumber(major: 2, minor: 1)
                .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(
                        oldPublishedDataSetVersion,
                        draftDataSetVersion,
                        liveDataSetVersion
                    );
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
                () =>
                    Assert.Equal(
                        draftDataSetVersion.PublicVersion,
                        dataSetViewModel.DraftVersion.Version
                    ),
                () =>
                    Assert.Equal(draftDataSetVersion.Status, dataSetViewModel.DraftVersion.Status),
                () =>
                    Assert.Equal(
                        draftDataSetVersion.VersionType,
                        dataSetViewModel.DraftVersion.Type
                    ),
                () =>
                    Assert.Equal(
                        draftReleaseVersion.Id,
                        dataSetViewModel.DraftVersion.ReleaseVersion.Id
                    ),
                () =>
                    Assert.Equal(
                        draftReleaseVersion.Release.Title,
                        dataSetViewModel.DraftVersion.ReleaseVersion.Title
                    ),
                () => Assert.Equal(liveDataSetVersion.Id, dataSetViewModel.LatestLiveVersion.Id),
                () =>
                    Assert.Equal(
                        liveDataSetVersion.PublicVersion,
                        dataSetViewModel.LatestLiveVersion.Version
                    ),
                () =>
                    Assert.Equal(
                        liveDataSetVersion.Status,
                        dataSetViewModel.LatestLiveVersion.Status
                    ),
                () =>
                    Assert.Equal(
                        liveDataSetVersion.VersionType,
                        dataSetViewModel.LatestLiveVersion.Type
                    ),
                () =>
                    Assert.Equal(
                        liveReleaseVersion.Id,
                        dataSetViewModel.LatestLiveVersion.ReleaseVersion.Id
                    ),
                () =>
                    Assert.Equal(
                        liveReleaseVersion.Release.Title,
                        dataSetViewModel.LatestLiveVersion.ReleaseVersion.Title
                    ),
                () =>
                    Assert.Equal(
                        liveDataSetVersion.Published.TruncateNanoseconds(),
                        dataSetViewModel.LatestLiveVersion?.Published
                    ),
                () =>
                    Assert.Equal(
                        new[]
                        {
                            oldPublishedReleaseVersion.ReleaseId,
                            liveReleaseVersion.ReleaseId,
                            draftReleaseVersion.ReleaseId,
                        }.Order(),
                        dataSetViewModel.PreviousReleaseIds.Order()
                    )
            );
        }

        [Theory]
        [InlineData(1, 10, 2)]
        [InlineData(1, 2, 10)]
        [InlineData(2, 2, 10)]
        public async Task PublicationHasMultipleDataSets_Success_CorrectPaging(
            int page,
            int pageSize,
            int numberOfAvailableDataSets
        )
        {
            Publication publication = _dataFixture.DefaultPublication();

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .ForInstance(s =>
                    s.Set(
                        rf => rf.ReleaseVersion,
                        () =>
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture.DefaultRelease().WithPublication(publication)
                                )
                    )
                )
                .ForInstance(s =>
                    s.Set(rf => rf.File, () => _dataFixture.DefaultFile(FileType.Data))
                )
                .GenerateList(numberOfAvailableDataSets);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.AddRange(releaseFiles);
                });

            var dataSets = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id)
                .GenerateList(numberOfAvailableDataSets);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.AddRange(dataSets));

            var dataSetVersions = dataSets
                .Select(
                    (ds, index) =>
                        _dataFixture
                            .DefaultDataSetVersion(
                                filters: 1,
                                indicators: 1,
                                locations: 1,
                                timePeriods: 2
                            )
                            .WithStatusPublished()
                            .WithDataSet(ds)
                            .WithRelease(
                                _dataFixture
                                    .DefaultDataSetVersionRelease()
                                    .WithReleaseFileId(releaseFiles[index].Id)
                            )
                            .FinishWith(dsv => ds.LatestLiveVersion = dsv)
                            .Generate()
                )
                .ToList();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
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

            var response = await ListPublicationDataSets(
                publicationId: publication.Id,
                page: page,
                pageSize: pageSize
            );

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

            pagedResult.AssertHasExpectedPagingAndResultCount(
                expectedTotalResults: numberOfAvailableDataSets,
                expectedPage: page,
                expectedPageSize: pageSize
            );

            Assert.Equal(expectedDataSetIds.Count, pagedResult.Results.Count);
            Assert.Equal(expectedDataSetIds, pagedResult.Results.Select(ds => ds.Id));
        }

        [Fact]
        public async Task PublicationHasMultipleDataSets_Success_CorrectOrdering()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .ForInstance(s =>
                    s.Set(
                        rf => rf.ReleaseVersion,
                        () =>
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture.DefaultRelease().WithPublication(publication)
                                )
                    )
                )
                .ForInstance(s =>
                    s.Set(rf => rf.File, () => _dataFixture.DefaultFile(FileType.Data))
                )
                .GenerateList(7);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.AddRange(releaseFiles);
                });

            var dataSets = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id)
                .ForIndex(3, s => s.SetTitle("Dataset B"))
                .ForIndex(5, s => s.SetTitle("Dataset A"))
                .GenerateList(6);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.AddRange(dataSets));

            var dataSetVersions = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .ForIndex(
                    0,
                    s =>
                    {
                        // Associate data set 0 with a live version published 3 days ago
                        s.SetDataSet(dataSets[0]);
                        s.SetStatus(DataSetVersionStatus.Deprecated);
                        s.SetPublished(DateTimeOffset.UtcNow.AddDays(-3));
                        s.SetRelease(
                            _dataFixture
                                .DefaultDataSetVersionRelease()
                                .WithReleaseFileId(releaseFiles[0].Id)
                        );
                    }
                )
                .ForIndex(
                    1,
                    s =>
                    {
                        // Associate data set 1 with a live version published 4 days ago
                        s.SetDataSet(dataSets[1]);
                        s.SetStatus(DataSetVersionStatus.Published);
                        s.SetPublished(DateTimeOffset.UtcNow.AddDays(-4));
                        s.SetRelease(
                            _dataFixture
                                .DefaultDataSetVersionRelease()
                                .WithReleaseFileId(releaseFiles[1].Id)
                        );
                    }
                )
                .ForIndex(
                    2,
                    s =>
                    {
                        // Also associate data set 1 with a draft version that should have no bearing on the ordering
                        s.SetDataSet(dataSets[1]);
                        s.SetStatusDraft();
                        s.SetRelease(
                            _dataFixture
                                .DefaultDataSetVersionRelease()
                                .WithReleaseFileId(releaseFiles[2].Id)
                        );
                    }
                )
                .ForIndex(
                    3,
                    s =>
                    {
                        // Associate dataset 2 with a live version published 1 day ago
                        s.SetDataSet(dataSets[2]);
                        s.SetStatus(DataSetVersionStatus.Deprecated);
                        s.SetPublished(DateTimeOffset.UtcNow.AddDays(-1));
                        s.SetRelease(
                            _dataFixture
                                .DefaultDataSetVersionRelease()
                                .WithReleaseFileId(releaseFiles[3].Id)
                        );
                    }
                )
                .ForIndex(
                    4,
                    s =>
                    {
                        // Associate dataset 3 with a draft version that should have no bearing on the ordering
                        s.SetDataSet(dataSets[3]);
                        s.SetStatusDraft();
                        s.SetRelease(
                            _dataFixture
                                .DefaultDataSetVersionRelease()
                                .WithReleaseFileId(releaseFiles[4].Id)
                        );
                    }
                )
                .ForIndex(
                    5,
                    s =>
                    {
                        // Associate dataset 4 with a live version published 2 days ago
                        s.SetDataSet(dataSets[4]);
                        s.SetStatus(DataSetVersionStatus.Published);
                        s.SetPublished(DateTimeOffset.UtcNow.AddDays(-2));
                        s.SetRelease(
                            _dataFixture
                                .DefaultDataSetVersionRelease()
                                .WithReleaseFileId(releaseFiles[5].Id)
                        );
                    }
                )
                .ForIndex(
                    6,
                    s =>
                    {
                        // Associate dataset 5 with a draft version that should have no bearing on the ordering
                        s.SetDataSet(dataSets[5]);
                        s.SetStatusDraft();
                        s.SetRelease(
                            _dataFixture
                                .DefaultDataSetVersionRelease()
                                .WithReleaseFileId(releaseFiles[6].Id)
                        );
                    }
                )
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

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
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
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.UnavailableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task PublicationHasSingleDataSetWithoutLiveVersion_LatestLiveVersionIsEmpty(
            DataSetVersionStatus dataSetVersionStatus
        )
        {
            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(
                            _dataFixture
                                .DefaultRelease()
                                .WithPublication(_dataFixture.DefaultPublication())
                        )
                )
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusDraft()
                .WithPublicationId(releaseFile.ReleaseVersion.PublicationId);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                )
                .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListPublicationDataSets(releaseFile.ReleaseVersion.PublicationId);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

            pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: 1);

            var dataSetViewModel = pagedResult.Results.Single();

            Assert.Null(dataSetViewModel.LatestLiveVersion);
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Deprecated)]
        [InlineData(DataSetVersionStatus.Published)]
        public async Task PublicationHasSingleDataSetWithoutDraftVersion_DraftVersionIsEmpty(
            DataSetVersionStatus dataSetVersionStatus
        )
        {
            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    _dataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(
                            _dataFixture
                                .DefaultRelease()
                                .WithPublication(_dataFixture.DefaultPublication())
                        )
                )
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(releaseFile.ReleaseVersion.PublicationId);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                )
                .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListPublicationDataSets(releaseFile.ReleaseVersion.PublicationId);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

            pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: 1);

            var dataSetViewModel = pagedResult.Results.Single();

            Assert.Null(dataSetViewModel.DraftVersion);
        }

        [Fact]
        public async Task PublicationHasNoDataSets_ReturnsEmpty()
        {
            Publication publication = _dataFixture.DefaultPublication();

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Publications.Add(publication));

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
            Publication publication = _dataFixture.DefaultPublication();

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Publications.Add(publication));

            var client = fixture.CreateClient().WithUser(TestUser.Authenticated);

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
            Publication publication = _dataFixture.DefaultPublication();

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Publications.Add(publication));

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
            var response = await ListPublicationDataSets(
                publicationId: Guid.NewGuid(),
                pageSize: pageSize
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasInclusiveBetweenError("pageSize", from: 1, to: 100);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public async Task PageSizeInAllowedRange_Success(int pageSize)
        {
            Publication publication = _dataFixture.DefaultPublication();

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Publications.Add(publication));

            var response = await ListPublicationDataSets(publication.Id, pageSize: pageSize);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetSummaryViewModel>>();

            pagedResult.AssertHasPagingConsistentWithEmptyResults(expectedPageSize: pageSize);
        }

        private async Task<HttpResponseMessage> ListPublicationDataSets(
            Guid publicationId,
            int? page = null,
            int? pageSize = null,
            HttpClient? client = null
        )
        {
            var user = _dataFixture
                .AuthenticatedUser()
                .WithClaim(SecurityClaimTypes.AccessAllPublications.ToString());

            client ??= fixture.CreateClient().WithUser(TestUser.Bau);

            var queryParams = new Dictionary<string, string?>
            {
                { "page", page?.ToString() },
                { "pageSize", pageSize?.ToString() },
                { "publicationId", publicationId.ToString() },
            };

            var uri = QueryHelpers.AddQueryString(BaseUrl, queryParams);

            return await client.GetAsync(uri);
        }
    }

    public class GetDataSetTests(OptimisedDataSetsControllerTestsFixture fixture)
        : OptimisedDataSetsControllerTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 1, draftVersion: true)
                        .Generate(3)
                );

            var liveReleaseVersion = publication.ReleaseVersions.First(rv =>
                rv.Published is not null
            );

            var draftReleaseVersion = publication.ReleaseVersions.First(rv =>
                rv.Published is null && rv.ReleaseId != liveReleaseVersion.ReleaseId
            );

            ReleaseFile liveReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(_dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(liveReleaseVersion);

            ReleaseFile draftReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(_dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(draftReleaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.AddRange(liveReleaseFile, draftReleaseFile);
                });

            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion liveDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusPublished()
                .WithTotalResults(5000)
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(liveReleaseFile.Id)
                )
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion draftDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusDraft()
                .WithVersionNumber(1, 1)
                .WithTotalResults(6000)
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(draftReleaseFile.Id)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = liveDataSetVersion.Id,
                TargetDataSetVersionId = draftDataSetVersion.Id,
                LocationMappingPlan = new LocationMappingPlan(),
                FilterMappingPlan = new FilterMappingPlan(),
                LocationMappingsComplete = false,
                FilterMappingsComplete = true,
            };

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(liveDataSetVersion, draftDataSetVersion);
                    context.DataSets.Update(dataSet);
                    context.DataSetVersionMappings.Add(mappings);
                });

            var response = await GetDataSet(dataSet.Id);

            var viewModel = response.AssertOk<DataSetViewModel>();

            Assert.Equal(dataSet.Id, viewModel.Id);
            Assert.Equal(dataSet.Title, viewModel.Title);
            Assert.Equal(dataSet.Summary, viewModel.Summary);
            Assert.Equal(dataSet.Status, viewModel.Status);

            Assert.Equal(liveDataSetVersion.Id, viewModel.LatestLiveVersion!.Id);
            Assert.Equal(liveDataSetVersion.PublicVersion, viewModel.LatestLiveVersion.Version);
            Assert.Equal(liveDataSetVersion.Status, viewModel.LatestLiveVersion.Status);
            Assert.Equal(liveDataSetVersion.VersionType, viewModel.LatestLiveVersion.Type);
            Assert.Equal(liveDataSetVersion.TotalResults, viewModel.LatestLiveVersion.TotalResults);
            Assert.Equal(
                liveDataSetVersion.Published.TruncateNanoseconds(),
                viewModel.LatestLiveVersion.Published
            );
            Assert.Equal(liveReleaseFile.File.DataSetFileId, viewModel.LatestLiveVersion.File.Id);
            Assert.Equal(liveReleaseFile.Name, viewModel.LatestLiveVersion.File.Title);

            Assert.Equal(
                liveDataSetVersion.MetaSummary!.GeographicLevels.Select(l => l.GetEnumLabel()),
                viewModel.LatestLiveVersion.GeographicLevels
            );
            Assert.Equal(
                liveDataSetVersion.MetaSummary!.Filters,
                viewModel.LatestLiveVersion.Filters
            );
            Assert.Equal(
                TimePeriodRangeViewModel.Create(liveDataSetVersion.MetaSummary!.TimePeriodRange),
                viewModel.LatestLiveVersion.TimePeriods
            );
            Assert.Equal(
                liveDataSetVersion.MetaSummary!.Indicators,
                viewModel.LatestLiveVersion.Indicators
            );

            Assert.Equal(liveReleaseVersion.Id, viewModel.LatestLiveVersion.ReleaseVersion.Id);
            Assert.Equal(
                liveReleaseVersion.Release.Title,
                viewModel.LatestLiveVersion.ReleaseVersion.Title
            );

            Assert.Equal(draftDataSetVersion.Id, viewModel.DraftVersion!.Id);
            Assert.Equal(draftDataSetVersion.PublicVersion, viewModel.DraftVersion.Version);
            Assert.Equal(draftDataSetVersion.Status, viewModel.DraftVersion.Status);
            Assert.Equal(draftDataSetVersion.VersionType, viewModel.DraftVersion.Type);
            Assert.Equal(draftDataSetVersion.TotalResults, viewModel.DraftVersion.TotalResults);
            Assert.Equal(draftReleaseFile.File.DataSetFileId, viewModel.DraftVersion.File.Id);
            Assert.Equal(draftReleaseFile.Name, viewModel.DraftVersion.File.Title);

            Assert.Equal(draftReleaseVersion.Id, viewModel.DraftVersion.ReleaseVersion.Id);
            Assert.Equal(
                draftReleaseVersion.Release.Title,
                viewModel.DraftVersion.ReleaseVersion.Title
            );
            Assert.Equal(
                draftDataSetVersion.MetaSummary!.GeographicLevels.Select(l => l.GetEnumLabel()),
                viewModel.DraftVersion.GeographicLevels
            );
            Assert.Equal(draftDataSetVersion.MetaSummary!.Filters, viewModel.DraftVersion.Filters);
            Assert.Equal(
                TimePeriodRangeViewModel.Create(draftDataSetVersion.MetaSummary!.TimePeriodRange),
                viewModel.DraftVersion.TimePeriods
            );
            Assert.Equal(
                draftDataSetVersion.MetaSummary!.Indicators,
                viewModel.DraftVersion.Indicators
            );

            Assert.Equal(
                new[] { draftReleaseVersion.ReleaseId, liveReleaseVersion.ReleaseId }.Order(),
                viewModel.PreviousReleaseIds.Order()
            );

            Assert.NotNull(viewModel.DraftVersion.MappingStatus);
            Assert.False(viewModel.DraftVersion.MappingStatus.LocationsComplete);
            Assert.True(viewModel.DraftVersion.MappingStatus.FiltersComplete);
        }

        [Fact]
        public async Task ReleaseFilesWithSameFileId_Returns200_SameDataSetFileId()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 1, draftVersion: true)
                        .Generate(1)
                );

            File file = _dataFixture.DefaultFile(FileType.Data);

            var liveReleaseVersion = publication.ReleaseVersions.Single(rv =>
                rv.Published is not null
            );

            var draftReleaseVersion = publication.ReleaseVersions.Single(rv =>
                rv.Published is null
            );

            ReleaseFile liveReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(file)
                .WithReleaseVersion(liveReleaseVersion);

            ReleaseFile draftReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(file)
                .WithReleaseVersion(draftReleaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.AddRange(liveReleaseFile, draftReleaseFile);
                });

            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion liveDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(liveReleaseFile.Id)
                )
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion draftDataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusDraft()
                .WithVersionNumber(1, 1)
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture
                        .DefaultDataSetVersionRelease()
                        .WithReleaseFileId(draftReleaseFile.Id)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
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
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1).Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single();

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(_dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                )
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
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
            Assert.Equal(new[] { releaseVersion.ReleaseId }, viewModel.PreviousReleaseIds);

            Assert.Equal(dataSetVersion.Id, viewModel.LatestLiveVersion!.Id);
            Assert.Equal(dataSetVersion.PublicVersion, viewModel.LatestLiveVersion.Version);
            Assert.Equal(dataSetVersion.Status, viewModel.LatestLiveVersion.Status);
            Assert.Equal(dataSetVersion.VersionType, viewModel.LatestLiveVersion.Type);
            Assert.Equal(releaseFile.File.DataSetFileId, viewModel.LatestLiveVersion!.File.Id);
            Assert.Equal(releaseFile.Name, viewModel.LatestLiveVersion!.File.Title);

            Assert.Null(viewModel.DraftVersion);
        }

        [Fact]
        public async Task RequestedDataSetHasNoLiveVersion_Returns200_NoLiveVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1)
                );

            var releaseVersion = publication.ReleaseVersions.Single();

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(_dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = _dataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(
                    _dataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id)
                )
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
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
            Assert.Equal(new[] { releaseVersion.ReleaseId }, viewModel.PreviousReleaseIds);

            Assert.Equal(dataSetVersion.Id, viewModel.DraftVersion!.Id);
            Assert.Equal(dataSetVersion.PublicVersion, viewModel.DraftVersion.Version);
            Assert.Equal(dataSetVersion.Status, viewModel.DraftVersion.Status);
            Assert.Equal(dataSetVersion.VersionType, viewModel.DraftVersion.Type);
            Assert.Equal(releaseFile.File.DataSetFileId, viewModel.DraftVersion!.File.Id);
            Assert.Equal(releaseFile.Name, viewModel.DraftVersion!.File.Title);
            Assert.Null(viewModel.DraftVersion.MappingStatus);

            Assert.Null(viewModel.LatestLiveVersion);
        }

        [Fact]
        public async Task RequestedDataSetHasNoVersions_Returns200_NoLatestLiveVersionOrDraftVersion()
        {
            Publication publication = _dataFixture.DefaultPublication();

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Publications.Add(publication));

            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            var response = await GetDataSet(dataSet.Id);

            var viewModel = response.AssertOk<DataSetSummaryViewModel>();

            Assert.Equal(dataSet.Id, viewModel.Id);
            Assert.Equal(dataSet.Title, viewModel.Title);
            Assert.Equal(dataSet.Summary, viewModel.Summary);
            Assert.Equal(dataSet.Status, viewModel.Status);
            Assert.Empty(viewModel.PreviousReleaseIds);

            Assert.Null(viewModel.DraftVersion);
            Assert.Null(viewModel.LatestLiveVersion);
        }

        [Fact]
        public async Task NoPermissionsToViewPublication_Returns403()
        {
            Publication publication = _dataFixture.DefaultPublication();

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.Publications.Add(publication));

            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publication.Id);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            var client = fixture.CreateClient().WithUser(TestUser.Authenticated);

            var response = await GetDataSet(dataSetId: dataSet.Id, client: client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1)
                );

            var releaseVersion = publication.ReleaseVersions.Single();

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(_dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
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
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1)
                );

            File file = _dataFixture.DefaultFile(FileType.Data);

            var releaseVersion = publication.ReleaseVersions.Single();

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(file)
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context => context.ReleaseFiles.Add(releaseFile));

            DataSet dataSet = _dataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(Guid.NewGuid());

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.Add(dataSet));

            var response = await GetDataSet(dataSet.Id);

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSet(
            Guid dataSetId,
            HttpClient? client = null
        )
        {
            var user = _dataFixture
                .AuthenticatedUser()
                .WithClaim(SecurityClaimTypes.AccessAllPublications.ToString());

            client ??= fixture.CreateClient().WithUser(TestUser.Bau);

            var uri = new Uri($"{BaseUrl}/{dataSetId}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class CreateDataSetTests(OptimisedDataSetsControllerTestsFixture fixture)
        : OptimisedDataSetsControllerTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(
                    _dataFixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1)
                );

            var draftReleaseVersion = publication.ReleaseVersions.Single();

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(_dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(draftReleaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet? dataSet = null;
            DataSetVersion? dataSetVersion = null;

            var processorClient = fixture.GetProcessorClientMock();

            processorClient
                .Setup(c => c.CreateDataSet(releaseFile.Id, It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    dataSet = _dataFixture
                        .DefaultDataSet()
                        .WithStatusPublished()
                        .WithPublicationId(publication.Id);

                    await fixture
                        .GetPublicDataDbContext()
                        .AddTestData(context => context.DataSets.Add(dataSet));

                    dataSetVersion = _dataFixture
                        .DefaultDataSetVersion()
                        .WithStatusProcessing()
                        .WithRelease(
                            _dataFixture
                                .DefaultDataSetVersionRelease()
                                .WithReleaseFileId(releaseFile.Id)
                        )
                        .WithDataSet(dataSet)
                        .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

                    await fixture
                        .GetPublicDataDbContext()
                        .AddTestData(context =>
                        {
                            context.DataSetVersions.Add(dataSetVersion);
                            context.DataSets.Update(dataSet);
                        });

                    return new ProcessDataSetVersionResponseViewModel
                    {
                        DataSetId = dataSet.Id,
                        DataSetVersionId = dataSetVersion.Id,
                        InstanceId = Guid.NewGuid(),
                    };
                });

            var client = fixture.CreateClient().WithUser(TestUser.Bau);

            var response = await CreateDataSet(releaseFile.Id, client);

            MockUtils.VerifyAllMocks(processorClient);

            var content = response.AssertOk<DataSetViewModel>();

            Assert.NotNull(content);
            Assert.Equal(dataSet!.Id, content.Id);
            Assert.Equal(dataSet.Title, content.Title);
            Assert.Equal(dataSet.Status, content.Status);
            Assert.Equal(dataSet.Summary, content.Summary);
            Assert.Null(dataSet.LatestLiveVersion);
            Assert.Equal(dataSetVersion!.Id, content.DraftVersion!.Id);
            Assert.Equal(dataSetVersion.PublicVersion, content.DraftVersion!.Version);
            Assert.Equal(dataSetVersion.Status, content.DraftVersion!.Status);
            Assert.Equal(dataSetVersion.VersionType, content.DraftVersion!.Type);
            Assert.Equal(releaseFile.File.DataSetFileId, content.DraftVersion!.File.Id);
            Assert.Equal(releaseFile.Name, content.DraftVersion!.File.Title);
            Assert.Equal(draftReleaseVersion.Id, content.DraftVersion!.ReleaseVersion.Id);
            Assert.Equal(
                draftReleaseVersion.Release.Title,
                content.DraftVersion!.ReleaseVersion.Title
            );
            Assert.Null(content.DraftVersion!.GeographicLevels);
            Assert.Null(content.DraftVersion!.TimePeriods);
            Assert.Null(content.DraftVersion!.Filters);
            Assert.Null(content.DraftVersion!.Indicators);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = fixture.CreateClient().WithUser(TestUser.Authenticated);

            var response = await CreateDataSet(Guid.NewGuid(), client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task ProcessorReturns400_Returns400_WithProcessorErrors()
        {
            var releaseFileId = Guid.NewGuid();
            var processorClient = fixture.GetProcessorClientMock();

            ErrorViewModel[] processorErrors =
            [
                new ErrorViewModel
                {
                    Code = "TestError1",
                    Message = "Test message 1",
                    Path = "releaseFileId",
                },
                new ErrorViewModel
                {
                    Code = "TestError2",
                    Message = "Test message 2",
                    Path = "releaseFileId",
                },
            ];

            processorClient
                .Setup(c => c.CreateDataSet(releaseFileId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidationUtils.ValidationResult(processorErrors));

            var client = fixture.CreateClient().WithUser(TestUser.Bau);
            var response = await CreateDataSet(releaseFileId, client);

            MockUtils.VerifyAllMocks(processorClient);

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(processorErrors, validationProblem.Errors);
        }

        private async Task<HttpResponseMessage> CreateDataSet(
            Guid releaseFileId,
            HttpClient? client = null
        )
        {
            client ??= fixture.CreateClient().WithUser(TestUser.Bau);

            var request = new DataSetCreateRequest { ReleaseFileId = releaseFileId };

            return await client.PostAsJsonAsync(BaseUrl, request);
        }
    }
}
