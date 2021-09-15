using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyImageServicePermissionTests
    {
        private readonly MethodologyVersion _methodologyVersion = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task DeleteAll()
        {
            var methodologyFileRepository = new Mock<IMethodologyFileRepository>(Strict);

            methodologyFileRepository.Setup(mock => mock.GetByFileType(_methodologyVersion.Id, Image))
                .ReturnsAsync(new List<MethodologyFile>
                {
                    new ()
                    {
                        Id = Guid.NewGuid(),
                        FileId = Guid.NewGuid()
                    }
                });

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodologyVersion, SecurityPolicies.CanUpdateSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyImageService(
                            methodologyFileRepository: methodologyFileRepository.Object,
                            userService: userService.Object);
                        return service.DeleteAll(_methodologyVersion.Id);
                    }
                );
        }

        [Fact]
        public async Task Delete()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodologyVersion, SecurityPolicies.CanUpdateSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyImageService(userService: userService.Object);
                        return service.Delete(methodologyVersionId: _methodologyVersion.Id,
                            fileIds: new List<Guid>());
                    }
                );
        }

        [Fact]
        public async Task Upload()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodologyVersion, SecurityPolicies.CanUpdateSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyImageService(userService: userService.Object);
                        return service.Upload(methodologyVersionId: _methodologyVersion.Id,
                            formFile: new Mock<IFormFile>().Object);
                    }
                );
        }

        private MethodologyImageService SetupMethodologyImageService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IBlobStorageService blobStorageService = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null,
            IFileRepository fileRepository = null,
            IMethodologyFileRepository methodologyFileRepository = null,
            IUserService userService = null)
        {
            return new(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                blobStorageService ?? Mock.Of<IBlobStorageService>(),
                fileUploadsValidatorService ?? Mock.Of<IFileUploadsValidatorService>(),
                fileRepository ?? Mock.Of<IFileRepository>(),
                methodologyFileRepository ?? Mock.Of<IMethodologyFileRepository>(),
                userService ?? Mock.Of<IUserService>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            return MockUtils.MockPersistenceHelper<ContentDbContext, MethodologyVersion>(
                _methodologyVersion.Id, _methodologyVersion);
        }
    }
}
