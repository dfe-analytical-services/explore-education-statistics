using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
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
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Security.ContentSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent
{
    public class ContentServicePermissionTests
    {
        private static readonly Guid ContentSectionId = Guid.NewGuid();
        private static readonly Guid ContentBlockId = Guid.NewGuid();

        private readonly Release _release = new()
        {
            Id = Guid.NewGuid(),
            Content = ListOf(
                new ContentSection
                {
                    Id = ContentSectionId,
                    Content = new List<ContentBlock>
                    {
                        new DataBlock
                        {
                            Id = ContentBlockId
                        }
                    }
                })
        };

        [Fact]
        public async Task AddContentBlock()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupContentService(userService: userService.Object);
                        return service.AddContentBlock(
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
                        return service.RemoveContentBlock(
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
                        return service.RemoveContentSection(
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
                        return service.ReorderContentBlocks(
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
                        return service.ReorderContentSections(
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
                        return service.UpdateContentSectionHeading(
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
                        return service.UpdateTextBasedContentBlock(
                            _release.Id,
                            ContentSectionId,
                            ContentBlockId,
                            new ContentBlockUpdateRequest());
                    }
                );
        }

        private ContentService SetupContentService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IContentSectionRepository contentSectionRepository = null,
            IContentBlockService contentBlockService = null,
            IHubContext<ReleaseContentHub, IReleaseContentHubClient> hubContext = null,
            IUserService userService = null)
        {
            var dbContext = contentDbContext ?? new Mock<ContentDbContext>().Object;

            return new ContentService(
                dbContext,
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                contentSectionRepository ?? new ContentSectionRepository(dbContext),
                contentBlockService ?? Mock.Of<IContentBlockService>(),
                hubContext ?? Mock.Of<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(),
                userService ?? new Mock<IUserService>().Object,
                AdminMapper()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(mock, _release.Id, _release);
            return mock;
        }
    }
}