using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseChecklistPermissionServiceTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };


        [Fact]
        public void GetChecklist()
        {
            PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, SecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseChecklistService(userService: userService.Object);
                        return service.GetChecklist(_release.Id);
                    }
                );
        }

        private ReleaseChecklistService BuildReleaseChecklistService(
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            ITableStorageService tableStorageService = null,
            IUserService userService = null,
            IMetaGuidanceService metaGuidanceService = null,
            IFileRepository fileRepository = null,
            IFootnoteRepository footnoteRepository = null,
            IDataBlockService dataBlockService = null)
        {

            return new ReleaseChecklistService(
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                tableStorageService ?? new Mock<ITableStorageService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                metaGuidanceService ?? new Mock<IMetaGuidanceService>().Object,
                fileRepository ?? new Mock<IFileRepository>().Object,
                footnoteRepository ?? new Mock<IFootnoteRepository>().Object,
                dataBlockService ?? new Mock<IDataBlockService>().Object
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(mock, _release.Id, _release);
            return mock;
        }
    }
}