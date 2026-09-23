#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class OrganisationsServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetAllOrganisationsTests : OrganisationsServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public async Task WhenOrganisationsExist_ReturnsOrganisationsOrderedByTitle(int numOrganisations)
        {
            // Arrange
            var organisations = _dataFixture.DefaultOrganisation().GenerateArray(numOrganisations).Shuffle();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Organisations.AddRange(organisations);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var result = await sut.GetAllOrganisations();

                // Assert
                var expectedOrganisations = organisations.OrderBy(o => o.Title).ToArray();

                Assert.Equal(expectedOrganisations.Length, result.Length);
                Assert.All(
                    expectedOrganisations,
                    (expectedOrganisation, index) =>
                    {
                        var actualOrganisation = result[index];
                        Assert.Equal(expectedOrganisation.Id, actualOrganisation.Id);
                        Assert.Equal(expectedOrganisation.GISLogoHexCode, actualOrganisation.GISLogoHexCode);
                        Assert.Equal(expectedOrganisation.LogoFileName, actualOrganisation.LogoFileName);
                        Assert.Equal(expectedOrganisation.Title, actualOrganisation.Title);
                        Assert.Equal(expectedOrganisation.Url, actualOrganisation.Url);
                        Assert.Equal(expectedOrganisation.UseGISLogo, actualOrganisation.UseGISLogo);
                    }
                );
            }
        }

        [Fact]
        public async Task WhenOrganisationDoesNotUseGISLogo_ReturnsNullHexCode()
        {
            // Arrange
            Organisation organisation = _dataFixture
                .DefaultOrganisation()
                .WithGisLogoHexCode(null)
                .WithLogoFileName("ofsted-logo.png")
                .WithUseGisLogo(false);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Organisations.Add(organisation);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var result = await sut.GetAllOrganisations();

                // Assert
                var actualOrganisation = Assert.Single(result);
                Assert.Null(actualOrganisation.GISLogoHexCode);
                Assert.Equal("ofsted-logo.png", actualOrganisation.LogoFileName);
                Assert.False(actualOrganisation.UseGISLogo);
            }
        }
    }

    private static OrganisationsService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
