#nullable enable
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

// ReSharper disable once ClassNeverInstantiated.Global
public class DataSetVersionsControllerTestsFixture()
    : OptimisedAdminCollectionFixture(
        capabilities: [AdminIntegrationTestCapability.UserAuth, AdminIntegrationTestCapability.Postgres]
    );

[CollectionDefinition(nameof(DataSetVersionsControllerTestsFixture))]
public class DataSetVersionsControllerTestsCollection : ICollectionFixture<DataSetVersionsControllerTestsFixture>;

[Collection(nameof(DataSetVersionsControllerTestsFixture))]
public abstract class DataSetVersionsControllerTests(DataSetVersionsControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private const string BaseUrl = "api/public-data/data-set-versions";
    private static readonly DataFixture DataFixture = new();

    public class ListVersionsTests(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.AvailableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task BauUser_OnlyPreviouslyPublishedVersionsReturned(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .WithPublished(DateTimeOffset.UtcNow)
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListLiveVersions(dataSetId: dataSet.Id, page: 1, pageSize: 10);

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
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.UnavailableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task DraftVersionsNotReturned(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListLiveVersions(dataSetId: dataSet.Id, page: 1, pageSize: 10);

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
        public async Task ResultsArePaginatedCorrectly(int page, int pageSize, int numberOfPublishedDataSetVersions)
        {
            var releaseFiles = DataFixture
                .DefaultReleaseFile()
                .ForInstance(s =>
                    s.Set(
                        rf => rf.ReleaseVersion,
                        () =>
                            DataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication())
                                )
                    )
                )
                .ForInstance(s => s.Set(rf => rf.File, () => DataFixture.DefaultFile(FileType.Data)))
                .GenerateList(numberOfPublishedDataSetVersions);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFiles);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            var now = DateTimeOffset.UtcNow.AddDays(-numberOfPublishedDataSetVersions);
            var dataSetVersions = releaseFiles
                .Select(
                    (rf, index) =>
                        DataFixture
                            .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                            .WithVersionNumber(major: 1, minor: index, patch: 1)
                            .WithStatusPublished()
                            .WithPublished(now.AddDays(index))
                            .WithDataSet(dataSet)
                            .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(rf.Id))
                            .Generate()
                )
                .ToList();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersions);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListLiveVersions(dataSetId: dataSet.Id, page: page, pageSize: pageSize);

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
            ReleaseFile targetReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            ReleaseFile otherReleaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.AddRange(targetReleaseFile, otherReleaseFile);
                });

            DataSet targetDataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            DataSet otherDataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context => context.DataSets.AddRange(targetDataSet, otherDataSet));

            DataSetVersion targetDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(targetDataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(targetReleaseFile.Id))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion otherDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(otherDataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(otherReleaseFile.Id))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(targetDataSetVersion, otherDataSetVersion);
                    context.DataSets.UpdateRange(targetDataSet, otherDataSet);
                });

            var response = await ListLiveVersions(dataSetId: targetDataSet.Id, page: 1, pageSize: 10);

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
        public async Task NotBauUserAndNotOnPublicationTeam_Returns403()
        {
            // The data set must actually exist - otherwise the request is rejected as Not Found
            // before the authorization check is ever reached.
            DataSet dataSet = DataFixture.DefaultDataSet();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            var response = await ListLiveVersions(
                dataSetId: dataSet.Id,
                page: 1,
                pageSize: 1,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task UserOnPublicationTeam_CanListLiveVersions(PublicationRole publicationRole)
        {
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            Publication publication = DataFixture.DefaultPublication();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(publication))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            UserPublicationRole userPublicationRole = DataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRole);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                    context.UserPublicationRoles.Add(userPublicationRole);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished().WithPublicationId(publication.Id);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithStatusPublished()
                .WithPublished(DateTimeOffset.UtcNow)
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            // The DataSet <-> DataSetVersion relationship is circular once LatestLiveVersion is set, so
            // this must remain a two-step Add-then-Update rather than a single combined AddTestData call.
            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await ListLiveVersions(dataSetId: dataSet.Id, page: 1, pageSize: 10, user: identityUser);

            var viewModel = response.AssertOk<PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>>();

            Assert.Equal(dataSetVersion.Id, Assert.Single(viewModel.Results).Id);
        }

        [Fact]
        public async Task UserWithOnlyPreReleaseRole_Returns403()
        {
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            Publication publication = DataFixture.DefaultPublication();

            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease().WithPublication(publication));

            UserPreReleaseRole userPreReleaseRole = DataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseVersions.Add(releaseVersion);
                    context.UserPreReleaseRoles.Add(userPreReleaseRole);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithPublicationId(publication.Id);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            var response = await ListLiveVersions(dataSetId: dataSet.Id, page: 1, pageSize: 10, user: identityUser);

            response.AssertForbidden();
        }

        [Fact]
        public async Task NoDataSetId_Returns400()
        {
            var client = fixture.CreateClient(user: OptimisedTestUsers.Bau);

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
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var queryParams = new Dictionary<string, string?>
            {
                { "dataSetId", dataSetId.ToString() },
                { "page", page?.ToString() },
                { "pageSize", pageSize?.ToString() },
            };

            var uri = QueryHelpers.AddQueryString(BaseUrl, queryParams);

            return await client.GetAsync(uri);
        }
    }

    public class GetDataSetVersionTests(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        public static TheoryData<DataSetVersionStatus> AllDataSetVersionStatuses =>
            new(EnumUtil.GetEnums<DataSetVersionStatus>());

        [Theory]
        [MemberData(nameof(AllDataSetVersionStatuses))]
        public async Task BauUser_Success(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            var dataSetVersions = DataFixture
                .DefaultDataSetVersion()
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet)
                .GenerateList(3);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersions);
                    context.DataSets.Update(dataSet);
                });

            var requestedDataSetVersion = dataSetVersions[1];

            var response = await GetDataSetVersion(dataSetVersionId: requestedDataSetVersion.Id);

            var viewModel = response.AssertOk<DataSetVersionInfoViewModel>();

            Assert.NotNull(viewModel);
            Assert.Equal(requestedDataSetVersion.Id, viewModel.Id);
            Assert.Equal(requestedDataSetVersion.PublicVersion, viewModel.Version);
            Assert.Equal(requestedDataSetVersion.Status, viewModel.Status);
            Assert.Equal(requestedDataSetVersion.VersionType, viewModel.Type);
            Assert.Equal(requestedDataSetVersion.Notes, viewModel.Notes);
        }

        [Fact]
        public async Task NotBauUserAndNotOnPublicationTeam_Returns403()
        {
            // The data set version must actually exist - otherwise the request is rejected as
            // Not Found before the authorization check is ever reached.
            DataSet dataSet = DataFixture.DefaultDataSet();

            DataSetVersion dataSetVersion = DataFixture.DefaultDataSetVersion().WithDataSet(dataSet).Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(dataSetVersion);
                });

            var response = await GetDataSetVersion(
                dataSetVersionId: dataSetVersion.Id,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionDoesNotExist_Returns404()
        {
            var response = await GetDataSetVersion(dataSetVersionId: Guid.NewGuid());

            response.AssertNotFound();
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task UserOnPublicationTeam_CanGetDataSetVersion(PublicationRole publicationRole)
        {
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            Publication publication = DataFixture.DefaultPublication();

            UserPublicationRole userPublicationRole = DataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRole);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.UserPublicationRoles.Add(userPublicationRole);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished().WithPublicationId(publication.Id);

            DataSetVersion dataSetVersion = DataFixture.DefaultDataSetVersion().WithDataSet(dataSet).Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(dataSetVersion);
                });

            var response = await GetDataSetVersion(dataSetVersionId: dataSetVersion.Id, user: identityUser);

            var viewModel = response.AssertOk<DataSetVersionInfoViewModel>();

            Assert.Equal(dataSetVersion.Id, viewModel.Id);
        }

        [Fact]
        public async Task UserWithOnlyPreReleaseRole_Returns403()
        {
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            Publication publication = DataFixture
                .DefaultPublication()
                .WithReleases(DataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true).Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single();

            UserPreReleaseRole userPreReleaseRole = DataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.UserPreReleaseRoles.Add(userPreReleaseRole);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished().WithPublicationId(publication.Id);

            DataSetVersion dataSetVersion = DataFixture.DefaultDataSetVersion().WithDataSet(dataSet).Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(dataSetVersion);
                });

            var response = await GetDataSetVersion(dataSetVersionId: dataSetVersion.Id, user: identityUser);

            response.AssertForbidden();
        }

        private async Task<HttpResponseMessage> GetDataSetVersion(Guid dataSetVersionId, ClaimsPrincipal? user = null)
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var uri = $"{BaseUrl}/{dataSetVersionId}";

            return await client.GetAsync(uri);
        }
    }

    public class CreateNextVersionTests(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(currentDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            DataSetVersion? nextVersion = null;

            var processorClientMock = fixture.GetProcessorClientMock();

            processorClientMock
                .Setup(c =>
                    c.CreateNextDataSetVersionMappings(dataSet.Id, releaseFile.Id, null, It.IsAny<CancellationToken>())
                )
                .Returns(async () =>
                {
                    var savedDataSet = await fixture
                        .GetPublicDataDbContext()
                        .DataSets.SingleAsync(ds => ds.Id == dataSet.Id);

                    nextVersion = DataFixture
                        .DefaultDataSetVersion()
                        .WithStatusMapping()
                        .WithVersionNumber(major: 1, minor: 1)
                        .WithDataSet(savedDataSet)
                        .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                        .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

                    await fixture
                        .GetPublicDataDbContext()
                        .AddTestData(context =>
                        {
                            context.DataSetVersions.Add(nextVersion);
                            context.DataSets.Update(savedDataSet);
                        });

                    return new ProcessDataSetVersionResponseViewModel
                    {
                        DataSetId = dataSet.Id,
                        DataSetVersionId = nextVersion.Id,
                        InstanceId = Guid.NewGuid(),
                    };
                });

            var response = await CreateNextVersion(dataSetId: dataSet.Id, releaseFileId: releaseFile.Id);

            MockUtils.VerifyAllMocks(processorClientMock);

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
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            var response = await CreateNextVersion(
                dataSetId: Guid.NewGuid(),
                releaseFileId: releaseFile.Id,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        // This behaviour is intentional for now - a publication role does not yet satisfy
        // CanManagePublicApiDataSets, which is currently BAU-only. Revisit this test once
        // ManagePublicApiDataSetsAuthorizationHandler gains publication-role support (see the
        // TODO comment in that handler).
        [Fact]
        public async Task UserOnPublicationTeam_Returns403()
        {
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            UserPublicationRole userPublicationRole = DataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(releaseFile.ReleaseVersion.Release.Publication)
                .WithRole(PublicationRole.Approver);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                    context.UserPublicationRoles.Add(userPublicationRole);
                });

            var response = await CreateNextVersion(
                dataSetId: Guid.NewGuid(),
                releaseFileId: releaseFile.Id,
                user: identityUser
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task ReleaseVersionApproved_Returns403()
        {
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            var response = await CreateNextVersion(
                dataSetId: Guid.NewGuid(),
                releaseFileId: releaseFile.Id,
                user: OptimisedTestUsers.Bau
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var response = await CreateNextVersion(dataSetId: Guid.Empty, releaseFileId: Guid.Empty);

            var validationProblem = response.AssertValidationProblem();
            Assert.Equal(2, validationProblem.Errors.Count);
            validationProblem.AssertHasNotEmptyError("dataSetId");
            validationProblem.AssertHasNotEmptyError("releaseFileId");
        }

        private async Task<HttpResponseMessage> CreateNextVersion(
            Guid dataSetId,
            Guid releaseFileId,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var uri = new Uri(BaseUrl, UriKind.Relative);

            return await client.PostAsync(
                uri,
                new JsonNetContent(
                    new NextDataSetVersionCreateRequest { DataSetId = dataSetId, ReleaseFileId = releaseFileId }
                )
            );
        }
    }

    public class CompleteNextDataSetVersionImport(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

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
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var processorClientMock = fixture.GetProcessorClientMock();

            processorClientMock
                .Setup(c => c.CompleteNextDataSetVersionImport(nextDataSetVersion.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                    new ProcessDataSetVersionResponseViewModel
                    {
                        DataSetId = dataSet.Id,
                        DataSetVersionId = nextDataSetVersion.Id,
                        InstanceId = Guid.NewGuid(),
                    }
                );

            var response = await CompleteNextVersionImport(dataSetVersionId: nextDataSetVersion.Id);

            MockUtils.VerifyAllMocks(processorClientMock);

            var viewModel = response.AssertOk<DataSetVersionSummaryViewModel>();

            Assert.Equal(viewModel.Id, nextDataSetVersion.Id);
            Assert.Equal(viewModel.Version, nextDataSetVersion.PublicVersion);
            Assert.Equal(viewModel.Status, nextDataSetVersion.Status);
            Assert.Equal(viewModel.Type, nextDataSetVersion.VersionType);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            // The data set version must actually exist - otherwise the request is rejected as
            // Not Found before the authorization check is ever reached.
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture.GetContentDbContext().AddTestData(context => context.ReleaseFiles.Add(releaseFile));

            DataSet dataSet = DataFixture.DefaultDataSet();

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(nextDataSetVersion);
                });

            var response = await CompleteNextVersionImport(
                dataSetVersionId: nextDataSetVersion.Id,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task UserOnPublicationTeam_Returns403(PublicationRole publicationRole)
        {
            // CanManagePublicApiDataSets is currently BAU-only - having a role on the publication does
            // not (yet) satisfy this policy.
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            Publication publication = DataFixture.DefaultPublication();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(publication))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            UserPublicationRole userPublicationRole = DataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRole);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                    context.UserPublicationRoles.Add(userPublicationRole);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithPublicationId(publication.Id);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(nextDataSetVersion);
                });

            var response = await CompleteNextVersionImport(dataSetVersionId: nextDataSetVersion.Id, user: identityUser);

            response.AssertForbidden();
        }

        [Fact]
        public async Task ReleaseVersionApproved_Returns403()
        {
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(nextDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await CompleteNextVersionImport(dataSetVersionId: nextDataSetVersion.Id);

            response.AssertForbidden();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var response = await CompleteNextVersionImport(dataSetVersionId: Guid.Empty);

            var validationProblem = response.AssertValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasNotEmptyError("dataSetVersionId");
        }

        private async Task<HttpResponseMessage> CompleteNextVersionImport(
            Guid dataSetVersionId,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var uri = new Uri($"{BaseUrl}/complete", UriKind.Relative);

            return await client.PostAsync(
                uri,
                new JsonNetContent(new NextDataSetVersionCompleteImportRequest { DataSetVersionId = dataSetVersionId })
            );
        }
    }

    public class DeleteVersionTests(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var dataSetVersion = await SetupDataSetVersionForDeletionData();

            var dataSetVersionId = dataSetVersion.Id;

            var processorClientMock = fixture.GetProcessorClientMock();

            processorClientMock
                .Setup(c => c.DeleteDataSetVersion(dataSetVersionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Either<ActionResult, Unit>(Unit.Instance));

            var response = await DeleteVersion(dataSetVersionId);

            MockUtils.VerifyAllMocks(processorClientMock);

            response.AssertNoContent();
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var dataSetVersion = await SetupDataSetVersionForDeletionData();

            var response = await DeleteVersion(dataSetVersion.Id, user: OptimisedTestUsers.Authenticated);

            response.AssertForbidden();
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task UserOnPublicationTeam_Returns403(PublicationRole publicationRole)
        {
            // CanManagePublicApiDataSets is currently BAU-only - having a role on the publication does
            // not (yet) satisfy this policy.
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            Publication publication = DataFixture.DefaultPublication();

            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(publication))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            UserPublicationRole userPublicationRole = DataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRole);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                    context.UserPublicationRoles.Add(userPublicationRole);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusDraft().WithPublicationId(publication.Id);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .WithImports(() => DataFixture.DefaultDataSetVersionImport().Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            // The DataSet <-> DataSetVersion relationship is circular once LatestDraftVersion is set, so
            // this must remain a two-step Add-then-Update rather than a single combined AddTestData call.
            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await DeleteVersion(dataSetVersion.Id, user: identityUser);

            response.AssertForbidden();
        }

        [Fact]
        public async Task ReleaseVersionApproved_Returns403()
        {
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusDraft();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .WithImports(() => DataFixture.DefaultDataSetVersionImport().Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var response = await DeleteVersion(dataSetVersion.Id);

            response.AssertForbidden();
        }

        [Fact]
        public async Task ProcessorReturns404_Returns404()
        {
            var dataSetVersion = await SetupDataSetVersionForDeletionData();

            var processorClientMock = fixture.GetProcessorClientMock();

            processorClientMock
                .Setup(c => c.DeleteDataSetVersion(dataSetVersion.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Either<ActionResult, Unit>(new NotFoundResult()));

            var response = await DeleteVersion(dataSetVersion.Id);

            MockUtils.VerifyAllMocks(processorClientMock);

            response.AssertNotFound();
        }

        [Fact]
        public async Task ProcessorReturns400_Returns400()
        {
            var dataSetVersion = await SetupDataSetVersionForDeletionData();

            var processorClientMock = fixture.GetProcessorClientMock();

            processorClientMock
                .Setup(c => c.DeleteDataSetVersion(dataSetVersion.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new Either<ActionResult, Unit>(
                        new BadRequestObjectResult(
                            new ValidationProblemViewModel
                            {
                                Errors = new ErrorViewModel[]
                                {
                                    new() { Code = "error code", Path = "error path" },
                                },
                            }
                        )
                    )
                );

            var response = await DeleteVersion(dataSetVersion.Id);

            MockUtils.VerifyAllMocks(processorClientMock);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError("error path", "error code");
        }

        [Fact]
        public async Task ProcessorClientThrows_Returns500()
        {
            var dataSetVersion = await SetupDataSetVersionForDeletionData();

            var processorClientMock = fixture.GetProcessorClientMock();

            processorClientMock
                .Setup(c => c.DeleteDataSetVersion(dataSetVersion.Id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException());

            await Assert.ThrowsAsync<HttpRequestException>(() => DeleteVersion(dataSetVersion.Id));

            MockUtils.VerifyAllMocks(processorClientMock);
        }

        private async Task<HttpResponseMessage> DeleteVersion(Guid dataSetVersionId, ClaimsPrincipal? user = null)
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var uri = new Uri($"{BaseUrl}/{dataSetVersionId}", UriKind.Relative);

            return await client.DeleteAsync(uri);
        }

        private async Task<DataSetVersion> SetupDataSetVersionForDeletionData()
        {
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusDraft();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .WithImports(() => DataFixture.DefaultDataSetVersionImport().Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });
            return dataSetVersion;
        }
    }

    public class UnfinaliseVersionTests(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var dataSetVersion = await SetupPatchVersion();
            var processorClientMock = fixture.GetProcessorClientMock();
            processorClientMock
                .Setup(client => client.UnfinaliseDataSetVersion(dataSetVersion.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Instance);

            var response = await UnfinaliseVersion(dataSetVersion.Id);

            response.AssertNoContent();
            MockUtils.VerifyAllMocks(processorClientMock);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var dataSetVersion = await SetupPatchVersion();

            var response = await UnfinaliseVersion(dataSetVersion.Id, OptimisedTestUsers.Authenticated);

            response.AssertForbidden();
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task UserOnPublicationTeam_Returns403(PublicationRole publicationRole)
        {
            // CanManagePublicApiDataSets is currently BAU-only - having a role on the publication does
            // not (yet) satisfy this policy.
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            Publication publication = DataFixture.DefaultPublication();

            var releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(publication))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .Generate();

            UserPublicationRole userPublicationRole = DataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRole);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                    context.UserPublicationRoles.Add(userPublicationRole);
                });

            var dataSet = DataFixture.DefaultDataSet().WithStatusDraft().WithPublicationId(publication.Id).Generate();
            var dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0, patch: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(dataSetVersion);
                });

            var response = await UnfinaliseVersion(dataSetVersion.Id, identityUser);

            response.AssertForbidden();
        }

        [Fact]
        public async Task ReleaseVersionApproved_Returns403()
        {
            var releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .Generate();
            await fixture.GetContentDbContext().AddTestData(context => context.ReleaseFiles.Add(releaseFile));

            var dataSet = DataFixture.DefaultDataSet().WithStatusDraft().Generate();
            var dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0, patch: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .Generate();
            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await UnfinaliseVersion(dataSetVersion.Id);

            response.AssertForbidden();
        }

        [Fact]
        public async Task ProcessorValidationFailure_IsPropagated()
        {
            var dataSetVersion = await SetupPatchVersion();
            var processorClientMock = fixture.GetProcessorClientMock();
            processorClientMock
                .Setup(client => client.UnfinaliseDataSetVersion(dataSetVersion.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new BadRequestObjectResult(
                        new ValidationProblemViewModel
                        {
                            Errors = [new ErrorViewModel { Code = "cannot_unfinalise", Path = "dataSetVersionId" }],
                        }
                    )
                );

            var response = await UnfinaliseVersion(dataSetVersion.Id);

            response.AssertValidationProblem().AssertHasError("dataSetVersionId", "cannot_unfinalise");
            MockUtils.VerifyAllMocks(processorClientMock);
        }

        private async Task<HttpResponseMessage> UnfinaliseVersion(Guid dataSetVersionId, ClaimsPrincipal? user = null)
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);
            return await client.PostAsync(
                new Uri($"{BaseUrl}/{dataSetVersionId}/unfinalise", UriKind.Relative),
                content: null
            );
        }

        private async Task<DataSetVersion> SetupPatchVersion()
        {
            var releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .Generate();
            await fixture.GetContentDbContext().AddTestData(context => context.ReleaseFiles.Add(releaseFile));

            var dataSet = DataFixture.DefaultDataSet().WithStatusDraft().Generate();
            var dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 0, patch: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .Generate();
            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersions.Add(dataSetVersion));
            return dataSetVersion;
        }
    }

    public class GetVersionChangesTests(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        [Fact]
        public async Task BauUser_Success_Returns200()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusDraft();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var mockedChanges = new MockedChanges { Changes = ["test"] };

            var publicDataApiClientMock = fixture.GetPublicDataApiClientMock();

            publicDataApiClientMock
                .Setup(c =>
                    c.GetDataSetVersionChanges(
                        dataSetVersion.DataSetId,
                        dataSetVersion.PublicVersion,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(mockedChanges) }
                );

            var response = await GetVersionChanges(dataSetVersion.Id);

            MockUtils.VerifyAllMocks(publicDataApiClientMock);

            response.AssertOk(mockedChanges, useSystemJson: true);
        }

        [Fact]
        public async Task NotBauUserAndNotOnPublicationTeam_Returns403()
        {
            // The data set version must actually exist - otherwise the request is rejected as
            // Not Found before the authorization check is ever reached.
            DataSet dataSet = DataFixture.DefaultDataSet();

            DataSetVersion dataSetVersion = DataFixture.DefaultDataSetVersion().WithDataSet(dataSet).Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(dataSetVersion);
                });

            var response = await GetVersionChanges(dataSetVersion.Id, user: OptimisedTestUsers.Authenticated);

            response.AssertForbidden();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            var response = await GetVersionChanges(Guid.NewGuid());

            response.AssertNotFound();
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task UserOnPublicationTeam_CanGetVersionChanges(PublicationRole publicationRole)
        {
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            Publication publication = DataFixture.DefaultPublication();

            UserPublicationRole userPublicationRole = DataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRole);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.UserPublicationRoles.Add(userPublicationRole);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithPublicationId(publication.Id);

            DataSetVersion dataSetVersion = DataFixture.DefaultDataSetVersion().WithDataSet(dataSet).Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(dataSetVersion);
                });

            var mockedChanges = new MockedChanges { Changes = ["test"] };

            var publicDataApiClientMock = fixture.GetPublicDataApiClientMock();

            // Setting up the mock only for this exact data set version's id/public version confirms the
            // right resource was resolved and passed through to the downstream API call.
            publicDataApiClientMock
                .Setup(c =>
                    c.GetDataSetVersionChanges(
                        dataSetVersion.DataSetId,
                        dataSetVersion.PublicVersion,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(mockedChanges) }
                );

            var response = await GetVersionChanges(dataSetVersion.Id, user: identityUser);

            MockUtils.VerifyAllMocks(publicDataApiClientMock);

            response.AssertOk<MockedChanges>(useSystemJson: true);
        }

        [Fact]
        public async Task UserWithOnlyPreReleaseRole_Returns403()
        {
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            Publication publication = DataFixture.DefaultPublication();

            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion()
                .WithRelease(DataFixture.DefaultRelease().WithPublication(publication));

            UserPreReleaseRole userPreReleaseRole = DataFixture
                .DefaultUserPreReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseVersions.Add(releaseVersion);
                    context.UserPreReleaseRoles.Add(userPreReleaseRole);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithPublicationId(publication.Id);

            DataSetVersion dataSetVersion = DataFixture.DefaultDataSetVersion().WithDataSet(dataSet).Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(dataSetVersion);
                });

            var response = await GetVersionChanges(dataSetVersion.Id, user: identityUser);

            response.AssertForbidden();
        }

        [Fact]
        public async Task PublicDataApiReturns400_Returns400()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusDraft();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var publicDataApiClientMock = fixture.GetPublicDataApiClientMock();

            publicDataApiClientMock
                .Setup(c =>
                    c.GetDataSetVersionChanges(
                        dataSetVersion.DataSetId,
                        dataSetVersion.PublicVersion,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(ValidationUtils.ValidationResult());

            var response = await GetVersionChanges(dataSetVersion.Id);

            MockUtils.VerifyAllMocks(publicDataApiClientMock);

            response.AssertValidationProblem();
        }

        [Fact]
        public async Task PublicDataApiClientThrows_Returns500()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusDraft();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(1, 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var publicDataApiClientMock = fixture.GetPublicDataApiClientMock();

            publicDataApiClientMock
                .Setup(c =>
                    c.GetDataSetVersionChanges(
                        dataSetVersion.DataSetId,
                        dataSetVersion.PublicVersion,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new HttpRequestException());

            await Assert.ThrowsAsync<HttpRequestException>(() => GetVersionChanges(dataSetVersion.Id));

            MockUtils.VerifyAllMocks(publicDataApiClientMock);
        }

        private async Task<HttpResponseMessage> GetVersionChanges(Guid dataSetVersionId, ClaimsPrincipal? user = null)
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var uri = new Uri($"{BaseUrl}/{dataSetVersionId}/changes", UriKind.Relative);

            return await client.GetAsync(uri);
        }

        private record MockedChanges
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local - the Test response is accessed in a Deep Assert, which the compiler can not determine.
            public List<string> Changes { get; init; } = [];
        }
    }

    public class UpdateVersionTests(DataSetVersionsControllerTestsFixture fixture)
        : DataSetVersionsControllerTests(fixture)
    {
        [Theory]
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.UpdateableStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task Success(DataSetVersionStatus dataSetVersionStatus)
        {
            ReleaseFile releaseFile = DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(
                    DataFixture
                        .DefaultReleaseVersion()
                        .WithRelease(DataFixture.DefaultRelease().WithPublication(DataFixture.DefaultPublication()))
                )
                .WithFile(DataFixture.DefaultFile(FileType.Data));

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

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
                .WithRelease(DataFixture.DefaultDataSetVersionRelease().WithReleaseFileId(releaseFile.Id))
                .WithNotes("initial notes.")
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
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
                viewModel.GeographicLevels
            );
            Assert.Equal(
                TimePeriodRangeViewModel.Create(nextDataSetVersion.MetaSummary!.TimePeriodRange),
                viewModel.TimePeriods
            );
            Assert.Equal(nextDataSetVersion.MetaSummary!.Filters, viewModel.Filters);
            Assert.Equal(nextDataSetVersion.MetaSummary!.Indicators, viewModel.Indicators);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            // The data set version must actually exist - otherwise the request is rejected as
            // Not Found before the authorization check is ever reached.
            DataSet dataSet = DataFixture.DefaultDataSet();
            DataSetVersion dataSetVersion = DataFixture.DefaultDataSetVersion().WithDataSet(dataSet).Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(dataSetVersion);
                });

            var updateRequest = new DataSetVersionUpdateRequest();

            var response = await UpdateVersion(
                dataSetVersion.Id,
                updateRequest,
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        [Theory]
        [InlineData(PublicationRole.Approver)]
        [InlineData(PublicationRole.Drafter)]
        public async Task UserOnPublicationTeam_Returns403(PublicationRole publicationRole)
        {
            // CanManagePublicApiDataSets is currently BAU-only - having a role on the publication does
            // not (yet) satisfy this policy.
            ClaimsPrincipal identityUser = DataFixture.StandardUser();
            User user = DataFixture.DefaultUser().WithId(identityUser.GetUserId());

            Publication publication = DataFixture.DefaultPublication();

            UserPublicationRole userPublicationRole = DataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(publicationRole);

            await fixture
                .GetContentDbContext()
                .AddTestData(context =>
                {
                    context.Publications.Add(publication);
                    context.UserPublicationRoles.Add(userPublicationRole);
                });

            DataSet dataSet = DataFixture.DefaultDataSet().WithPublicationId(publication.Id);
            DataSetVersion dataSetVersion = DataFixture.DefaultDataSetVersion().WithDataSet(dataSet).Generate();

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSets.Add(dataSet);
                    context.DataSetVersions.Add(dataSetVersion);
                });

            var updateRequest = new DataSetVersionUpdateRequest();

            var response = await UpdateVersion(dataSetVersion.Id, updateRequest, user: identityUser);

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
        [MemberData(
            nameof(DataSetVersionStatusTheoryData.ReadOnlyStatuses),
            MemberType = typeof(DataSetVersionStatusTheoryData)
        )]
        public async Task DataSetVersionCannotBeUpdated_Returns400(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

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

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var updateRequest = new DataSetVersionUpdateRequest();

            var response = await UpdateVersion(nextDataSetVersion.Id, updateRequest);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "dataSetVersionId",
                expectedCode: ValidationMessages.DataSetVersionCannotBeUpdated.Code
            );
        }

        [Fact]
        public async Task DataSetVersionIsFirstVersion_UpdatingNotes_Returns400()
        {
            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusDraft();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

            var updateRequest = new DataSetVersionUpdateRequest { Notes = "updated notes." };

            var response = await UpdateVersion(dataSetVersion.Id, updateRequest);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "notes",
                expectedCode: ValidationMessages.DataSetVersionCannotHaveNotes.Code
            );
        }

        private async Task<HttpResponseMessage> UpdateVersion(
            Guid dataSetVersionId,
            DataSetVersionUpdateRequest updateRequest,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            return await client.PatchAsJsonAsync($"{BaseUrl}/{dataSetVersionId}", updateRequest);
        }
    }
}
