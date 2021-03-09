using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyImageServicePermissionTests
    {
        private readonly Methodology _methodology = new Methodology
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void Upload()
        {
            PolicyCheckBuilder<SecurityPolicies>()
                .ExpectResourceCheckToFail(_methodology, SecurityPolicies.CanUpdateSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyImageService(userService: userService.Object);
                        return service.Upload(methodologyId: _methodology.Id,
                            formFile: new Mock<IFormFile>().Object);
                    }
                );
        }

        private MethodologyImageService SetupMethodologyImageService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IBlobStorageService blobStorageService = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null,
            IMethodologyFileRepository methodologyFileRepository = null,
            IUserService userService = null)
        {
            return new MethodologyImageService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>().Object,
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                userService ?? new Mock<IUserService>().Object
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(mock, _methodology.Id, _methodology);
            return mock;
        }
    }
}
