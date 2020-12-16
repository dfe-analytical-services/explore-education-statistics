using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class CancelSpecificFileImportAuthorizationHandlersTests
    {
        [Fact]
        public void CannotCancelFinishedOrAbortingImport()
        {
            var finishedOrAbortingStatuses = EnumUtil
                .GetEnumValues<IStatus>()
                .Where(ImportStatus.IsFinishedOrAbortingState)
                .ToList();

            finishedOrAbortingStatuses.ForEach(state =>
            {
                var releaseId = Guid.NewGuid();
                var dataFileName = "my_data_file.csv";

                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(releaseId, dataFileName))
                    .ReturnsAsync(new ImportStatus
                    {
                        Status = state
                    });

                // Assert that no users can cancel a finished or aborting Import
                AssertHandlerSucceedsWithCorrectClaims<ReleaseFileImportInfo, CancelSpecificFileImportRequirement>(
                    new CancelSpecificFileImportAuthorizationHandler(importStatusService.Object),
                    new ReleaseFileImportInfo
                    {
                        ReleaseId = releaseId,
                        DataFileName = dataFileName,
                    });
            });
        }
        
        [Fact]
        public void CanCancelFinishedImportWithCorrectClaimAndImportState()
        {
            var nonFinishedOrAbortingStatuses = EnumUtil
                .GetEnumValues<IStatus>()
                .Where(s => !ImportStatus.IsFinishedOrAbortingState(s))
                .ToList();
            
            nonFinishedOrAbortingStatuses.ForEach(state =>
            {
                var releaseId = Guid.NewGuid();
                var dataFileName = "my_data_file.csv";
            
                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(releaseId, dataFileName))
                    .ReturnsAsync(new ImportStatus
                    {
                        Status = state
                    });

                // Assert that no users can cancel a finished Import
                AssertHandlerSucceedsWithCorrectClaims<ReleaseFileImportInfo, CancelSpecificFileImportRequirement>(
                    new CancelSpecificFileImportAuthorizationHandler(importStatusService.Object), new ReleaseFileImportInfo
                    {
                        ReleaseId = releaseId,
                        DataFileName = dataFileName,
                    },
                    SecurityClaimTypes.CancelAllFileImports);
            });
        }
    }
}