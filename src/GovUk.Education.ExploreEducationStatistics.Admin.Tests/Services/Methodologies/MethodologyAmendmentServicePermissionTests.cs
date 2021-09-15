using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyAmendmentServicePermissionTests
    {
        private readonly MethodologyVersion _methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task CreateMethodologyAmendment()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodologyVersion, CanMakeAmendmentOfSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyAmendmentService(userService: userService.Object);
                        return service.CreateMethodologyAmendment(_methodologyVersion.Id);
                    }
                );
        }

        private MethodologyAmendmentService SetupMethodologyAmendmentService(
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IUserService userService = null,
            IMethodologyService methodologyService = null)
        {
            return new MethodologyAmendmentService(
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                userService ?? new Mock<IUserService>().Object,
                methodologyService ?? new Mock<IMethodologyService>().Object,
                InMemoryApplicationDbContext()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            return MockUtils.MockPersistenceHelper<ContentDbContext, MethodologyVersion>(_methodologyVersion.Id, _methodologyVersion);
        }
    }
}
