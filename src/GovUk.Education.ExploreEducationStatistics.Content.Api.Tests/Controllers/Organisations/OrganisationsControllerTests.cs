using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Organisations;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Organisations;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Organisations;

public abstract class OrganisationsControllerTests
{
    private readonly OrganisationsServiceMockBuilder _organisationsService = new();

    public class GetAllOrganisationsTests : OrganisationsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsOrganisations_ReturnsOk()
        {
            // Arrange
            OrganisationDto[] organisations =
            [
                new OrganisationDtoBuilder().WithTitle("Organisation A").Build(),
                new OrganisationDtoBuilder().WithTitle("Organisation B").Build(),
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
