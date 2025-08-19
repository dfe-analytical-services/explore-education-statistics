#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    AuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseVersionAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static Moq.MockBehavior;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class MakeAmendmentOfSpecificReleaseAuthorizationHandlerTests
{
    private readonly DataFixture _dataFixture = new();

    public class ClaimTests : MakeAmendmentOfSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionUnpublished()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: null, Version: 0 });

            // Assert that no users can amend an unpublished Release that is the only version
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionPublished()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: not null, Version: 0 });

            // Assert that users with the "MakeAmendmentOfAllReleases" claim can amend a published Release that is the only version
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion,
                MakeAmendmentsOfAllReleases);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_UnpublishedVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1, draftVersion: true)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: null, Version: 1 });

            // Assert that no users can amend a version that is not published
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_NotLatestVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 2)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv.Version == 0);

            // Assert that no users can amend an amendment Release if it is not the latest version
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_LatestVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 2)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv.Version == 1);

            // Assert that users with the "MakeAmendmentOfAllReleases" claim can amend a published Release that is the latest version
            await AssertHandlerSucceedsWithCorrectClaims<ReleaseVersion, MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion,
                MakeAmendmentsOfAllReleases);
        }
    }

    public class PublicationRoleTests : MakeAmendmentOfSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionUnpublished()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: null, Version: 0 });

            // Assert that no User Publication roles will allow an unpublished Release that is the only version to be amended
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionPublished()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: not null, Version: 0 });

            // Assert that a User who has the Publication Owner role on a Release can amend it if it is the only version published
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion,
                Owner);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_UnpublishedVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1, draftVersion: true)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: null, Version: 1 });

            // Assert that no User Publication roles will allow an amendment Release that is not yet approved to be amended
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_NotLatestVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 2)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv.Version == 0);

            // Assert that no User Publication roles will allow an amendment Release that is not the latest version to be amended
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_LatestVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 2)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv.Version == 1);

            // Assert that a User who has the Publication Owner role on a Release can amend it if it is the latest published version
            await AssertReleaseVersionHandlerSucceedsWithCorrectPublicationRoles<
                MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion,
                Owner);
        }
    }

    public class ReleaseRoleTests : MakeAmendmentOfSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionUnpublished()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: null, Version: 0 });

            // Assert that no User Release roles will allow an unpublished Release that is the only version to be amended
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionPublished()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: not null, Version: 0 });

            // Assert that no User Release roles will allow a published Release that is the only version to be amended
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_UnpublishedVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 1, draftVersion: true)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv is { Published: null, Version: 1 });

            // Assert that no User Release roles will allow an amendment Release that is not yet approved to be amended
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_NotLatestVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 2)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv.Version == 0);

            // Assert that no User Release roles will allow an amendment Release that is not the latest version to be amended
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }

        [Fact]
        public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_LatestVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 2)
                    .Generate(1));

            var releaseVersion = publication.ReleaseVersions.Single(rv => rv.Version == 1);

            // Assert that no User Release roles will allow an amendment Release that is the latest version to be amended
            await AssertReleaseVersionHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Publications.Add(publication);
                    contentDbContext.SaveChanges();

                    return CreateHandler(contentDbContext);
                },
                releaseVersion);
        }
    }

    private static MakeAmendmentOfSpecificReleaseAuthorizationHandler CreateHandler(
        ContentDbContext contentDbContext)
    {
        var userReleaseRoleRepository = new UserReleaseRoleRepository(
            contentDbContext,
            logger: Mock.Of<ILogger<UserReleaseRoleRepository>>());

        var userPublicationRoleRepository = new UserPublicationRoleRepository(
            contentDbContext);

        return new MakeAmendmentOfSpecificReleaseAuthorizationHandler(
            new AuthorizationHandlerService(
                releaseVersionRepository: new ReleaseVersionRepository(contentDbContext),
                userReleaseRoleRepository: userReleaseRoleRepository,
                userPublicationRoleRepository: userPublicationRoleRepository,
                preReleaseService: Mock.Of<IPreReleaseService>(Strict)),
            new ReleaseVersionRepository(contentDbContext));
    }
}
