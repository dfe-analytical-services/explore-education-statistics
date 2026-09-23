#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public abstract class OrganisationsControllerTests
{
    private readonly OrganisationsServiceMockBuilder _organisationsService = new();

    public class GetAllOrganisationsTests : OrganisationsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsOrganisations_ReturnsOk()
        {
            // Arrange
            OrganisationViewModel[] organisations =
            [
                new OrganisationViewModelBuilder().WithTitle("Organisation A").Build(),
                new OrganisationViewModelBuilder().WithTitle("Organisation B").Build(),
            ];
            _organisationsService.WhereHasOrganisations(organisations);

            var sut = BuildController();

            // Act
            var result = await sut.GetAllOrganisations();

            // Assert
            _organisationsService.Assert.GetAllOrganisationsWasCalled();
            Assert.Equal(organisations, result);
        }

        [Fact]
        public async Task WhenServiceReturnsNoOrganisations_ReturnsEmpty()
        {
            // Arrange
            _organisationsService.WhereHasOrganisations([]);

            var sut = BuildController();

            // Act
            var result = await sut.GetAllOrganisations();

            // Assert
            _organisationsService.Assert.GetAllOrganisationsWasCalled();
            Assert.Empty(result);
        }
    }

    private OrganisationsController BuildController() => new(_organisationsService.Build());
}
