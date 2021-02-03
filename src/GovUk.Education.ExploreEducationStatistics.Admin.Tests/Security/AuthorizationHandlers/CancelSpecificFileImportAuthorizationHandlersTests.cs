using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
                .GetEnumValues<ImportStatus>()
                .Where(status => status.IsFinishedOrAborting())
                .ToList();

            finishedOrAbortingStatuses.ForEach(status =>
            {
                var file = new File
                {
                    Id = Guid.NewGuid()
                };

                var importRepository = new Mock<IImportRepository>();

                importRepository
                    .Setup(s => s.GetStatusByFileId(file.Id))
                    .ReturnsAsync(status);

                // Assert that no users can cancel a finished or aborting Import
                AssertHandlerSucceedsWithCorrectClaims<File, CancelSpecificFileImportRequirement>(
                    new CancelSpecificFileImportAuthorizationHandler(importRepository.Object), file);
            });
        }
        
        [Fact]
        public void CanCancelHealthyOngoingImportWithCorrectClaim()
        {
            var nonFinishedOrAbortingStatuses = EnumUtil
                .GetEnumValues<ImportStatus>()
                .Where(status => !status.IsFinishedOrAborting())
                .ToList();

            nonFinishedOrAbortingStatuses.ForEach(status =>
            {
                var file = new File
                {
                    Id = Guid.NewGuid()
                };

                var importRepository = new Mock<IImportRepository>();

                importRepository
                    .Setup(s => s.GetStatusByFileId(file.Id))
                    .ReturnsAsync(status);

                // Assert that users with the CancelAllFileImports claim can cancel a non-finished-or-aborting Import
                AssertHandlerSucceedsWithCorrectClaims<File, CancelSpecificFileImportRequirement>(
                    new CancelSpecificFileImportAuthorizationHandler(importRepository.Object),
                    file,
                    SecurityClaimTypes.CancelAllFileImports);
            });
        }
    }
}