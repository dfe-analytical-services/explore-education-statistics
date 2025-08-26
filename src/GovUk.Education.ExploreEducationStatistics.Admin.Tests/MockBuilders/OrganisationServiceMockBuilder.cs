#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class OrganisationServiceMockBuilder
{
    private readonly Mock<IOrganisationService> _mock = new(MockBehavior.Strict);

    private Organisation[]? _organisations;

    public IOrganisationService Build()
    {
        _mock.Setup(m => m.GetAllOrganisations(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_organisations ?? []);

        return _mock.Object;
    }

    public OrganisationServiceMockBuilder WhereHasOrganisations(Organisation[] organisations)
    {
        _organisations = organisations;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IOrganisationService> mock)
    {
        public void GetAllOrganisationsWasCalled()
        {
            mock.Verify(m => m.GetAllOrganisations(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
