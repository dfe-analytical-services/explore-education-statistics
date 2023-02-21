#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
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

public class KeyStatisticServicePermissionTests
{
    private readonly Release _release = new();

    [Fact]
    public async Task CreateKeyStatisticDataBlock()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupKeyStatisticService(userService: userService.Object);
                    return service.CreateKeyStatisticDataBlock(
                        _release.Id,
                        new KeyStatisticDataBlockCreateRequest());
                }
            );
    }

    [Fact]
    public async Task CreateKeyStatisticText()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupKeyStatisticService(userService: userService.Object);
                    return service.CreateKeyStatisticText(
                        _release.Id,
                        new KeyStatisticTextCreateRequest());
                }
            );
    }

    [Fact]
    public async Task UpdateKeyStatisticDataBlock()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupKeyStatisticService(userService: userService.Object);
                    return service.CreateKeyStatisticDataBlock(
                        _release.Id,
                        new KeyStatisticDataBlockCreateRequest());
                }
            );
    }

    [Fact]
    public async Task UpdateKeyStatisticText()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupKeyStatisticService(userService: userService.Object);
                    return service.CreateKeyStatisticText(
                        _release.Id,
                        new KeyStatisticTextCreateRequest());
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
                    var service = SetupKeyStatisticService(userService: userService.Object);
                    return service.Delete(
                        _release.Id,
                        Guid.NewGuid());
                }
            );
    }

    [Fact]
    public async Task Reorder()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupKeyStatisticService(userService: userService.Object);
                    return service.Reorder(
                        _release.Id,
                        new List<Guid>());
                }
            );
    }

    private KeyStatisticService SetupKeyStatisticService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IDataBlockService? dataBlockService = null,
        IUserService? userService = null)
    {
        return new KeyStatisticService(
            contentDbContext ?? new Mock<ContentDbContext>().Object,
            persistenceHelper ?? DefaultPersistenceHelperMock().Object,
            dataBlockService ?? Mock.Of<IDataBlockService>(),
            userService ?? Mock.Of<IUserService>(),
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
