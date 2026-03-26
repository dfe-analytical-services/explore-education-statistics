#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public class CancelSpecificFileImportAuthorizationHandlersTests
{
    private static readonly DataFixture _dataFixture = new();

    private static readonly File file = _dataFixture.DefaultFile();

    [Fact]
    public async Task FinishedOrAbortingImport_Fails()
    {
        var finishedOrAbortingStatuses = EnumUtil
            .GetEnums<DataImportStatus>()
            .Where(status => status.IsFinishedOrAborting())
            .ToList();

        foreach (var status in finishedOrAbortingStatuses)
        {
            var importRepository = new Mock<IDataImportRepository>();
            importRepository.Setup(s => s.GetStatusByFileId(file.Id)).ReturnsAsync(status);

            // Assert that no users can cancel a finished or aborting Import
            await AssertHandlerSucceedsWithCorrectClaims<CancelSpecificFileImportRequirement, File>(
                handler: SetupHandler(importRepository.Object),
                entity: file,
                claimsExpectedToSucceed: []
            );
        }
    }

    [Fact]
    public async Task HealthyOngoingImport_SucceedsOnlyForValidClaims()
    {
        var nonFinishedOrAbortingStatuses = EnumUtil
            .GetEnums<DataImportStatus>()
            .Where(status => !status.IsFinishedOrAborting())
            .ToList();

        foreach (var status in nonFinishedOrAbortingStatuses)
        {
            var importRepository = new Mock<IDataImportRepository>();
            importRepository.Setup(s => s.GetStatusByFileId(file.Id)).ReturnsAsync(status);

            // Assert that users with the CancelAllFileImports claim can cancel a non-finished-or-aborting Import
            await AssertHandlerSucceedsWithCorrectClaims<CancelSpecificFileImportRequirement, File>(
                handler: SetupHandler(importRepository.Object),
                entity: file,
                claimsExpectedToSucceed: [SecurityClaimTypes.CancelAllFileImports]
            );
        }
    }

    private static CancelSpecificFileImportAuthorizationHandler SetupHandler(
        IDataImportRepository? dataImportRepository = null
    ) => new(dataImportRepository ?? Mock.Of<IDataImportRepository>(MockBehavior.Strict));
}
