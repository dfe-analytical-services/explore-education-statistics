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
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyPermissionTests
    {
        private readonly Publication _publication = new Publication
        {
            Id = Guid.NewGuid()
        };

        private readonly Methodology _methodology = new Methodology
        {
            Id = Guid.NewGuid(),
            Status = Draft
        };

        [Fact]
        public async Task CreateMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_publication, SecurityPolicies.CanCreateMethodologyForSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Publication>(_publication.Id, _publication).Object,
                            userService: userService.Object);
                        return service.CreateMethodology(_publication.Id);
                    }
                );
        }
        
        [Fact]
        public async Task GetSummary()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodology, SecurityPolicies.CanViewSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Methodology>(_methodology.Id, _methodology).Object,
                            userService: userService.Object);
                        return service.GetSummary(_methodology.Id);
                    }
                );
        }
        
        [Fact]
        public async Task UpdateMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodology, SecurityPolicies.CanUpdateSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Methodology>(_methodology.Id, _methodology).Object,
                            userService: userService.Object);
                        return service.UpdateMethodology(_methodology.Id, new MethodologyUpdateRequest
                        {
                            Status    = Draft
                        });
                    }
                );
        }
        
        [Fact]
        public async Task DeleteMethodology()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodology, SecurityPolicies.CanDeleteSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyService(
                            contentPersistenceHelper: MockPersistenceHelper<ContentDbContext, Methodology>(_methodology.Id, _methodology).Object,
                            userService: userService.Object);
                        return service.DeleteMethodology(_methodology.Id);
                    }
                );
        }

        private MethodologyService SetupMethodologyService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            ICacheService cacheService = null,
            IMethodologyContentService methodologyContentService = null,
            IMethodologyFileRepository methodologyFileRepository = null,
            IMethodologyRepository methodologyRepository = null,
            IMethodologyImageService methodologyImageService = null,
            IPublishingService publishingService = null,
            IUserService userService = null)
        {
            return new MethodologyService(
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                AdminMapper(),
                cacheService ?? new Mock<ICacheService>().Object,
                methodologyContentService ?? new Mock<IMethodologyContentService>().Object,
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                methodologyRepository ?? new Mock<IMethodologyRepository>().Object,
                methodologyImageService ?? new Mock<IMethodologyImageService>().Object,
                publishingService ?? new Mock<IPublishingService>().Object,
                userService ?? new Mock<IUserService>().Object
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            return MockPersistenceHelper<ContentDbContext, Methodology>(_methodology.Id, _methodology);
        }
    }
}
