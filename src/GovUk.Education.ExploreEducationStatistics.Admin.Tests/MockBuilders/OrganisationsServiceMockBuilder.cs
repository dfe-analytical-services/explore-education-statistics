#nullable enable
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class OrganisationsServiceMockBuilder
{
    private readonly Mock<IOrganisationsService> _mock = new(MockBehavior.Strict);

    private OrganisationViewModel[]? _organisations;

    private static readonly Expression<Func<IOrganisationsService, Task<OrganisationViewModel[]>>> GetAllOrganisations =
        m => m.GetAllOrganisations(It.IsAny<CancellationToken>());

    public OrganisationsServiceMockBuilder()
    {
        _mock.Setup(GetAllOrganisations).ReturnsAsync(() => _organisations ?? []);
    }

    public IOrganisationsService Build() => _mock.Object;

    public OrganisationsServiceMockBuilder WhereHasOrganisations(OrganisationViewModel[] organisations)
    {
        _organisations = organisations;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IOrganisationsService> mock)
    {
        public void GetAllOrganisationsWasCalled()
        {
            mock.Verify(GetAllOrganisations, Times.Once);
        }
    }
}
