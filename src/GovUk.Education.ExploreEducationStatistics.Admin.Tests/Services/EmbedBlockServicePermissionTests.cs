using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class EmbedBlockServicePermissionTests
{
    private readonly Release _release = new()
    {
        Id = Guid.NewGuid()
    };

    [Fact]
    public async Task Create()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupEmbedBlockService(userService: userService.Object);
                    return service.Create(releaseId: _release.Id,
                        request: new EmbedBlockCreateRequest
                        {
                            Title = "Test title",
                            Url = "http://www.test.com",
                            ContentSectionId = Guid.NewGuid(),
                        });
                }
            );
    }

    [Fact]
    public async Task Update()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupEmbedBlockService(userService: userService.Object);
                    return service.Update(releaseId: _release.Id,
                        contentBlockId: Guid.NewGuid(),
                        request: new EmbedBlockUpdateRequest
                        {
                            Title = "Test title",
                            Url = "http://www.test.com",
                        });
                }
            );
    }

    [Fact]
    public async Task Delete()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupEmbedBlockService(userService: userService.Object);
                    return service.Delete(releaseId: _release.Id, Guid.NewGuid());
                }
            );
    }

    private EmbedBlockService SetupEmbedBlockService(
        ContentDbContext contentDbContext = null,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
        IContentBlockService contentBlockService = null,
        IUserService userService = null)
    {
        return new EmbedBlockService(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
            contentBlockService ?? Mock.Of<IContentBlockService>(),
            userService ?? Mock.Of<IUserService>(),
            AdminMapper());
    }

    private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
    {
        return MockUtils.MockPersistenceHelper<ContentDbContext, Release>(_release.Id, _release);
    }
}
