using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Mappings;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class MethodologyServiceTests
{
    private readonly string sitemapItemLastModifiedTime = "2024-05-04T10:24:13";

    [Fact]
    public async Task GetLatestMethodologyBySlug()
    {
        var methodologyVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            OwningPublicationSlug = "publication-title",
            OwningPublicationTitle = "Publication title",
            LatestPublishedVersionId = methodologyVersionId,
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = methodologyVersionId,
                    PublishingStrategy = Immediately,
                    Published = DateTime.UtcNow,
                    Status = Approved,
                    AlternativeSlug = "alternative-title",
                    AlternativeTitle = "Alternative title",
                },
            },
        };

        var publication = new Publication
        {
            Slug = "publication-title",
            Title = "Publication title",
            LatestPublishedReleaseVersionId = Guid.NewGuid(),
            LatestPublishedReleaseVersion = new ReleaseVersion
            {
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug",
                },
            },
            Methodologies = new List<PublicationMethodology>
            {
                new() { Methodology = methodology, Owner = true },
            },
            Contact = new Contact
            {
                TeamEmail = "team-email",
                TeamName = "team-name",
                ContactName = "contact-name",
                ContactTelNo = "contact-tel-no",
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Attach(methodology.Versions[0]);

            var service = SetupMethodologyService(contentDbContext);

            var result = (await service.GetLatestMethodologyBySlug(methodology.Versions[0].Slug)).AssertRight();

            Assert.Equal(methodology.Versions[0].Id, result.Id);
            Assert.Equal(methodology.Versions[0].Published, result.Published);
            Assert.Equal("alternative-title", result.Slug);
            Assert.Equal("Alternative title", result.Title);
            Assert.Empty(result.Annexes);
            Assert.Empty(result.Content);
            Assert.Empty(result.Notes);

            Assert.Single(result.Publications);
            Assert.Equal(publication.Id, result.Publications[0].Id);
            Assert.Equal(publication.Slug, result.Publications[0].Slug);
            Assert.Equal(
                publication.LatestPublishedReleaseVersion!.Release.Slug,
                result.Publications[0].LatestReleaseSlug
            );
            Assert.Equal(publication.Title, result.Publications[0].Title);
            Assert.True(result.Publications[0].Owner);
            Assert.NotNull(result.Publications[0].Contact);
        }
    }

    [Fact]
    public async Task GetLatestMethodologyBySlug_FiltersUnpublishedPublications()
    {
        var methodologyVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            OwningPublicationSlug = "publication-a",
            OwningPublicationTitle = "Publication A",
            LatestPublishedVersionId = methodologyVersionId,
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = methodologyVersionId,
                    PublishingStrategy = Immediately,
                    Published = DateTime.UtcNow,
                    Status = Approved,
                },
            },
        };

        // Publication has a published release and is visible
        var publicationA = new Publication
        {
            Slug = "publication-a",
            Title = "Publication A",
            LatestPublishedReleaseVersion = new ReleaseVersion
            {
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug",
                },
            },
            LatestPublishedReleaseVersionId = Guid.NewGuid(),
            Methodologies = new List<PublicationMethodology>
            {
                new() { Methodology = methodology, Owner = true },
            },
            Contact = new Contact
            {
                TeamEmail = "team-email",
                TeamName = "team-name",
                ContactName = "contact-name",
                ContactTelNo = "contact-tel-no",
            },
        };

        // Publication has no published releases and is not visible
        var publicationB = new Publication
        {
            Slug = "publication-b",
            Title = "Publication B",
            LatestPublishedReleaseVersion = new ReleaseVersion
            {
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug",
                },
            },
            LatestPublishedReleaseVersionId = null,
            Methodologies = new List<PublicationMethodology>
            {
                new() { Methodology = methodology, Owner = false },
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.Publications.AddRangeAsync(publicationA, publicationB);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Attach(methodology.Versions[0]);
            contentDbContext.Attach(publicationA);

            var service = SetupMethodologyService(contentDbContext);

            var result = (await service.GetLatestMethodologyBySlug(methodology.Versions[0].Slug)).AssertRight();

            Assert.Single(result.Publications);
            Assert.Equal(publicationA.Id, result.Publications[0].Id);
            Assert.Equal(publicationA.Slug, result.Publications[0].Slug);
            Assert.Equal(
                publicationA.LatestPublishedReleaseVersion!.Release.Slug,
                result.Publications[0].LatestReleaseSlug
            );
            Assert.Equal(publicationA.Title, result.Publications[0].Title);
        }
    }

    [Fact]
    public async Task GetLatestMethodologyBySlug_TestContentSections()
    {
        var methodologyVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            OwningPublicationSlug = "methodology-title",
            OwningPublicationTitle = "Methodology title",
            LatestPublishedVersionId = methodologyVersionId,
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = methodologyVersionId,
                    MethodologyContent = new MethodologyVersionContent
                    {
                        Annexes = new List<ContentSection>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 2,
                                Heading = "Annex 3 heading",
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 0,
                                Heading = "Annex 1 heading",
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 1,
                                Heading = "Annex 2 heading",
                            },
                        },
                        Content = new List<ContentSection>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 2,
                                Heading = "Section 3 heading",
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 0,
                                Heading = "Section 1 heading",
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 1,
                                Heading = "Section 2 heading",
                            },
                        },
                    },
                    PublishingStrategy = Immediately,
                    Status = Approved,
                    AlternativeTitle = "Alternative title",
                    AlternativeSlug = "alternative-title",
                },
            },
        };
        var methodologyVersion = methodology.Versions[0];

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Attach(methodology.Versions[0]);

            var service = SetupMethodologyService(contentDbContext);

            var result = (await service.GetLatestMethodologyBySlug(methodology.Versions[0].Slug)).AssertRight();

            Assert.Equal(3, result.Annexes.Count);
            Assert.Equal(3, result.Content.Count);

            var expectedAnnex1 = methodologyVersion.MethodologyContent.Annexes.Single(section => section.Order == 0);
            var expectedAnnex2 = methodologyVersion.MethodologyContent.Annexes.Single(section => section.Order == 1);
            var expectedAnnex3 = methodologyVersion.MethodologyContent.Annexes.Single(section => section.Order == 2);

            AssertContentSectionAndViewModelEqual(expectedAnnex1, result.Annexes[0]);
            AssertContentSectionAndViewModelEqual(expectedAnnex2, result.Annexes[1]);
            AssertContentSectionAndViewModelEqual(expectedAnnex3, result.Annexes[2]);

            var expectedContent1 = methodologyVersion.MethodologyContent.Content.Single(section => section.Order == 0);
            var expectedContent2 = methodologyVersion.MethodologyContent.Content.Single(section => section.Order == 1);
            var expectedContent3 = methodologyVersion.MethodologyContent.Content.Single(section => section.Order == 2);

            AssertContentSectionAndViewModelEqual(expectedContent1, result.Content[0]);
            AssertContentSectionAndViewModelEqual(expectedContent2, result.Content[1]);
            AssertContentSectionAndViewModelEqual(expectedContent3, result.Content[2]);
        }
    }

    private static void AssertContentSectionAndViewModelEqual(ContentSection expected, ContentSectionViewModel actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Heading, actual.Heading);
        Assert.Equal(expected.Order, actual.Order);
    }

    [Fact]
    public async Task GetLatestMethodologyBySlug_TestNotes()
    {
        var methodologyVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            OwningPublicationSlug = "methodology-slug",
            OwningPublicationTitle = "Methodology title",
            LatestPublishedVersionId = methodologyVersionId,
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = methodologyVersionId,
                    PublishingStrategy = Immediately,
                    Status = Approved,
                    AlternativeTitle = "Alternative title",
                },
            },
        };
        var methodologyVersion = methodology.Versions[0];

        var otherNote = new MethodologyNote
        {
            DisplayDate = DateTime.Today.AddDays(-2).ToUniversalTime(),
            Content = "Other note",
            MethodologyVersion = methodologyVersion,
        };

        var earliestNote = new MethodologyNote
        {
            DisplayDate = DateTime.Today.AddDays(-3).ToUniversalTime(),
            Content = "Earliest note",
            MethodologyVersion = methodologyVersion,
        };

        var latestNote = new MethodologyNote
        {
            DisplayDate = DateTime.Today.AddDays(-1).ToUniversalTime(),
            Content = "Latest note",
            MethodologyVersion = methodologyVersion,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.MethodologyNotes.AddRangeAsync(otherNote, earliestNote, latestNote);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Attach(methodology.Versions[0]);

            var service = SetupMethodologyService(contentDbContext);

            var result = (await service.GetLatestMethodologyBySlug(methodology.Versions[0].Slug)).AssertRight();

            Assert.Equal(3, result.Notes.Count);

            AssertMethodologyNoteAndViewModelEqual(latestNote, result.Notes[0]);
            AssertMethodologyNoteAndViewModelEqual(otherNote, result.Notes[1]);
            AssertMethodologyNoteAndViewModelEqual(earliestNote, result.Notes[2]);
        }
    }

    private static void AssertMethodologyNoteAndViewModelEqual(
        MethodologyNote expected,
        MethodologyNoteViewModel actual
    )
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Content, actual.Content);
        Assert.Equal(expected.DisplayDate, actual.DisplayDate);
    }

    [Fact]
    public async Task GetLatestMethodologyBySlug_MethodologyHasNoPublishedVersion()
    {
        var methodology = new Methodology
        {
            OwningPublicationSlug = "methodology-title",
            OwningPublicationTitle = "Methodology title",
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext);

            var result = await service.GetLatestMethodologyBySlug(methodology.OwningPublicationSlug);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetLatestMethodologyBySlug_SlugNotFound()
    {
        // Set up a methodology with a different slug to make sure it's not returned
        var methodology = new Methodology { OwningPublicationTitle = "Methodology title" };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.SaveChangesAsync();
        }

        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

        // Expect no call to get the latest published version since no methodology will be found

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            var result = await service.GetLatestMethodologyBySlug("methodology-slug");

            VerifyAllMocks(methodologyVersionRepository);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetSummariesTree()
    {
        var publication = new Publication { Title = "Publication title" };

        var theme = new Theme { Title = "Theme title", Publications = [publication] };

        var methodologyVersion1Id = Guid.NewGuid();
        var methodologyVersion2Id = Guid.NewGuid();
        var latestVersions = ListOf(
            new MethodologyVersion
            {
                Id = methodologyVersion1Id,
                MethodologyContent = new MethodologyVersionContent(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                AlternativeTitle = "Methodology 1 v0 title",
                AlternativeSlug = "methodology-1-slug",
                Version = 0,
                Methodology = new Methodology { LatestPublishedVersionId = methodologyVersion1Id },
            },
            new MethodologyVersion
            {
                Id = methodologyVersion2Id,
                MethodologyContent = new MethodologyVersionContent(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                AlternativeTitle = "Methodology 2 v0 title",
                AlternativeSlug = "methodology-2-slug",
                Version = 0,
                Methodology = new Methodology { LatestPublishedVersionId = methodologyVersion2Id },
            }
        );

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Themes.AddAsync(theme);
            await contentDbContext.SaveChangesAsync();
        }

        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

        methodologyVersionRepository
            .Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
            .ReturnsAsync(latestVersions);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            var result = await service.GetSummariesTree();
            VerifyAllMocks(methodologyVersionRepository);

            var themes = result.AssertRight();

            // Assert the details of the result are correct.
            Assert.Single(themes);

            Assert.Equal(theme.Id, themes[0].Id);
            Assert.Equal("Theme title", themes[0].Title);

            var publications = themes[0].Publications;
            Assert.Single(publications);

            Assert.Equal(publication.Id, publications[0].Id);
            Assert.Equal("Publication title", publications[0].Title);

            var methodologies = publications[0].Methodologies;
            Assert.Equal(2, methodologies.Count);

            Assert.Equal(latestVersions[0].Id, methodologies[0].Id);
            Assert.Equal("methodology-1-slug", methodologies[0].Slug);
            Assert.Equal("Methodology 1 v0 title", methodologies[0].Title);

            Assert.Equal(latestVersions[1].Id, methodologies[1].Id);
            Assert.Equal("methodology-2-slug", methodologies[1].Slug);
            Assert.Equal("Methodology 2 v0 title", methodologies[1].Title);
        }
    }

    [Fact]
    public async Task GetSummariesTree_ThemeWithoutPublicationsIsNotIncluded()
    {
        var theme = new Theme
        {
            Title = "Theme title",
            Slug = "theme-slug",
            Summary = "Theme summary",
            Publications = [],
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Themes.AddAsync(theme);
            await contentDbContext.SaveChangesAsync();
        }

        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            var result = await service.GetSummariesTree();

            VerifyAllMocks(methodologyVersionRepository);

            result.AssertRight();

            Assert.Empty(result.Right);
        }
    }

    [Fact]
    public async Task GetSummariesTree_ThemeWithoutPublishedMethodologiesIsNotIncluded()
    {
        var publication = new Publication { Title = "Publication title", Slug = "publication-slug" };

        var theme = new Theme
        {
            Title = "Theme title",
            Slug = "theme-slug",
            Summary = "Theme summary",
            Publications = ListOf(publication),
        };

        // This test sets up returning an empty list of the latest publicly accessible methodologies for a publication.
        // The theme/publication shouldn't be visible because it has no methodology leaf nodes.
        // This would be the case if:
        // * There are no approved methodologies yet.
        // * All of the approved methodologies depend on other releases which aren't published yet.
        // * If the publication doesn't have at least one published release yet.
        var latestMethodologies = new List<MethodologyVersion>();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Themes.AddAsync(theme);
            await contentDbContext.SaveChangesAsync();
        }

        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

        methodologyVersionRepository
            .Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
            .ReturnsAsync(latestMethodologies);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            var result = await service.GetSummariesTree();

            result.AssertRight();

            Assert.Empty(result.Right);
        }

        VerifyAllMocks(methodologyVersionRepository);
    }

    [Fact]
    public async Task ListSitemapItems()
    {
        var methodologyVersionId = Guid.NewGuid();
        var methodologyUpdatedDate = DateTime.Parse(sitemapItemLastModifiedTime);

        var methodology = new Methodology
        {
            OwningPublicationSlug = "publication-title",
            OwningPublicationTitle = "Publication title",
            LatestPublishedVersionId = methodologyVersionId,
            Versions =
            [
                new MethodologyVersion
                {
                    Id = methodologyVersionId,
                    PublishingStrategy = Immediately,
                    Published = methodologyUpdatedDate,
                    Updated = methodologyUpdatedDate,
                    Status = Approved,
                },
            ],
        };

        var publication = new Publication
        {
            Slug = "publication-title",
            Title = "Publication title",
            LatestPublishedReleaseVersionId = Guid.NewGuid(),
            Methodologies = [new PublicationMethodology { Methodology = methodology, Owner = true }],
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext);
            var result = (await service.ListSitemapItems()).AssertRight();

            var item = Assert.Single(result);
            Assert.Equal(methodology.OwningPublicationSlug, item.Slug);
            Assert.Equal(methodologyUpdatedDate, item.LastModified);
        }
    }

    [Fact]
    public async Task ListSitemapItems_AlternativeSlugTakesPriority()
    {
        var methodologyVersionId = Guid.NewGuid();
        var methodologyUpdatedDate = DateTime.Parse(sitemapItemLastModifiedTime);

        var methodology = new Methodology
        {
            OwningPublicationSlug = "publication-title",
            OwningPublicationTitle = "Publication title",
            LatestPublishedVersionId = methodologyVersionId,
            Versions =
            [
                new MethodologyVersion
                {
                    Id = methodologyVersionId,
                    PublishingStrategy = Immediately,
                    Published = methodologyUpdatedDate,
                    Updated = methodologyUpdatedDate,
                    Status = Approved,
                    AlternativeSlug = "alternative-title",
                    AlternativeTitle = "Alternative title",
                },
            ],
        };

        var publication = new Publication
        {
            Slug = "publication-title",
            Title = "Publication title",
            LatestPublishedReleaseVersionId = Guid.NewGuid(),
            Methodologies = [new PublicationMethodology { Methodology = methodology, Owner = true }],
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext);
            var result = (await service.ListSitemapItems()).AssertRight();

            var item = Assert.Single(result);
            Assert.Equal("alternative-title", item.Slug);
            Assert.Equal(methodologyUpdatedDate, item.LastModified);
        }
    }

    private static MethodologyService SetupMethodologyService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IMethodologyVersionRepository? methodologyVersionRepository = null
    )
    {
        return new(
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            MapperUtils.ContentMapper(contentDbContext),
            methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(MockBehavior.Strict)
        );
    }
}
