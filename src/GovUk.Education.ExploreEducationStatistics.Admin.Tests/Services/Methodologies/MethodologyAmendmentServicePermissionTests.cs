using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyAmendmentServicePermissionTests
    {
        private readonly Methodology _methodology = new Methodology
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async void CreateMethodologyAmendment()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodology, CanMakeAmendmentOfSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyAmendmentService(userService: userService.Object);
                        return service.CreateMethodologyAmendment(_methodology.Id);
                    }
                );
        }

        private MethodologyAmendmentService SetupMethodologyAmendmentService(
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IUserService userService = null)
        {
            return new MethodologyAmendmentService(
                contentPersistenceHelper ?? DefaultPersistenceHelperMock().Object,
                userService ?? new Mock<IUserService>().Object,
                MapperUtils.AdminMapper()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            return MockUtils.MockPersistenceHelper<ContentDbContext, Methodology>(_methodology.Id, _methodology);
        }
    }
}
