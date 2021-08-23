#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyServicePermissionTests
    {
        private readonly Publication _publication = new()
        {
            Id = Guid.NewGuid()
        };

        private readonly Methodology _methodology = new()
        {
            Id = Guid.NewGuid(),
            AlternativeTitle = "Title",
            Status = Draft
        };

        private readonly Methodology _approvedMethodology = new()
        {
            Id = Guid.NewGuid(),
            Status = Approved
        };

        [Fact]
        public async Task CreateMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_publication, CanCreateMethodologyForSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Publication>(
                                _publication.Id, _publication).Object,
                            userService: userService.Object);
                        return service.CreateMethodology(_publication.Id);
                    }
                );
        }

        [Fact]
        public async Task GetSummary()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodology, CanViewSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Methodology>(
                                _methodology.Id, _methodology).Object,
                            userService: userService.Object);
                        return service.GetSummary(_methodology.Id);
                    }
                );
        }

        [Fact]
        public async Task GetUnpublishedReleasesUsingMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodology, CanApproveSpecificMethodology)
                .SetupResourceCheckToFail(_methodology, CanMarkSpecificMethodologyAsDraft)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Methodology>(
                                _methodology.Id, _methodology).Object,
                            userService: userService.Object);
                        return service.GetUnpublishedReleasesUsingMethodology(_methodology.Id);
                    }
                );
        }

        [Fact]
        public async Task UpdateMethodologyDetails()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodology, CanUpdateSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Methodology>(
                                _methodology.Id, _methodology).Object,
                            userService: userService.Object);
                        return service.UpdateMethodology(_methodology.Id, new MethodologyUpdateRequest
                        {
                            Status = Draft,
                            Title = "Updated Title"
                        });
                    }
                );
        }

        [Fact]
        public async Task UpdateMethodologyStatus_Approve()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodology, CanApproveSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Methodology>(
                                _methodology.Id, _methodology).Object,
                            userService: userService.Object);
                        return service.UpdateMethodology(_methodology.Id, new MethodologyUpdateRequest
                        {
                            Status = Approved
                        });
                    }
                );
        }

        [Fact]
        public async Task UpdateMethodologyStatus_MarkAsDraft()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_approvedMethodology, CanMarkSpecificMethodologyAsDraft)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Methodology>(
                                _approvedMethodology.Id, _approvedMethodology).Object,
                            userService: userService.Object);
                        return service.UpdateMethodology(_approvedMethodology.Id, new MethodologyUpdateRequest
                        {
                            Status = Draft
                        });
                    }
                );
        }

        [Fact]
        public async Task DeleteMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodology, CanDeleteSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Methodology>(
                                _methodology.Id, _methodology).Object,
                            userService: userService.Object);
                        return service.DeleteMethodology(_methodology.Id);
                    }
                );
        }

        private MethodologyService SetupMethodologyService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IBlobCacheService? blobCacheService = null,
            IMethodologyContentService? methodologyContentService = null,
            IMethodologyFileRepository? methodologyFileRepository = null,
            IMethodologyRepository? methodologyRepository = null,
            IMethodologyImageService? methodologyImageService = null,
            IPublishingService? publishingService = null,
            IUserService? userService = null)
        {
            return new(
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                AdminMapper(),
                blobCacheService ?? Mock.Of<IBlobCacheService>(),
                methodologyContentService ?? Mock.Of<IMethodologyContentService>(),
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                methodologyRepository ?? Mock.Of<IMethodologyRepository>(),
                methodologyImageService ?? Mock.Of<IMethodologyImageService>(),
                publishingService ?? Mock.Of<IPublishingService>(),
                userService ?? Mock.Of<IUserService>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            return MockPersistenceHelper<ContentDbContext, Methodology>(_methodology.Id, _methodology);
        }
    }
}
