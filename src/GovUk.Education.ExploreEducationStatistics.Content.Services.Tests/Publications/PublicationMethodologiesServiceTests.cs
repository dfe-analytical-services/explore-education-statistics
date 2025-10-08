using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Publications;

public abstract class PublicationMethodologiesServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetMethodologiesForPublicationTests : PublicationMethodologiesServiceTests
    {
        [Fact]
        public async Task WhenPublicationHasPublishedMethodologies_ReturnsMethodologies()
        {
            // Arrange
            var (publication, otherPublication1, otherPublication2) = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .GenerateTuple3();

            var methodologies = _dataFixture
                .DefaultMethodology()
                .ForIndex(0, s => s.SetOwningPublication(publication))
                .ForIndex(1, s => s.SetOwningPublication(otherPublication1).SetAdoptingPublications([publication]))
                .ForIndex(2, s => s.SetOwningPublication(otherPublication2).SetAdoptingPublications([publication]))
                .WithMethodologyVersions(_ => [_dataFixture.DefaultMethodologyVersion()])
                .FinishWith(m => m.LatestPublishedVersion = m.Versions[0])
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.AddRange(publication, otherPublication1, otherPublication2);
                context.Methodologies.AddRange(methodologies);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetPublicationMethodologies(publication.Slug);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal(methodologies.Count, result.Methodologies.Length);
                Assert.All(
                    methodologies,
                    (expected, index) =>
                    {
                        var actual = result.Methodologies[index];
                        Assert.Equal(expected.Id, actual.MethodologyId);
                        Assert.Equal(expected.LatestPublishedVersion!.Slug, actual.Slug);
                        Assert.Equal(expected.LatestPublishedVersion!.Title, actual.Title);
                    }
                );
                Assert.Null(result.ExternalMethodology);
            }
        }

        [Fact]
        public async Task WhenPublicationHasExternalMethodology_ReturnsExternalMethodology()
        {
            // Arrange
            var externalMethodology = new ExternalMethodology
            {
                Title = "External methodology",
                Url = "https://test.com/external-methodology",
            };
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithExternalMethodology(externalMethodology);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetPublicationMethodologies(publication.Slug);

                // Assert
                var result = outcome.AssertRight();
                Assert.NotNull(result.ExternalMethodology);
                Assert.Equal(externalMethodology.Title, result.ExternalMethodology.Title);
                Assert.Equal(externalMethodology.Url, result.ExternalMethodology.Url);
                Assert.Empty(result.Methodologies);
            }
        }

        [Fact]
        public async Task WhenMultipleMethodologiesExist_ReturnsMethodologiesOrderedByTitle()
        {
            // Arrange
            var (publication, otherPublication1, otherPublication2) = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .GenerateTuple3();

            var methodologies = _dataFixture
                .DefaultMethodology()
                .ForIndex(
                    0,
                    s =>
                        s.SetOwningPublication(publication)
                            .SetMethodologyVersions(_ =>
                                [_dataFixture.DefaultMethodologyVersion().WithAlternativeTitle("Methodology C")]
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetOwningPublication(otherPublication1)
                            .SetAdoptingPublications([publication])
                            .SetMethodologyVersions(_ =>
                                [_dataFixture.DefaultMethodologyVersion().WithAlternativeTitle("Methodology A")]
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetOwningPublication(otherPublication2)
                            .SetAdoptingPublications([publication])
                            .SetMethodologyVersions(_ =>
                                [_dataFixture.DefaultMethodologyVersion().WithAlternativeTitle("Methodology B")]
                            )
                )
                .WithMethodologyVersions(_ => [_dataFixture.DefaultMethodologyVersion()])
                .FinishWith(m => m.LatestPublishedVersion = m.Versions[0])
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.AddRange(publication, otherPublication1, otherPublication2);
                context.Methodologies.AddRange(methodologies);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetPublicationMethodologies(publication.Slug);

                // Assert
                var result = outcome.AssertRight();

                var expectedMethodologies = methodologies.OrderBy(m => m.LatestPublishedVersion!.Title).ToArray();

                Assert.Equal(expectedMethodologies.Length, result.Methodologies.Length);
                Assert.All(
                    expectedMethodologies,
                    (expectedMethodology, index) =>
                        Assert.Equal(expectedMethodology.Id, result.Methodologies[index].MethodologyId)
                );
            }
        }

        [Fact]
        public async Task WhenPublicationHasNoMethodologies_ReturnsEmpty()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetPublicationMethodologies(publication.Slug);

                // Assert
                var result = outcome.AssertRight();
                Assert.Empty(result.Methodologies);
                Assert.Null(result.ExternalMethodology);
            }
        }

        [Fact]
        public async Task WhenMethodologyHasNoPublishedVersions_MethodologyIsExcludedFromResults()
        {
            // Arrange
            var (publication, otherPublication1, otherPublication2) = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .GenerateTuple3();

            var methodologies = _dataFixture
                .DefaultMethodology()
                .ForIndex(0, s => s.SetOwningPublication(publication))
                .ForIndex(1, s => s.SetOwningPublication(otherPublication1).SetAdoptingPublications([publication]))
                .ForIndex(2, s => s.SetOwningPublication(otherPublication2).SetAdoptingPublications([publication]))
                .WithMethodologyVersions(_ => [_dataFixture.DefaultMethodologyVersion()])
                .GenerateList();

            // Give one of the methodologies a published version
            var publishedMethodology = methodologies[1];
            publishedMethodology.LatestPublishedVersion = publishedMethodology.Versions[0];

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.AddRange(publication, otherPublication1, otherPublication2);
                context.Methodologies.AddRange(methodologies);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetPublicationMethodologies(publication.Slug);

                // Assert
                var result = outcome.AssertRight();

                // Only the published methodology should be in the results
                var methodology = Assert.Single(result.Methodologies);
                Assert.Equal(publishedMethodology.Id, methodology.MethodologyId);
            }
        }

        [Fact]
        public async Task WhenPublicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const string publicationSlug = "publication-that-does-not-exist";
            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetPublicationMethodologies(publicationSlug);

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task WhenPublicationHasNoReleases_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetPublicationMethodologies(publication.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }

        [Fact]
        public async Task WhenPublicationHasNoPublishedRelease_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetPublicationMethodologies(publication.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }
    }

    private static PublicationMethodologiesService BuildService(ContentDbContext contentDbContext) =>
        new(contentDbContext);
}
