using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions;

public abstract class OrganisationExtensionsTests
{
    private readonly DataFixture _dataFixture = new();

    public class OrderByTitleWithDepartmentForEducationFirstTests : OrganisationExtensionsTests
    {
        [Fact]
        public void WhenDepartmentForEducationExists_ReturnsItFirstThenOthersOrderedByTitle()
        {
            // Arrange
            var organisations = _dataFixture
                .DefaultOrganisation()
                .ForIndex(0, s => s.SetTitle("Organisation C"))
                .ForIndex(1, s => s.SetTitle(Organisation.DepartmentForEducationTitle))
                .ForIndex(2, s => s.SetTitle("Organisation A"))
                .ForIndex(3, s => s.SetTitle("Organisation B"))
                .GenerateArray(4);

            // Act
            var result = organisations.OrderByTitleWithDepartmentForEducationFirst();

            // Assert
            string[] expectedTitles =
            [
                Organisation.DepartmentForEducationTitle,
                "Organisation A",
                "Organisation B",
                "Organisation C",
            ];

            Assert.Equal(expectedTitles, result.Select(o => o.Title));
        }

        [Fact]
        public void WhenDepartmentForEducationDoesNotExist_ReturnsOrganisationsOrderedByTitle()
        {
            // Arrange
            var organisations = _dataFixture
                .DefaultOrganisation()
                .ForIndex(0, s => s.SetTitle("Organisation C"))
                .ForIndex(1, s => s.SetTitle("Organisation A"))
                .ForIndex(2, s => s.SetTitle("Organisation B"))
                .GenerateArray(3);

            // Act
            var result = organisations.OrderByTitleWithDepartmentForEducationFirst();

            // Assert
            string[] expectedTitles = ["Organisation A", "Organisation B", "Organisation C"];

            Assert.Equal(expectedTitles, result.Select(o => o.Title));
        }

        [Fact]
        public void WhenNoOrganisationsExist_ReturnsEmpty()
        {
            // Arrange
            Organisation[] organisations = [];

            // Act
            var result = organisations.OrderByTitleWithDepartmentForEducationFirst();

            // Assert
            Assert.Empty(result);
        }
    }
}
