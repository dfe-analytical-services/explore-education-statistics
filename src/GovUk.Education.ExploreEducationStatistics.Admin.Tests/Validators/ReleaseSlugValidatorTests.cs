using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Validators;

public abstract class ReleaseSlugValidatorTests
{
    private readonly DataFixture _dataFixture = new();

    public class ValidateNewSlugTests : ReleaseSlugValidatorTests
    {
        public class ReleaseExistsTests : ValidateNewSlugTests
        {
            [Fact]
            public async Task TargetReleaseSlugUnchanged_Succeeds()
            {
                Release targetRelease = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-1")
                    .WithPublication(_dataFixture.DefaultPublication());

                using var context = DbUtils.InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Releases.Add(targetRelease);
                await context.SaveChangesAsync();

                var sut = BuildService(context);

                var result = await sut.ValidateNewSlug(
                    newReleaseSlug: "slug-1",
                    publicationId: targetRelease.PublicationId,
                    releaseId: targetRelease.Id
                );

                result.AssertRight();
            }

            [Fact]
            public async Task DifferentReleaseExistsWithSameSlugInDifferentPublication_Succeeds()
            {
                Release targetRelease = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-1")
                    .WithPublication(_dataFixture.DefaultPublication());

                Release otherRelease = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-2")
                    .WithPublication(_dataFixture.DefaultPublication());

                using var context = DbUtils.InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Releases.AddRange(targetRelease, otherRelease);
                await context.SaveChangesAsync();

                var sut = BuildService(context);

                var result = await sut.ValidateNewSlug(
                    newReleaseSlug: "slug-2",
                    publicationId: targetRelease.PublicationId,
                    releaseId: targetRelease.Id
                );

                result.AssertRight();
            }

            [Fact]
            public async Task DifferentReleaseExistsWithSameSlugInSamePublication_BadRequest()
            {
                Publication publication = _dataFixture.DefaultPublication();

                Release targetRelease = _dataFixture.DefaultRelease().WithSlug("slug-2").WithPublication(publication);

                Release otherRelease = _dataFixture.DefaultRelease().WithSlug("slug-1").WithPublication(publication);

                using var context = DbUtils.InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Releases.AddRange(targetRelease, otherRelease);
                await context.SaveChangesAsync();

                var sut = BuildService(context);

                var result = await sut.ValidateNewSlug(
                    newReleaseSlug: "slug-1",
                    publicationId: publication.Id,
                    releaseId: targetRelease.Id
                );

                var validationProblem = result.AssertBadRequestWithValidationProblem();

                var error = Assert.Single(validationProblem.Errors);

                Assert.Equal(ValidationErrorMessages.SlugNotUnique.ToString(), error.Code);
            }

            [Fact]
            public async Task RedirectExistsForNewSlugForDifferentReleaseInSamePublication_BadRequest()
            {
                Publication publication = _dataFixture.DefaultPublication();

                Release targetRelease = _dataFixture.DefaultRelease().WithSlug("slug-2").WithPublication(publication);

                Release otherRelease = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-2")
                    .WithRedirects([_dataFixture.DefaultReleaseRedirect().WithSlug("slug-1")])
                    .WithPublication(publication);

                using var context = DbUtils.InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Releases.AddRange(targetRelease, otherRelease);
                await context.SaveChangesAsync();

                var sut = BuildService(context);

                var result = await sut.ValidateNewSlug(
                    newReleaseSlug: "slug-1",
                    publicationId: publication.Id,
                    releaseId: targetRelease.Id
                );

                var validationProblem = result.AssertBadRequestWithValidationProblem();

                var error = Assert.Single(validationProblem.Errors);

                Assert.Equal(ValidationErrorMessages.ReleaseSlugUsedByRedirect.ToString(), error.Code);
            }

            [Fact]
            public async Task RedirectExistsForNewSlugForDifferentReleaseInDifferentPublication_Succeeds()
            {
                Release targetRelease = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-2")
                    .WithPublication(_dataFixture.DefaultPublication());

                Release otherRelease = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-2")
                    .WithRedirects([_dataFixture.DefaultReleaseRedirect().WithSlug("slug-1")])
                    .WithPublication(_dataFixture.DefaultPublication());

                using var context = DbUtils.InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Releases.AddRange(targetRelease, otherRelease);
                await context.SaveChangesAsync();

                var sut = BuildService(context);

                var result = await sut.ValidateNewSlug(
                    newReleaseSlug: "slug-1",
                    publicationId: targetRelease.PublicationId,
                    releaseId: targetRelease.Id
                );

                result.AssertRight();
            }

