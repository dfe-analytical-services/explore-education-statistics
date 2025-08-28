using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleasePublishingStatusServicePermissionTests
{
    private readonly ReleaseVersion _releaseVersion = new()
    {
        Id = Guid.NewGuid()
    };

    [Fact]
    public async Task GetReleaseStatusesAsync()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheck(_releaseVersion, ContentSecurityPolicies.CanViewSpecificReleaseVersion, false)
            .AssertForbidden(
                async userService =>
                {
                    var service = BuildReleaseStatusService(userService: userService.Object);

                    return await service.GetReleaseStatusAsync(_releaseVersion.Id);
                }
            );
    }

    private ReleasePublishingStatusService BuildReleaseStatusService(
        IMapper mapper = null,
        IUserService userService = null,
        IPersistenceHelper<ContentDbContext> persistenceHelper = null,
        IPublisherTableStorageService publisherTableStorageService = null)
    {
        return new ReleasePublishingStatusService(
            mapper ?? MapperUtils.AdminMapper(),
            userService ?? new Mock<IUserService>().Object,
            persistenceHelper ?? MockUtils
                .MockPersistenceHelper<ContentDbContext, ReleaseVersion>(_releaseVersion.Id, _releaseVersion)
                .Object,
            publisherTableStorageService ?? new Mock<IPublisherTableStorageService>().Object
        );
    }
}
