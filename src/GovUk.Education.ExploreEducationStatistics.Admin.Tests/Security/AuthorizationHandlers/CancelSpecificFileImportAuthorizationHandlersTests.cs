using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class CancelSpecificFileImportAuthorizationHandlersTests
    {
        [Fact]
        public async Task CannotCancelFinishedOrAbortingImport()
        {
            var finishedOrAbortingStatuses = EnumUtil
                .GetEnumValues<DataImportStatus>()
                .Where(status => status.IsFinishedOrAborting())
                .ToList();

            await finishedOrAbortingStatuses
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async status =>
                {
                    var file = new File
                    {
                        Id = Guid.NewGuid()
                    };

                    var importRepository = new Mock<IDataImportRepository>();

                    importRepository
                        .Setup(s => s.GetStatusByFileId(file.Id))
                        .ReturnsAsync(status);

                    // Assert that no users can cancel a finished or aborting Import
                    await AssertHandlerSucceedsWithCorrectClaims<File, CancelSpecificFileImportRequirement>(
                        new CancelSpecificFileImportAuthorizationHandler(importRepository.Object), file);
                });
        }

        [Fact]
        public async Task CanCancelHealthyOngoingImportWithCorrectClaim()
        {
            var nonFinishedOrAbortingStatuses = EnumUtil
                .GetEnumValues<DataImportStatus>()
                .Where(status => !status.IsFinishedOrAborting())
                .ToList();

            await nonFinishedOrAbortingStatuses
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async status =>
            {
                var file = new File
                {
                    Id = Guid.NewGuid()
                };

                var importRepository = new Mock<IDataImportRepository>();

                importRepository
                    .Setup(s => s.GetStatusByFileId(file.Id))
                    .ReturnsAsync(status);

                // Assert that users with the CancelAllFileImports claim can cancel a non-finished-or-aborting Import
                await AssertHandlerSucceedsWithCorrectClaims<File, CancelSpecificFileImportRequirement>(
                    new CancelSpecificFileImportAuthorizationHandler(importRepository.Object),
                    file,
                    SecurityClaimTypes.CancelAllFileImports);
            });
        }
    }
}
