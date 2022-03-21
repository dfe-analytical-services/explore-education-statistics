using System;
using System.Threading.Tasks;
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
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleasePublishingStatusServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task GetReleaseStatusesAsync()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheck(_release, ContentSecurityPolicies.CanViewSpecificRelease, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = BuildReleaseStatusService(userService: userService.Object);

                        return await service.GetReleaseStatusAsync(_release.Id);
                    }
                );
        }

        private ReleasePublishingStatusService BuildReleaseStatusService(
            IMapper mapper = null,
            IUserService userService = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            ITableStorageService publisherTableStorageService = null)
        {
            return new ReleasePublishingStatusService(
                mapper ?? MapperUtils.AdminMapper(),
                userService ?? new Mock<IUserService>().Object,
                persistenceHelper ?? MockUtils.MockPersistenceHelper<ContentDbContext, Release>(_release.Id, _release)
                    .Object,
                publisherTableStorageService ?? new Mock<ITableStorageService>().Object
            );
        }
    }
}