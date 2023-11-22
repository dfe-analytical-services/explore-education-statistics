using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseChecklistPermissionServiceTests
    {
        private readonly Release _release = new()
        {
            Id = Guid.NewGuid(),
            Publication = new Publication()
        };

        [Fact]
        public async Task GetChecklist()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFailWithMatcher<Release>(
                    r => r.Id == _release.Id,
                    ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(async userService =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        await contentDbContext.AddRangeAsync(_release);
                        await contentDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        var service = BuildReleaseChecklistService(
                            contentDbContext: contentDbContext,
                            userService: userService.Object);
                        return await service.GetChecklist(_release.Id);
                    }
                });
        }

        private ReleaseChecklistService BuildReleaseChecklistService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IDataImportService dataImportService = null,
            IUserService userService = null,
            IDataGuidanceService dataGuidanceService = null,
            IReleaseDataFileRepository releaseDataFileRepository = null,
            IMethodologyVersionRepository methodologyVersionRepository = null,
            IFootnoteRepository footnoteRepository = null,
            IDataBlockService dataBlockService = null)
        {

            return new(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                dataImportService ?? new Mock<IDataImportService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                dataGuidanceService ?? new Mock<IDataGuidanceService>().Object,
                releaseDataFileRepository ?? new Mock<IReleaseDataFileRepository>().Object,
                methodologyVersionRepository ?? new Mock<IMethodologyVersionRepository>().Object,
                footnoteRepository ?? new Mock<IFootnoteRepository>().Object,
                dataBlockService ?? new Mock<IDataBlockService>().Object
            );
        }
    }
}
