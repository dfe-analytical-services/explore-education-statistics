using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
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
                        Content = new List<ContentBlock>
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

        private readonly Comment _comment = new Comment
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void AddCommentAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.AddCommentAsync(
                        _release.Id,
                        ContentSectionId,
                        ContentBlockId,
                        new CommentSaveRequest()),
                _release,
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void DeleteCommentAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.DeleteCommentAsync(_comment.Id),
                _comment,
                SecurityPolicies.CanUpdateSpecificComment);
        }

        [Fact]
        public void UpdateCommentAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.UpdateCommentAsync(
                        _comment.Id,
                        new CommentSaveRequest()),
                _comment,
                SecurityPolicies.CanUpdateSpecificComment);
        }

        [Fact]
        public void AddContentBlockAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.AddContentBlockAsync(
                        _release.Id,
                        ContentSectionId,
                        new ContentBlockAddRequest()),
                _release,
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void AddContentSectionAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.AddContentSectionAsync(
                        _release.Id,
                        new ContentSectionAddRequest()),
                _release,
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void AttachDataBlock()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.AttachDataBlock(
                        _release.Id,
                        ContentSectionId,
                        new ContentBlockAttachRequest()),
                _release,
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
                _release,
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void RemoveContentSectionAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.RemoveContentSectionAsync(
                        _release.Id,
                        ContentSectionId),
                _release,
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
                _release,
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void ReorderContentSectionsAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.ReorderContentSectionsAsync(
                        _release.Id,
                        new Dictionary<Guid, int>()),
                _release,
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
                _release,
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
                        new ContentBlockUpdateRequest()),
                _release,
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
                        new DataBlockUpdateRequest()),
                _release,
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        private void AssertSecurityPoliciesChecked<T, TProtectedResource>(
            Func<ContentService, Task<Either<ActionResult, T>>> protectedAction,
            TProtectedResource resource,
            params SecurityPolicies[] policies)
        {
            var (contentDbContext, persistenceHelper, mapper, userService) = Mocks();

            var service = new ContentService(contentDbContext.Object, persistenceHelper.Object, mapper.Object, userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, resource, userService, service, policies);
        }

        private (
            Mock<ContentDbContext>,
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<IMapper>,
            Mock<IUserService>) Mocks()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall(persistenceHelper, _release.Id, _release);
            MockUtils.SetupCall(persistenceHelper, _comment.Id, _comment);

            return (
                new Mock<ContentDbContext>(),
                persistenceHelper,
                new Mock<IMapper>(),
                new Mock<IUserService>());
        }
    }
}
