using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class CancelSpecificFileImportAuthorizationHandlersTests
    {
        [Fact]
        public void CannotCancelFinishedImport()
        {
            var releaseId = Guid.NewGuid();
            var dataFileName = "my_data_file.csv";
            
            var importStatusService = new Mock<IImportStatusService>();

            importStatusService
                .Setup(s => s.IsImportFinished(releaseId, dataFileName))
                .ReturnsAsync(true);

            // Assert that no users can cancel a finished Import
            AssertHandlerSucceedsWithCorrectClaims<ReleaseFileImportInfo, CancelSpecificFileImportRequirement>(
                new CancelSpecificFileImportAuthorizationHandler(importStatusService.Object), new ReleaseFileImportInfo
                {
                    ReleaseId = releaseId,
                    DataFileName = dataFileName,
                });
        }
        
        [Fact]
        public void CanCancelFinishedImportWithCorrectClaim()
        {
            var releaseId = Guid.NewGuid();
            var dataFileName = "my_data_file.csv";
            
            var importStatusService = new Mock<IImportStatusService>();

            importStatusService
                .Setup(s => s.IsImportFinished(releaseId, dataFileName))
                .ReturnsAsync(false);

            // Assert that no users can cancel a finished Import
            AssertHandlerSucceedsWithCorrectClaims<ReleaseFileImportInfo, CancelSpecificFileImportRequirement>(
                new CancelSpecificFileImportAuthorizationHandler(importStatusService.Object), new ReleaseFileImportInfo
                {
                    ReleaseId = releaseId,
                    DataFileName = dataFileName,
                },
                SecurityClaimTypes.CancelAllFileImports);
        }
    }
}