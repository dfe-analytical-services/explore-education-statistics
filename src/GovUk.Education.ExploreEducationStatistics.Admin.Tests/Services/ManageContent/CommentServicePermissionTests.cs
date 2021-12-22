#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent
{
    public class CommentServicePermissionTests
    {
        private static readonly Guid ContentSectionId = Guid.NewGuid();
        private static readonly Guid ContentBlockId = Guid.NewGuid();

        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid(),
            Content = new List<ReleaseContentSection>
            {
                new ReleaseContentSection
                {
                    ContentSection = new ContentSection
                    {
                        Id = ContentSectionId,
                        Content = new List<ContentBlock>
                        {
                            new DataBlock
                            {
                                Id = ContentBlockId
                            }
                        }
                    }
                }
            }
        };

        private readonly Comment _comment = new Comment
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task AddComment()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupCommentService(userService: userService.Object);
                        return service.AddComment(
                            _release.Id,
                            ContentSectionId,
                            ContentBlockId,
                            new CommentSaveRequest());
                    }
                );
        }

        [Fact]
        public async Task DeleteComment()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_comment, CanDeleteSpecificComment)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupCommentService(userService: userService.Object);
                        return service.DeleteComment(
                            _comment.Id);
                    }
                );
        }

        [Fact]
        public async Task SetResolved()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_comment, CanResolveSpecificComment)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupCommentService(userService: userService.Object);
                        return service.SetResolved(
                            _comment.Id,
                            true);
                    }
                );
        }

        [Fact]
        public async Task UpdateComment()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_comment, CanUpdateSpecificComment)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupCommentService(userService: userService.Object);
                        return service.UpdateComment(
                            _comment.Id,
                            new CommentSaveRequest());
                    }
                );
        }
        private CommentService SetupCommentService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IUserService? userService = null)
        {
            return new CommentService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                userService ?? new Mock<IUserService>().Object,
                AdminMapper()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(mock, _release.Id, _release);
            MockUtils.SetupCall(mock, _comment.Id, _comment);
            return mock;
        }
    }
}
