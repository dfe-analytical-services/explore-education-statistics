#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public abstract class OrganisationsControllerTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly OrganisationServiceMockBuilder _organisationService = new();

    public class GetAllOrganisationsTests : OrganisationsControllerTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public async Task GetAllOrganisations_ReturnsExpectedOrganisations(int numOrganisations)
        {
            // Arrange
            var organisations = _dataFixture.DefaultOrganisation()
                .GenerateArray(numOrganisations);
            _organisationService.WhereHasOrganisations(organisations);
            var sut = BuildController();

            // Act
            var result = await sut.GetAllOrganisations();

            // Assert
            _organisationService.Assert.GetAllOrganisationsWasCalled();
            Assert.Equal(numOrganisations, result.Length);
            Assert.All(result,
                (organisation, index) =>
                {
                    var expectedOrganisation = organisations[index];
                    Assert.Equal(expectedOrganisation.Id, organisation.Id);
                    Assert.Equal(expectedOrganisation.Title, organisation.Title);
                    Assert.Equal(expectedOrganisation.Url, organisation.Url);
                });
        }
    }

    private OrganisationsController BuildController()
    {
        return new OrganisationsController(
            _organisationService.Build()
        );
    }
}
