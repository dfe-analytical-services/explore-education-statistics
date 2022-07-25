#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Security.ContentSecurityPolicies;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class MethodologyImageServicePermissionTests
{
    private static readonly MethodologyVersion MethodologyVersion = new()
    {
        Id = Guid.NewGuid()
    };

    private static readonly File File = new()
    {
        Id = Guid.NewGuid()
    };

    private static readonly MethodologyFile MethodologyFile = new()
    {
        MethodologyVersion = MethodologyVersion,
        File = File
    };

    [Fact]
    public async Task StreamFile()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(MethodologyVersion, CanViewSpecificMethodologyVersion)
            .AssertForbidden(
                userService =>
                {
                    var persistenceHelper = MockPersistenceHelper<ContentDbContext, MethodologyFile>(MethodologyFile);

                    var service = BuildService(
                        userService: userService.Object,
                        persistenceHelper: persistenceHelper.Object
                    );
                    return service.Stream(MethodologyVersion.Id, File.Id);
                }
            );
    }

    private MethodologyImageService BuildService(
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IBlobStorageService? blobStorageService = null,
        IUserService? userService = null)
    {
        return new(
            persistenceHelper ?? DefaultPersistenceHelperMock().Object,
            blobStorageService ?? Mock.Of<IBlobStorageService>(),
            userService ?? Mock.Of<IUserService>()
        );
    }

    private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
    {
        return MockPersistenceHelper<ContentDbContext, MethodologyVersion>(MethodologyVersion.Id, MethodologyVersion);
    }
}
