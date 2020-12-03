using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Xunit;
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
                .ExpectCheck(SecurityPolicies.CanAccessAllImports)
                .AssertSuccess(
                    async userService =>
                    {
                        var tableStorageService = new Mock<ITableStorageService>(MockBehavior.Strict);
                        tableStorageService
                            .Setup(storageService =>
                                storageService.ExecuteQueryAsync(TableStorageTableNames.DatafileImportsTableName,
                                    It.IsAny<TableQuery<DatafileImport>>()))
                            .ReturnsAsync(new List<DatafileImport>());
                        var importStatusBauService = BuildImportStatusBauService(tableStorageService.Object,
                            userService: userService.Object);
                        return await importStatusBauService.GetAllIncompleteImports();
                    });
        }
    }
}
