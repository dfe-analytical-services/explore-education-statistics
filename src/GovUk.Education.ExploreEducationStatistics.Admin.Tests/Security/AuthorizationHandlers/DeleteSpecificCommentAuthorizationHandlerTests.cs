using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class DeleteSpecificCommentAuthorizationHandlerTests
    {
        [Fact]
        public async Task CanDeleteCommentAuthorizationHandler()
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
            };
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ApprovalStatus = ReleaseApprovalStatus.Draft,
                Content = new List<ReleaseContentSection>
                {
                    new ReleaseContentSection
                    {
                        ContentSection = new ContentSection
                        {
                            Content = new List<ContentBlock>
                            {
                                new DataBlock
                                {
                                    Comments = new List<Comment> { comment },
                                }
                            }
                        }
                    }
                }
            };

            await AssertHandlerOnlySucceedsWithReleaseRoles<DeleteSpecificCommentRequirement, Comment>(
                release.Id,
                comment,
                contentDbContext => contentDbContext.Add(release),
                contentDbContext => new DeleteSpecificCommentAuthorizationHandler(
                    contentDbContext,
                    new ReleasePublishingStatusRepository(Mock.Of<ITableStorageService>()),
                    new AuthorizationHandlerResourceRoleService(
                        new UserReleaseRoleRepository(contentDbContext),
                        new UserPublicationRoleRepository(contentDbContext),
                        Mock.Of<IPublicationRepository>(Strict))),
                ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.Lead);
        }
    }
}
