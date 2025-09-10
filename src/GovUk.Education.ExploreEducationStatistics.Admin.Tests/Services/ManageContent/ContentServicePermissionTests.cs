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
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Security.ContentSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent;

public class ContentServicePermissionTests
{
    private static readonly Guid ContentSectionId = Guid.NewGuid();
    private static readonly Guid ContentBlockId = Guid.NewGuid();

    private readonly ReleaseVersion _releaseVersion = new()
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
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupContentService(userService: userService.Object);
                    return service.AddContentBlock(
                        _releaseVersion.Id,
                        ContentSectionId,
                        new ContentBlockAddRequest());
                }
            );
    }

    [Fact]
    public async Task AddContentSection()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupContentService(userService: userService.Object);
                    return service.AddContentSectionAsync(
                        _releaseVersion.Id,
                        new ContentSectionAddRequest());
                }
            );
    }

    [Fact]
    public async Task AttachDataBlock()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupContentService(userService: userService.Object);
                    return service.AttachDataBlock(
                        _releaseVersion.Id,
                        ContentSectionId,
                        new DataBlockAttachRequest());
                }
            );
    }

    [Fact]
    public async Task GetContentBlocks()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanViewSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupContentService(userService: userService.Object);
                    return service.GetContentBlocks<HtmlBlock>(_releaseVersion.Id);
                }
            );
    }

    [Fact]
    public async Task RemoveContentBlock()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupContentService(userService: userService.Object);
                    return service.RemoveContentBlock(
                        _releaseVersion.Id,
                        ContentSectionId,
                        ContentBlockId);
                }
            );
    }

    [Fact]
    public async Task RemoveContentSection()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupContentService(userService: userService.Object);
                    return service.RemoveContentSection(
                        _releaseVersion.Id,
                        ContentSectionId);
                }
            );
    }

    [Fact]
    public async Task ReorderContentBlocks()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupContentService(userService: userService.Object);
                    return service.ReorderContentBlocks(
                        _releaseVersion.Id,
                        ContentSectionId,
                        new Dictionary<Guid, int>());
                }
            );
    }

    [Fact]
    public async Task ReorderContentSections()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupContentService(userService: userService.Object);
                    return service.ReorderContentSections(
                        _releaseVersion.Id,
                        new Dictionary<Guid, int>());
                }
            );
    }

    [Fact]
    public async Task UpdateContentSectionHeading()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupContentService(userService: userService.Object);
                    return service.UpdateContentSectionHeading(
                        _releaseVersion.Id,
                        ContentSectionId,
                        "");
                }
            );
    }

    [Fact]
    public async Task UpdateTextBasedContentBlock()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupContentService(userService: userService.Object);
                    return service.UpdateTextBasedContentBlock(
                        _releaseVersion.Id,
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
        var mock = MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseVersion>();
        MockUtils.SetupCall(mock, _releaseVersion.Id, _releaseVersion);
        return mock;
    }
}
