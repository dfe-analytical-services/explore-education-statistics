#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class OrganisationsServiceMockBuilder
{
    private readonly Mock<IOrganisationsService> _mock = new(MockBehavior.Strict);

    private Organisation[]? _organisations;

    public IOrganisationsService Build()
    {
        _mock.Setup(m => m.GetAllOrganisations(It.IsAny<CancellationToken>())).ReturnsAsync(_organisations ?? []);

        return _mock.Object;
    }

    public OrganisationsServiceMockBuilder WhereHasOrganisations(Organisation[] organisations)
    {
        _organisations = organisations;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IOrganisationsService> mock)
    {
        public void GetAllOrganisationsWasCalled()
        {
            mock.Verify(m => m.GetAllOrganisations(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
