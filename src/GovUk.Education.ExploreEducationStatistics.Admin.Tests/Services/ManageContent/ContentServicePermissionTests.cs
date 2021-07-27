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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Security.ContentSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent
{
    public class ContentServicePermissionTests
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
                        var service = SetupContentService(userService: userService.Object);
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
                .SetupResourceCheckToFail(_comment, CanUpdateSpecificComment)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.DeleteComment(
                            _comment.Id);
                    }
                );
        }

        [Fact]
        public async Task ResolveComment()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_comment, CanResolveSpecificComment)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.ResolveComment(
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
                        var service = SetupContentService(userService: userService.Object);
                        return service.UpdateComment(
                            _comment.Id,
                            new CommentSaveRequest());
                    }
                );
        }

        [Fact]
        public async Task AddContentBlock()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.AddContentBlockAsync(
                            _release.Id,
                            ContentSectionId,
                            new ContentBlockAddRequest());
                    }
                );
        }

        [Fact]
        public async Task AddContentSection()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.AddContentSectionAsync(
                            _release.Id,
                            new ContentSectionAddRequest());
                    }
                );
        }

        [Fact]
        public async Task AttachDataBlock()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.AttachDataBlock(
                            _release.Id,
                            ContentSectionId,
                            new ContentBlockAttachRequest());
                    }
                );
        }

        [Fact]
        public async Task GetContentBlocks()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.GetContentBlocks<HtmlBlock>(_release.Id);
                    }
                );
        }

        [Fact]
        public async Task RemoveContentBlock()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.RemoveContentBlockAsync(
                            _release.Id,
                            ContentSectionId,
                            ContentBlockId);
                    }
                );
        }

        [Fact]
        public async Task RemoveContentSection()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.RemoveContentSectionAsync(
                            _release.Id,
                            ContentSectionId);
                    }
                );
        }

        [Fact]
        public async Task ReorderContentBlocks()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.ReorderContentBlocksAsync(
                            _release.Id,
                            ContentSectionId,
                            new Dictionary<Guid, int>());
                    }
                );
        }

        [Fact]
        public async Task ReorderContentSections()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.ReorderContentSectionsAsync(
                            _release.Id,
                            new Dictionary<Guid, int>());
                    }
                );
        }

        [Fact]
        public async Task UpdateContentSectionHeading()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.UpdateContentSectionHeadingAsync(
                            _release.Id,
                            ContentSectionId,
                            "");
                    }
                );
        }

        [Fact]
        public async Task UpdateTextBasedContentBlock()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.UpdateTextBasedContentBlockAsync(
                            _release.Id,
                            ContentSectionId,
                            ContentBlockId,
                            new ContentBlockUpdateRequest());
                    }
                );
        }

        [Fact]
        public async Task UpdateDataBlock()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.UpdateDataBlockAsync(
                            _release.Id,
                            ContentSectionId,
                            ContentBlockId,
                            new DataBlockUpdateRequest());
                    }
                );
        }

        private ContentService SetupContentService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IReleaseContentSectionRepository releaseContentSectionRepository = null,
            IUserService userService = null)
        {
            return new ContentService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                releaseContentSectionRepository ?? new ReleaseContentSectionRepository(contentDbContext),
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
