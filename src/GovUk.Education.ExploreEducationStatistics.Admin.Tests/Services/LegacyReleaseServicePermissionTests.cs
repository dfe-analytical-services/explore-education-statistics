#nullable enable
using System;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class LegacyReleaseServicePermissionTests
{
    private readonly LegacyRelease _legacyRelease = new()
    {
        Id = Guid.NewGuid()
    };

    private readonly Publication _publication = new()
    {
        Id = Guid.NewGuid()
    };

    [Fact]
    public async Task GetLegacyRelease()
    {
        await PolicyCheckBuilder()
            .SetupResourceCheckToFail(_legacyRelease, CanViewLegacyRelease)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.GetLegacyRelease(_legacyRelease.Id);
                }
            );
    }

    [Fact]
    public async Task ListLegacyReleases()
    {
        await PolicyCheckBuilder()
            .SetupResourceCheckToFail(_publication, CanViewSpecificPublication)
            .AssertForbidden(
                userService =>
                {
                    var service = SetupService(userService: userService.Object);
                    return service.ListLegacyReleases(_publication.Id);
                }
            );
    }

    private LegacyReleaseService SetupService(
        ContentDbContext? contentDbContext = null,
        PersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IPublicationCacheService? publicationCacheService = null,
        IUserService? userService = null,
        IMapper? mapper = null)
    {
        return new LegacyReleaseService(
            contentDbContext ?? new Mock<ContentDbContext>().Object,
            mapper ?? AdminMapper(),
            userService ?? Mock.Of<IUserService>(Strict),
            persistenceHelper ?? DefaultPersistenceHelperMock().Object,
            publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict)
        );
    }

    private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
    {
        var mock = MockPersistenceHelper<ContentDbContext, Release>();
        SetupCall(mock, _legacyRelease.Id, _legacyRelease);
        SetupCall(mock, _publication.Id, _publication);

        return mock;
    }
}
