#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Validators;

public abstract class OrganisationsValidatorTests
{
    private readonly DataFixture _dataFixture = new();

    public class ValidateOrganisationsTests : OrganisationsValidatorTests
    {
        [Fact]
        public async Task WhenOrganisationIdsIsNull_ReturnsEmpty()
        {
            // Arrange
            await using var context = InMemoryApplicationDbContext();
            var sut = BuildService(context);

            // Act
            var result = await sut.ValidateOrganisations(organisationIds: null);

            // Assert
            var actualOrganisations = result.AssertRight();
            Assert.Empty(actualOrganisations);
        }

        [Fact]
        public async Task WhenOrganisationIdsIsEmpty_ReturnsEmpty()
        {
            // Arrange
            await using var context = InMemoryApplicationDbContext();
            var sut = BuildService(context);

            // Act
            var result = await sut.ValidateOrganisations(organisationIds: []);

            // Assert
            var actualOrganisations = result.AssertRight();
            Assert.Empty(actualOrganisations);
        }

        [Fact]
        public async Task WhenOrganisationIdsExist_ReturnsOrganisations()
        {
            // Arrange
            var organisations = _dataFixture.DefaultOrganisation()
                .GenerateArray(2);
            var organisationIds = GetOrganisationIds(organisations);

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
                var result = await sut.ValidateOrganisations(organisationIds);

                // Assert
                var actualOrganisations = result.AssertRight();
                Assert.Equal(organisations.Length, actualOrganisations.Length);
                Assert.All(organisations, organisation => Assert.Contains(organisation, actualOrganisations));
            }
        }

        [Fact]
        public async Task WhenOrganisationIdsDoNotExist_ReturnsValidationErrors()
        {
            // Arrange
            var organisations = _dataFixture.DefaultOrganisation()
                .GenerateArray(2);
            Guid[] organisationIdsNotExisting = [Guid.NewGuid(), Guid.NewGuid()];
            const string errorPath = "testPath";

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
                var result = await sut.ValidateOrganisations(
                    organisationIds: organisationIdsNotExisting,
                    path: errorPath);

                // Assert
                var validationProblem = result.AssertBadRequestWithValidationProblem();
                Assert.Equal(organisationIdsNotExisting.Length, validationProblem.Errors.Count);
                Assert.All(organisationIdsNotExisting,
                    organisationId =>
                    {
                        validationProblem.AssertHasError(
                            expectedCode: ValidationMessages.OrganisationNotFound.Code,
                            expectedMessage: ValidationMessages.OrganisationNotFound.Message,
                            expectedPath: errorPath,
                            expectedDetail: new InvalidErrorDetail<Guid>(organisationId));
                    });
            }
        }

        [Fact]
        public async Task WhenOrganisationIdsPartiallyExist_ReturnsValidationErrors()
        {
            // Arrange
            var organisations = _dataFixture.DefaultOrganisation()
                .GenerateArray(2);
            var organisationIds = GetOrganisationIds(organisations);
            var organisationIdNotExisting = Guid.NewGuid();
            const string errorPath = "testPath";

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
                var result = await sut.ValidateOrganisations(
                    organisationIds: [.. organisationIds, organisationIdNotExisting],
                    path: errorPath);

                // Assert
                var validationProblem = result.AssertBadRequestWithValidationProblem();
                Assert.Single(validationProblem.Errors);
                validationProblem.AssertHasError(
                    expectedCode: ValidationMessages.OrganisationNotFound.Code,
                    expectedMessage: ValidationMessages.OrganisationNotFound.Message,
                    expectedPath: errorPath,
                    expectedDetail: new InvalidErrorDetail<Guid>(organisationIdNotExisting));
            }
        }

        [Fact]
        public async Task WhenOrganisationIdsHasDuplicates_ReturnsUniqueOrganisations()
        {
            // Arrange
            var organisations = _dataFixture.DefaultOrganisation()
                .GenerateArray(2);
            var organisationIds = GetOrganisationIds(organisations);
            Guid[] organisationIdsWithDuplicates = [.. organisationIds, .. organisationIds];

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
                var result = await sut.ValidateOrganisations(organisationIdsWithDuplicates);

                // Assert
                var actualOrganisations = result.AssertRight();
                Assert.Equal(organisations.Length, actualOrganisations.Length);
                Assert.All(organisations, organisation => Assert.Contains(organisation, actualOrganisations));
            }
        }
    }

    private static Guid[] GetOrganisationIds(Organisation[] organisations) =>
        organisations.Select(o => o.Id).ToArray();

    private static OrganisationsValidator BuildService(ContentDbContext? context = null)
    {
        return new OrganisationsValidator(
            context ?? Mock.Of<ContentDbContext>(MockBehavior.Strict)
        );
    }
}
