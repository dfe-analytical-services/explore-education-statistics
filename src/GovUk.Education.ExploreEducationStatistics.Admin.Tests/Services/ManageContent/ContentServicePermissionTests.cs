using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent
{
    public class ContentServicePermissionTests
    {
        private static readonly Guid ContentSectionId = Guid.NewGuid();
        private static readonly Guid ContentBlockId = Guid.NewGuid();
        
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid(),
            Content = new List<ReleaseContentSection>
            {
                new ReleaseContentSection
                {
                    ContentSection = new ContentSection
                    {
                        Id = ContentSectionId,
                        Content = new List<IContentBlock>
                        {
                            new DataBlock
                            {
                                Id = ContentBlockId
                            }
                        }
                    }
                }
            }
        };

        [Fact]
        public void AddCommentAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.AddCommentAsync(
                        _release.Id,
                        ContentSectionId,
                        ContentBlockId,
                        new AddCommentRequest()), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void DeleteCommentAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.DeleteCommentAsync(
                        _release.Id,
                        ContentSectionId,
                        ContentBlockId,
                        Guid.NewGuid()), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void UpdateCommentAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.UpdateCommentAsync(
                        _release.Id,
                        ContentSectionId,
                        ContentBlockId,
                        Guid.NewGuid(),
                        new UpdateCommentRequest()), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void AddContentBlockAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.AddContentBlockAsync(
                        _release.Id,
                        ContentSectionId,
                        new AddContentBlockRequest()), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void AddContentSectionAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.AddContentSectionAsync(
                        _release.Id,
                        new AddContentSectionRequest()), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void AttachContentBlockAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.AttachContentBlockAsync(
                        _release.Id,
                        ContentSectionId,
                        new AttachContentBlockRequest()), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void RemoveContentBlockAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.RemoveContentBlockAsync(
                        _release.Id,
                        ContentSectionId,
                        ContentBlockId), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void RemoveContentSectionAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.RemoveContentSectionAsync(
                        _release.Id,
                        ContentSectionId), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void ReorderContentBlocksAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.ReorderContentBlocksAsync(
                        _release.Id,
                        ContentSectionId,
                        new Dictionary<Guid, int>()), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void ReorderContentSectionsAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.ReorderContentSectionsAsync(
                        _release.Id,
                        new Dictionary<Guid, int>()), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void UpdateContentSectionHeadingAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.UpdateContentSectionHeadingAsync(
                        _release.Id,
                        ContentSectionId,
                        ""), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void UpdateTextBasedContentBlockAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.UpdateTextBasedContentBlockAsync(
                        _release.Id,
                        ContentSectionId,
                        ContentBlockId,
                        new UpdateTextBasedContentBlockRequest()), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void UpdateDataBlockAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.UpdateDataBlockAsync(
                        _release.Id,
                        ContentSectionId,
                        ContentBlockId,
                        new UpdateDataBlockRequest()), 
                SecurityPolicies.CanUpdateSpecificRelease);
        }
        
        private void AssertSecurityPoliciesChecked<T>(
            Func<ContentService, Task<Either<ActionResult, T>>> protectedAction, params SecurityPolicies[] policies)
        {
            var (contentDbContext, releaseHelper, mapper, userService) = Mocks();

            var service = new ContentService(contentDbContext.Object, releaseHelper.Object, mapper.Object, userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, _release, userService, service, policies);
        }
        
        private (
            Mock<ContentDbContext>,
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<IMapper>,
            Mock<IUserService>) Mocks()
        {
            return (
                new Mock<ContentDbContext>(), 
                MockUtils.MockPersistenceHelper<ContentDbContext, Release>(_release.Id, _release), 
                new Mock<IMapper>(), 
                new Mock<IUserService>());
        }
    }
}