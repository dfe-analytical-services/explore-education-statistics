#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Security.ContentSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class PublicationServicePermissionTests
{
    private readonly Publication _publication = new()
    {
        Slug = "publication-slug"
    };

    [Fact]
    public async Task Get()
    {
        await PolicyCheckBuilder<ContentSecurityPolicies>()
            .SetupResourceCheckToFail(_publication, CanViewSpecificPublication)
            .AssertForbidden(
                userService =>
                {
                    var service = BuildService(userService: userService.Object);
                    return service.Get(_publication.Slug);
                }
            );
    }

    private PublicationService BuildService(
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IPublicationRepository? publicationRepository = null,
        IUserService? userService = null)
    {
        return new(
            persistenceHelper ?? DefaultPersistenceHelperMock().Object,
            publicationRepository ?? Mock.Of<IPublicationRepository>(MockBehavior.Strict),
            userService ?? Mock.Of<IUserService>(MockBehavior.Strict));
    }

    private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
    {
        return MockUtils.MockPersistenceHelper<ContentDbContext, Publication>(_publication);
    }
}
