using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations.Dtos;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class OrganisationsServiceMockBuilder
{
    private readonly Mock<IOrganisationsService> _mock = new(MockBehavior.Strict);

    private OrganisationDto[]? _organisations;

    private static readonly Expression<Func<IOrganisationsService, Task<OrganisationDto[]>>> GetAllOrganisations = m =>
        m.GetAllOrganisations(It.IsAny<CancellationToken>());

    public OrganisationsServiceMockBuilder()
    {
        _mock.Setup(GetAllOrganisations).ReturnsAsync(() => _organisations ?? []);
    }

    public IOrganisationsService Build() => _mock.Object;

    public OrganisationsServiceMockBuilder WhereHasOrganisations(OrganisationDto[] organisations)
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
