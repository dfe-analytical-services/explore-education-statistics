using System;
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseStatusServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void GetReleaseStatusesAsync()
        {
            PermissionTestUtils.PolicyCheckBuilder<ContentSecurityPolicies>()
                .ExpectResourceCheck(_release, ContentSecurityPolicies.CanViewRelease, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = BuildReleaseStatusService(userService: userService.Object);

                        return await service.GetReleaseStatusAsync(_release.Id);
                    }
                );
        }

        private ReleaseStatusService BuildReleaseStatusService(
            IMapper mapper = null,
            IUserService userService = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            ITableStorageService publisherTableStorageService = null)
        {
            return new ReleaseStatusService(
                mapper ?? MapperUtils.AdminMapper(),
                userService ?? new Mock<IUserService>().Object,
                persistenceHelper ?? MockUtils.MockPersistenceHelper<ContentDbContext, Release>(_release.Id, _release)
                    .Object,
                publisherTableStorageService ?? new Mock<ITableStorageService>().Object
            );
        }
    }
}