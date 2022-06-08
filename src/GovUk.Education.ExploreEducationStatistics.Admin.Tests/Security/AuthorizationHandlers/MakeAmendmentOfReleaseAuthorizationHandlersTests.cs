#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MakeAmendmentOfSpecificReleaseAuthorizationHandlerTests
    {
        public class ClaimTests
        {
            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionDraft()
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = new Publication(),
                    ApprovalStatus = Draft
                };

                // Assert that no users can amend a draft Release that is the only version
                await AssertReleaseHandlerSucceedsWithCorrectClaims<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();
                        return CreateHandler(contentDbContext);
                    },
                    release);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionPublished()
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = new Publication(),
                    ApprovalStatus = Approved,
                    Published = DateTime.UtcNow
                };

                // Assert that users with the "MakeAmendmentOfAllReleases" claim can amend a published Release that is the only version
                await AssertReleaseHandlerSucceedsWithCorrectClaims<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    release,
                    MakeAmendmentsOfAllReleases);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_DraftVersion()
            {
                var publication = new Publication();

                var previousVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    Published = DateTime.UtcNow
                };

                var latestVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    PreviousVersionId = previousVersion.Id,
                    ApprovalStatus = Draft
                };

                // Assert that no users can amend an amendment Release if it is not yet approved
                await AssertReleaseHandlerSucceedsWithCorrectClaims<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(publication);
                        contentDbContext.AddRange(previousVersion, latestVersion);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    latestVersion);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_NotLatestVersion()
            {
                var publication = new Publication();

                var previousVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    Published = DateTime.UtcNow
                };

                var latestVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    PreviousVersionId = previousVersion.Id,
                    Published = DateTime.UtcNow
                };

                // Assert that no users can amend an amendment Release if it is not the latest version
                await AssertReleaseHandlerSucceedsWithCorrectClaims<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(publication);
                        contentDbContext.AddRange(previousVersion, latestVersion);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    previousVersion);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_LatestVersion()
            {
                var publication = new Publication();

                var previousVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    Published = DateTime.UtcNow
                };

                var latestVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    PreviousVersionId = previousVersion.Id,
                    Published = DateTime.UtcNow
                };

                // Assert that users with the "MakeAmendmentOfAllReleases" claim can amend a published Release that is the latest version
                await AssertReleaseHandlerSucceedsWithCorrectClaims<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(publication);
                        contentDbContext.AddRange(previousVersion, latestVersion);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    latestVersion,
                    MakeAmendmentsOfAllReleases);
            }
        }

        public class PublicationRoleTests
        {
            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionDraft()
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = Draft
                };

                // Assert that no User Publication roles will allow a draft Release that is the only version to be amended
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    release);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionPublished()
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = Approved,
                    Published = DateTime.UtcNow
                };

                // Assert that a User who has the Publication Owner role on a Release can amend it if it is the only version published
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    release,
                    Owner);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_DraftVersion()
            {
                var publication = new Publication
                {
                    Id = Guid.NewGuid()
                };

                var previousVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    Published = DateTime.UtcNow
                };

                var latestVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    PreviousVersionId = previousVersion.Id,
                    ApprovalStatus = Draft
                };

                // Assert that no User Publication roles will allow an amendment Release that is not yet approved to be amended
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(publication);
                        contentDbContext.AddRange(previousVersion, latestVersion);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    latestVersion);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_NotLatestVersion()
            {
                var publication = new Publication
                {
                    Id = Guid.NewGuid()
                };

                var previousVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    Published = DateTime.UtcNow
                };

                var latestVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    PreviousVersionId = previousVersion.Id,
                    Published = DateTime.UtcNow
                };

                // Assert that no User Publication roles will allow an amendment Release that is not the latest version to be amended
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.AddAsync(publication);
                        contentDbContext.AddRange(previousVersion, latestVersion);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    previousVersion);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_LatestVersion()
            {
                var publication = new Publication
                {
                    Id = Guid.NewGuid()
                };

                var previousVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    Published = DateTime.UtcNow
                };

                var latestVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    PreviousVersionId = previousVersion.Id,
                    Published = DateTime.UtcNow
                };

                // Assert that a User who has the Publication Owner role on a Release can amend it if it is the latest published version
                await AssertReleaseHandlerSucceedsWithCorrectPublicationRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(publication);
                        contentDbContext.AddRange(previousVersion, latestVersion);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    latestVersion,
                    Owner);
            }
        }

        public class ReleaseRoleTests
        {
            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionDraft()
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = Draft
                };

                // Assert that no User Release roles will allow a draft Release that is the only version to be amended
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    release);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_OnlyVersionPublished()
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    },
                    ApprovalStatus = Approved,
                    Published = DateTime.UtcNow
                };

                // Assert that no User Release roles will allow a published Release that is the only version to be amended
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(release);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    release);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_DraftVersion()
            {
                var publication = new Publication
                {
                    Id = Guid.NewGuid()
                };

                var previousVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    Published = DateTime.UtcNow
                };

                var latestVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    PreviousVersionId = previousVersion.Id,
                    ApprovalStatus = Draft
                };

                // Assert that no User Release roles will allow an amendment Release that is not yet approved to be amended
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(publication);
                        contentDbContext.AddRange(previousVersion, latestVersion);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    latestVersion);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_NotLatestVersion()
            {
                var publication = new Publication
                {
                    Id = Guid.NewGuid()
                };

                var previousVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    Published = DateTime.UtcNow
                };

                var latestVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    PreviousVersionId = previousVersion.Id,
                    Published = DateTime.UtcNow
                };

                // Assert that no User Release roles will allow an amendment Release that is not the latest version to be amended
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.AddAsync(publication);
                        contentDbContext.AddRange(previousVersion, latestVersion);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    previousVersion);
            }

            [Fact]
            public async Task MakeAmendmentOfSpecificReleaseAuthorizationHandler_LatestVersion()
            {
                var publication = new Publication
                {
                    Id = Guid.NewGuid()
                };

                var previousVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    Published = DateTime.UtcNow
                };

                var latestVersion = new Release
                {
                    Id = Guid.NewGuid(),
                    Publication = publication,
                    PreviousVersionId = previousVersion.Id,
                    Published = DateTime.UtcNow
                };

                // Assert that no User Release roles will allow an amendment Release that is the latest version to be amended
                await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                    contentDbContext =>
                    {
                        contentDbContext.Add(publication);
                        contentDbContext.AddRange(previousVersion, latestVersion);
                        contentDbContext.SaveChanges();

                        return CreateHandler(contentDbContext);
                    },
                    latestVersion);
            }
        }

        private static MakeAmendmentOfSpecificReleaseAuthorizationHandler CreateHandler(ContentDbContext contentDbContext)
        {
            return new MakeAmendmentOfSpecificReleaseAuthorizationHandler(contentDbContext,
                new AuthorizationHandlerResourceRoleService(
                    Mock.Of<IUserReleaseRoleRepository>(Strict),
                    new UserPublicationRoleRepository(contentDbContext),
                    Mock.Of<IPublicationRepository>(Strict)));
        }
    }
}
