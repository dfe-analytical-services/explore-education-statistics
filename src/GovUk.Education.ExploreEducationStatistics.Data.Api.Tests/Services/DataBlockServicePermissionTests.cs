#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services;

public class DataBlockServicePermissionTests
{
    private readonly DataBlock _dataBlock = new()
    {
        Id = Guid.NewGuid()
    };

    private readonly Release _release = new()
    {
        Id = Guid.NewGuid()
    };

    [Fact]
    public async Task GetDataBlockTableResult()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_release, ContentSecurityPolicies.CanViewSpecificRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = BuildService(
                        userService: userService.Object);
                    return service.GetDataBlockTableResult(_release.Id, _dataBlock.Id);
                });
    }

    private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
    {
        var mock = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
        MockUtils.SetupCall(mock, _release.Id, _release);
        return mock;
    }

    private DataBlockService BuildService(
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        ITableBuilderService? tableBuilderService = null,
        IUserService? userService = null)
    {
        return new DataBlockService(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            persistenceHelper ?? DefaultPersistenceHelperMock().Object,
            tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict),
            userService ?? Mock.Of<IUserService>(Strict)
        );
    }
}
