#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Moq;
using System;
using System.Threading.Tasks;
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

    private readonly ReleaseVersion _releaseVersion = new()
    {
        Id = Guid.NewGuid()
    };

    [Fact]
    public async Task GetDataBlockTableResult()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion)
            .AssertForbidden(
                userService =>
                {
                    var service = BuildService(
                        userService: userService.Object);
                    return service.GetDataBlockTableResult(_releaseVersion.Id, _dataBlock.Id);
                });
    }

    private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
    {
        var mock = MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseVersion>();
        MockUtils.SetupCall(mock, _releaseVersion.Id, _releaseVersion);
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
