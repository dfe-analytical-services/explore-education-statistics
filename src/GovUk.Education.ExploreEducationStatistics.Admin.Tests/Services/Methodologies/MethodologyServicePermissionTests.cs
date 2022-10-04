#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;

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
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = Guid.NewGuid()
                }
            }
        };

        private readonly MethodologyVersion _methodologyVersion = new()
        {
            Id = Guid.NewGuid(),
            AlternativeTitle = "Title",
            Status = Draft
        };

        [Fact]
        public async Task AdoptMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_publication, CanAdoptMethodologyForSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Publication>(
                                _publication.Id, _publication).Object,
                            userService: userService.Object);
                        return service.AdoptMethodology(_publication.Id, _methodologyVersion.Id);
                    }
                );
        }

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
        public async Task DropMethodology()
        {
            var link = new PublicationMethodology
            {
                PublicationId = new Guid(),
                MethodologyId = new Guid()
            };

            var persistenceHelper = new Mock<IPersistenceHelper<ContentDbContext>>(Strict);
            SetupCall(persistenceHelper, link);

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(link, CanDropMethodologyLink)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: persistenceHelper.Object,
                            userService: userService.Object);
                        return service.DropMethodology(link.PublicationId, link.MethodologyId);
                    }
                );
        }

        [Fact]
        public async Task GetAdoptableMethodologies()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_publication, CanAdoptMethodologyForSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Publication>(
                                _publication.Id, _publication).Object,
                            userService: userService.Object);
                        return service.GetAdoptableMethodologies(_publication.Id);
                    }
                );
        }

        [Fact]
        public async Task GetMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodologyVersion, CanViewSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, MethodologyVersion>(
                                _methodologyVersion.Id, _methodologyVersion).Object,
                            userService: userService.Object);
                        return service.GetMethodology(_methodologyVersion.Id);
                    }
                );
        }

        [Fact]
        public async Task GetUnpublishedReleasesUsingMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodologyVersion, CanApproveSpecificMethodology)
                .SetupResourceCheckToFail(_methodologyVersion, CanMarkSpecificMethodologyAsDraft)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, MethodologyVersion>(
                                _methodologyVersion.Id, _methodologyVersion).Object,
                            userService: userService.Object);
                        return service.GetUnpublishedReleasesUsingMethodology(_methodologyVersion.Id);
                    }
                );
        }

        [Fact]
        public async Task ListMethodologies()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_publication, CanViewSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Publication>(
                                _publication.Id, _publication).Object,
                            userService: userService.Object);
                        return service.ListMethodologies(_publication.Id);
                    }
                );
        }

        [Fact]
        public async Task UpdateMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodologyVersion, CanUpdateSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(userService: userService.Object);
                        return service.UpdateMethodology(_methodologyVersion.Id, new MethodologyUpdateRequest
                        {
                            Title = "Updated Title"
                        });
                    }
                );
        }

        [Fact]
        public async Task DeleteMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodology.Versions[0], CanDeleteSpecificMethodology)
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

        [Fact]
        public async Task DeleteMethodologyVersion()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodologyVersion, CanDeleteSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, MethodologyVersion>(
                                _methodologyVersion.Id, _methodologyVersion).Object,
                            userService: userService.Object);
                        return service.DeleteMethodologyVersion(_methodologyVersion.Id);
                    }
                );
        }

        private MethodologyService SetupMethodologyService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IMethodologyRepository? methodologyRepository = null,
            IMethodologyImageService? methodologyImageService = null,
            IMethodologyApprovalService? methodologyApprovalService = null,
            IMethodologyCacheService? methodologyCacheService = null,
            IUserService? userService = null)
        {
            return new(
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                AdminMapper(),
                methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(),
                methodologyRepository ?? Mock.Of<IMethodologyRepository>(),
                methodologyImageService ?? Mock.Of<IMethodologyImageService>(),
                methodologyApprovalService ?? Mock.Of<IMethodologyApprovalService>(Strict),
                methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
            userService ?? Mock.Of<IUserService>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            return MockPersistenceHelper<ContentDbContext, MethodologyVersion>(_methodologyVersion.Id, _methodologyVersion);
        }
    }
}
