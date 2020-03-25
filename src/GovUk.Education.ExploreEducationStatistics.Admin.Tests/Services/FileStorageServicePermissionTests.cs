using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
                service.UploadFilesAsync(
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
                    service.DeleteFileAsync(
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
                        false, 
                        ""
                        ), 
                CanUpdateSpecificRelease);
        }

        [Fact]
        public void DeleteDataFileAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.DeleteDataFileAsync(
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
                        _release.Id
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
            Func<FileStorageService, Task<Either<ActionResult, T>>> protectedAction, params SecurityPolicies[] policies)
        {
            var (configuration, subjectService, userService, releaseHelper, fileTypeService, contentDbContext) = Mocks();

            var service = new FileStorageService(configuration.Object, subjectService.Object, 
                userService.Object, releaseHelper.Object, fileTypeService.Object, contentDbContext.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, _release, userService, service, policies);
        }
        
        private (
            Mock<IConfiguration>,
            Mock<ISubjectService>,
            Mock<IUserService>, 
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<IFileTypeService>,
            Mock<ContentDbContext>) Mocks()
        {
            var mockConf= new Mock<IConfiguration>();
            mockConf.Setup(c => c.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);  
            
            return (
                mockConf,
                new Mock<ISubjectService>(), 
                new Mock<IUserService>(), 
                MockUtils.MockPersistenceHelper<ContentDbContext, Release>(_release.Id, _release),
                new Mock<IFileTypeService>(),
                new Mock<ContentDbContext>());
        }
    }
}
