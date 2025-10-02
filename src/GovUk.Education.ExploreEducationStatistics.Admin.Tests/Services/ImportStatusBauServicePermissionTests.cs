using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ImportStatusBauServiceTests;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ImportStatusBauServicePermissionTests
{
    [Fact]
    public async Task GetAllIncompleteImports()
    {
        await PolicyCheckBuilder()
            .SetupCheck(SecurityPolicies.CanAccessAllImports)
            .AssertSuccess(async userService =>
            {
                var importStatusBauService = BuildImportStatusBauService(
                    contentDbContext: InMemoryApplicationDbContext(),
                    userService: userService.Object
                );
                return await importStatusBauService.GetAllIncompleteImports();
            });
    }
}