            [Fact]
            public async Task RedirectExistsForNewSlugForSameRelease_BadRequest()
            {
                Release targetRelease = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-2")
                    .WithRedirects([_dataFixture.DefaultReleaseRedirect().WithSlug("slug-1")])
                    .WithPublication(_dataFixture.DefaultPublication());

                using var context = DbUtils.InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Releases.Add(targetRelease);
                await context.SaveChangesAsync();

                var sut = BuildService(context);

                var result = await sut.ValidateNewSlug(
                    newReleaseSlug: "slug-1",
                    publicationId: targetRelease.PublicationId,
                    releaseId: targetRelease.Id
                );

                var validationProblem = result.AssertBadRequestWithValidationProblem();

                var error = Assert.Single(validationProblem.Errors);

                Assert.Equal(ValidationErrorMessages.ReleaseSlugUsedByRedirect.ToString(), error.Code);
            }
        }

        public class ReleaseDoesNotExistTests : ValidateNewSlugTests
        {
            [Fact]
            public async Task ReleaseExistsWithSameSlugInDifferentPublication_Succeeds()
            {
                Publication targetPublication = _dataFixture.DefaultPublication();

                Release release = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-1")
                    .WithPublication(_dataFixture.DefaultPublication());

                using var context = DbUtils.InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Publications.Add(targetPublication);
                context.Releases.Add(release);
                await context.SaveChangesAsync();

                var sut = BuildService(context);

                var result = await sut.ValidateNewSlug(
                    newReleaseSlug: "slug-1",
                    publicationId: targetPublication.Id,
                    releaseId: null
                );

                result.AssertRight();
            }

            [Fact]
            public async Task ReleaseExistsWithSameSlugInSamePublication_BadRequest()
            {
                Release release = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-1")
                    .WithPublication(_dataFixture.DefaultPublication());

                using var context = DbUtils.InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Releases.Add(release);
                await context.SaveChangesAsync();

                var sut = BuildService(context);

                var result = await sut.ValidateNewSlug(
                    newReleaseSlug: "slug-1",
                    publicationId: release.PublicationId,
                    releaseId: null
                );

                var validationProblem = result.AssertBadRequestWithValidationProblem();

                var error = Assert.Single(validationProblem.Errors);

                Assert.Equal(ValidationErrorMessages.SlugNotUnique.ToString(), error.Code);
            }

            [Fact]
            public async Task RedirectExistsForNewSlugInDifferentPublication_Succeeds()
            {
                Publication targetPublication = _dataFixture.DefaultPublication();

                Release release = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-2")
                    .WithRedirects([_dataFixture.DefaultReleaseRedirect().WithSlug("slug-1")])
                    .WithPublication(_dataFixture.DefaultPublication());

                using var context = DbUtils.InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Publications.Add(targetPublication);
                context.Releases.Add(release);
                await context.SaveChangesAsync();

                var sut = BuildService(context);

                var result = await sut.ValidateNewSlug(
                    newReleaseSlug: "slug-1",
                    publicationId: targetPublication.Id,
                    releaseId: null
                );

                result.AssertRight();
            }

            [Fact]
            public async Task RedirectExistsForNewSlugInSamePublication_BadRequest()
            {
                Release release = _dataFixture
                    .DefaultRelease()
                    .WithSlug("slug-2")
                    .WithRedirects([_dataFixture.DefaultReleaseRedirect().WithSlug("slug-1")])
                    .WithPublication(_dataFixture.DefaultPublication());

                using var context = DbUtils.InMemoryApplicationDbContext(Guid.NewGuid().ToString());
                context.Releases.Add(release);
                await context.SaveChangesAsync();

                var sut = BuildService(context);

                var result = await sut.ValidateNewSlug(
                    newReleaseSlug: "slug-1",
                    publicationId: release.PublicationId,
                    releaseId: null
                );

                var validationProblem = result.AssertBadRequestWithValidationProblem();

                var error = Assert.Single(validationProblem.Errors);

                Assert.Equal(ValidationErrorMessages.ReleaseSlugUsedByRedirect.ToString(), error.Code);
            }
        }
    }

    private static ReleaseSlugValidator BuildService(ContentDbContext contentDbContext)
    {
        return new ReleaseSlugValidator(contentDbContext);
    }
}
