using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Moq;
using Xunit;
using PublisherReleaseStatus = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatus;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateSpecificReleaseAuthorizationHandlerTests
    {
        [Fact]
        public void ClaimSuccess_UpdateAllReleases_ReleasePublishingNotStarted()
        {
            // Assert that only users with the "UpdateAllReleases" claim can
            // update an arbitrary Release if it has not started publishing
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                release.Id,
                                ReleaseStatusOverallStage.Started,
                                ReleaseStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(new List<PublisherReleaseStatus>());

                    AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateSpecificReleaseRequirement>(
                        context => new UpdateSpecificReleaseAuthorizationHandler(
                            context,
                            releaseStatusRepository.Object
                        ),
                        release,
                        UpdateAllReleases
                    );
                }
            );
        }

        [Fact]
        public void AllClaimsFail_ReleasePublishing()
        {
            // Assert that no users can update an arbitrary Release
            // if it has started publishing
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status,
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                release.Id,
                                ReleaseStatusOverallStage.Started,
                                ReleaseStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(
                            new List<PublisherReleaseStatus>
                            {
                                new PublisherReleaseStatus()
                            }
                        );

                    AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateSpecificReleaseRequirement>(
                        context => new UpdateSpecificReleaseAuthorizationHandler(
                            context,
                            releaseStatusRepository.Object
                        ),
                        release
                    );
                }
            );
        }

        [Fact]
        public void AllClaimsFail_ReleasePublished()
        {
            // Assert that no users can update an arbitrary
            // Release if it has been published
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status,
                        Published = DateTime.Now
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                release.Id,
                                ReleaseStatusOverallStage.Started,
                                ReleaseStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(new List<PublisherReleaseStatus>());

                    AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateSpecificReleaseRequirement>(
                        context => new UpdateSpecificReleaseAuthorizationHandler(
                            context,
                            releaseStatusRepository.Object
                        ),
                        release
                    );
                }
            );
        }

        [Fact]
        public void RoleSuccess_EditorOrApprover_ReleasePublishingNotStarted()
        {
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                release.Id,
                                ReleaseStatusOverallStage.Started,
                                ReleaseStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(new List<PublisherReleaseStatus>());

                    // Assert that a User who has the "Contributor", "Lead" or "Approver" role on a
                    // Release can update it if it is not Approved
                    if (status != ReleaseStatus.Approved)
                    {
                        AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<UpdateSpecificReleaseRequirement>(
                            context => new UpdateSpecificReleaseAuthorizationHandler(
                                context,
                                releaseStatusRepository.Object
                            ),
                            release,
                            ReleaseRole.Contributor,
                            ReleaseRole.Lead,
                            ReleaseRole.Approver
                        );
                    }
                    else
                    {
                        // Assert that a User who has the "Approver" role on a
                        // Release can update it if it is Approved
                        AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<UpdateSpecificReleaseRequirement>(
                            context => new UpdateSpecificReleaseAuthorizationHandler(
                                context,
                                releaseStatusRepository.Object
                            ),
                            release,
                            ReleaseRole.Approver
                        );
                    }
                }
            );
        }

        [Fact]
        public void AllRolesFail_ReleasePublishing()
        {
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                release.Id,
                                ReleaseStatusOverallStage.Started,
                                ReleaseStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(
                            new List<PublisherReleaseStatus>
                            {
                                new PublisherReleaseStatus()
                            }
                        );

                    // Assert that no user can update a Release once it has started publishing
                    AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<UpdateSpecificReleaseRequirement>(
                        context => new UpdateSpecificReleaseAuthorizationHandler(
                            context,
                            releaseStatusRepository.Object
                        ),
                        release
                    );
                }
            );
        }

        [Fact]
        public void AllRolesFail_ReleasePublished()
        {
            GetEnumValues<ReleaseStatus>().ForEach(
                status =>
                {
                    var release = new Release
                    {
                        Id = Guid.NewGuid(),
                        Status = status,
                        Published = DateTime.Now,
                    };

                    var releaseStatusRepository = new Mock<IReleaseStatusRepository>();

                    releaseStatusRepository.Setup(
                            s => s.GetAllByOverallStage(
                                release.Id,
                                ReleaseStatusOverallStage.Started,
                                ReleaseStatusOverallStage.Complete
                            )
                        )
                        .ReturnsAsync(
                            new List<PublisherReleaseStatus>()
                        );

                    // Assert that no user can update a Release once it has been published
                    AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<UpdateSpecificReleaseRequirement>(
                        context => new UpdateSpecificReleaseAuthorizationHandler(
                            context,
                            releaseStatusRepository.Object
                        ),
                        release
                    );
                }
            );
        }
    }
}