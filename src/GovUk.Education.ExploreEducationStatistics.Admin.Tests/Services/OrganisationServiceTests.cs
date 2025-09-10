using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class OrganisationServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetAllOrganisationTests : OrganisationServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public async Task GetAllOrganisations_ReturnsExpectedOrganisationsInOrderByTitle(int numOrganisations)
        {
            // Arrange
            var organisations = _dataFixture.DefaultOrganisation()
                .GenerateArray(numOrganisations)
                .Shuffle();

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
                Assert.Equal(organisations.OrderBy(o => o.Title), result);
            }
        }
    }

    private static OrganisationService BuildService(ContentDbContext context = null)
    {
        return new OrganisationService(
            context ?? Mock.Of<ContentDbContext>(MockBehavior.Strict)
        );
    }
}
