#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataGuidanceServicePermissionTests
    {
        private readonly Release _release = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task GetDataGuidance()
        {
            await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
            await contentDbContext.Releases.AddRangeAsync(_release);
            await contentDbContext.SaveChangesAsync();

            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupService(contentDbContext: contentDbContext,
                            userService: userService.Object);
                        return service.GetDataGuidance(_release.Id);
                    }
                );
        }

        [Fact]
        public async Task UpdateDataGuidance()
        {
            await using var contentDbContext = DbUtils.InMemoryApplicationDbContext();
            await contentDbContext.Releases.AddRangeAsync(_release);
            await contentDbContext.SaveChangesAsync();

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, SecurityPolicies.CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupService(contentDbContext: contentDbContext,
                            userService: userService.Object);
                        return service.UpdateDataGuidance(_release.Id, new DataGuidanceUpdateRequest());
                    }
                );
        }

        private static DataGuidanceService SetupService(
            ContentDbContext? contentDbContext = null,
            StatisticsDbContext? statisticsDbContext = null,
            IDataGuidanceDataSetService? dataGuidanceDataSetService = null,
            IUserService? userService = null,
            IReleaseDataFileRepository? releaseDataFileRepository = null)
        {
            return new DataGuidanceService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                dataGuidanceDataSetService ?? Mock.Of<IDataGuidanceDataSetService>(),
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
                userService ?? Mock.Of<IUserService>(),
                releaseDataFileRepository ?? Mock.Of<IReleaseDataFileRepository>());
        }
    }
}
