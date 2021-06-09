using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ImportStatusBauServiceTests;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ImportStatusBauServicePermissionTests
    {
        [Fact]
        public void GetAllIncompleteImports()
        {
            PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.CanAccessAllImports)
                .AssertSuccess(
                    async userService =>
                    {
                        var importStatusBauService = BuildImportStatusBauService(
                            contentDbContext: InMemoryApplicationDbContext(),
                            userService: userService.Object);
                        return await importStatusBauService.GetAllIncompleteImports();
                    });
        }
    }
}