using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FileStorageServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void UploadFilesAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                service.UploadFileAsync(
                    _release.Id,
                    new Mock<IFormFile>().Object,
                    "",
                    ReleaseFileTypes.Ancillary,
                    false
                    ),
                CanUpdateSpecificRelease);
        }

        [Fact]
        public void DeleteFileAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.DeleteNonDataFileAsync(
                        _release.Id,
                        ReleaseFileTypes.Ancillary,
                        ""
                        ),
                CanUpdateSpecificRelease);
        }

        [Fact]
        public void UploadDataFilesAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.UploadDataFilesAsync(
                        _release.Id,
                        new Mock<IFormFile>().Object,
                        new Mock<IFormFile>().Object,
                        "",
                        ""
                        ),
                CanUpdateSpecificRelease);
        }

        [Fact]
        public void DeleteDataFileAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.DeleteDataFilesAsync(
                        _release.Id,
                        ""
                        ),
                CanUpdateSpecificRelease);
        }

        [Fact]
        public void ListFilesAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.ListFilesAsync(
                        _release.Id,
                        ReleaseFileTypes.Ancillary
                    ),
                CanViewSpecificRelease);
        }

        [Fact]
        public void ListPublicFilesPreview()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.ListPublicFilesPreview(
                        _release.Id,
                        new List<Guid>(){_release.Id}
                    ),
                CanViewSpecificRelease);
        }

        [Fact]
        public void StreamFile()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.StreamFile(
                        _release.Id,
                        ReleaseFileTypes.Ancillary,
                        ""
                    ),
                CanViewSpecificRelease);
        }

        private void AssertSecurityPoliciesChecked<T>(
            Func<ReleaseFilesService, Task<Either<ActionResult, T>>> protectedAction, params SecurityPolicies[] policies)
        {
            var (blobStorageService, userService, releaseHelper,
                contentDbContext, importService,
                fileUploadsValidatorService, subjectService,
                dataArchiveValidationService) = Mocks();

            var service = new ReleaseFilesService(
                blobStorageService.Object,
                userService.Object,
                releaseHelper.Object,
                contentDbContext.Object,
                importService.Object,
                fileUploadsValidatorService.Object,
                subjectService.Object,
                dataArchiveValidationService.Object
            );

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, _release, userService, service, policies);
        }

        private (
            Mock<IBlobStorageService>,
            Mock<IUserService>,
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<ContentDbContext>,
            Mock<IImportService>,
            Mock<IFileUploadsValidatorService>,
            Mock<ISubjectService>,
            Mock<IDataArchiveValidationService>
            ) Mocks()
        {
            return (
                new Mock<IBlobStorageService>(),
                new Mock<IUserService>(),
                MockUtils.MockPersistenceHelper<ContentDbContext, Release>(_release.Id, _release),
                new Mock<ContentDbContext>(),
                new Mock<IImportService>(),
                new Mock<IFileUploadsValidatorService>(),
                new Mock<ISubjectService>(),
                new Mock<IDataArchiveValidationService>()
                );
        }
    }
}
