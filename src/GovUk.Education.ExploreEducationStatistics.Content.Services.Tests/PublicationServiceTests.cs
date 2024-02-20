using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortOrder;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Requests.PublicationsSortBy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public abstract class PublicationServiceTests
    {
        private readonly string _contentDbContextId = Guid.NewGuid().ToString();

        private readonly DataFixture _dataFixture = new();

        public class GetSummaryTests : PublicationServiceTests
        {
            [Fact]
            public async Task PublicationExists_HasPublishedReleaseVersion_ReturnsPublication()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(_dataFixture
                        .DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1));

                await SeedDatabase(publication, _contentDbContextId);

                var result = await GetSummary(publication.Id, _contentDbContextId);

                var publicationViewModel = result.AssertRight();

                Assert.Equal(publication.Id, publicationViewModel.Id);
                Assert.Equal(publication.Title, publicationViewModel.Title);
                Assert.Equal(publication.Slug, publicationViewModel.Slug);
                Assert.Equal(publication.Summary, publicationViewModel.Summary);
                Assert.Equal(publication.LatestPublishedRelease!.Published!.Value, publicationViewModel.Published);
            }

            [Fact]
            public async Task PublicationExists_NoPublishedReleaseVersion_ReturnsNotFound()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(_dataFixture
                        .DefaultReleaseParent(publishedVersions: 0, draftVersion: true)
                        .Generate(1));

                await SeedDatabase(publication, _contentDbContextId);

                var result = await GetSummary(publication.Id, _contentDbContextId);

                var notFound = result.AssertLeft();

                notFound.AssertNotFoundResult();
            }

            [Fact]
            public async Task PublicationDoesNotExist_NotFound()
            {
                var result = await GetSummary(Guid.NewGuid(), _contentDbContextId);

                var notFound = result.AssertLeft();

                notFound.AssertNotFoundResult();
            }

            private static async Task SeedDatabase(Publication publication, string contentDbContextId)
            {
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    contentDbContext.Publications.Add(publication);
                    await contentDbContext.SaveChangesAsync();
                }
            }

            private static async Task<Either<ActionResult, PublishedPublicationSummaryViewModel>> GetSummary(
                Guid publicationId,
                string contentDbContextId)
            {
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    return await service.GetSummary(publicationId);
                }
            }
        }

        public class GetTests : PublicationServiceTests
        {
            private readonly Contact _contact = new()
            {
                TeamName = "Team name",
                TeamEmail = "team@email.com",
                ContactName = "Contact name",
                ContactTelNo = "1234"
            };

            private readonly ExternalMethodology _externalMethodology = new()
            {
                Title = "External methodology title",
                Url = "https://external.methodology.com",
            };

            private readonly LegacyRelease _legacyRelease = new()
            {
                Id = Guid.NewGuid(),
                Description = "Legacy release description",
                Url = "https://legacy.release.com",
                Order = 0,
            };

            [Fact]
            public async Task Success()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(ListOf<ReleaseParent>(
                        _dataFixture
                            .DefaultReleaseParent(publishedVersions: 1, year: 2020),
                        _dataFixture
                            .DefaultReleaseParent(publishedVersions: 0, draftVersion: true, year: 2021),
                        _dataFixture
                            .DefaultReleaseParent(publishedVersions: 2, draftVersion: true, year: 2022)))
                    .WithContact(_contact)
                    .WithExternalMethodology(_externalMethodology)
                    .WithLegacyReleases(ListOf(_legacyRelease))
                    .WithTopic(_dataFixture
                        .DefaultTopic()
                        .WithTheme(_dataFixture
                            .DefaultTheme()));

                // Expect the latest published release versions in reverse chronological order
                var expectedReleaseVersion1 = publication.Releases.Single(r => r is { Year: 2022, Version: 1 });
                var expectedReleaseVersion2 = publication.Releases.Single(r => r is { Year: 2020, Version: 0 });

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    contentDbContext.Publications.Add(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var result = await service.Get(publication.Slug);

                    var publicationViewModel = result.AssertRight();

                    Assert.Equal(publication.Id, publicationViewModel.Id);
                    Assert.Equal(publication.Title, publicationViewModel.Title);
                    Assert.Equal(publication.Slug, publicationViewModel.Slug);
                    Assert.False(publicationViewModel.IsSuperseded);

                    Assert.Equal(2, publicationViewModel.Releases.Count);
                    Assert.Equal(expectedReleaseVersion1.Id, publicationViewModel.LatestReleaseId);

                    Assert.Equal(expectedReleaseVersion1.Id, publicationViewModel.Releases[0].Id);
                    Assert.Equal(expectedReleaseVersion1.Slug, publicationViewModel.Releases[0].Slug);
                    Assert.Equal(expectedReleaseVersion1.Title, publicationViewModel.Releases[0].Title);
                    var releaseOrder1 = publicationViewModel.ReleaseOrders.Find(ro => ro.ReleaseId == expectedReleaseVersion1.Id)!;
                    Assert.False(releaseOrder1.IsAmendment);
                    Assert.False(releaseOrder1.IsDraft);
                    Assert.False(releaseOrder1.IsLegacy);

                    Assert.Equal(expectedReleaseVersion2.Id, publicationViewModel.Releases[1].Id);
                    Assert.Equal(expectedReleaseVersion2.Slug, publicationViewModel.Releases[1].Slug);
                    Assert.Equal(expectedReleaseVersion2.Title, publicationViewModel.Releases[1].Title);
                    var releaseOrder2 = publicationViewModel.ReleaseOrders.Find(ro => ro.ReleaseId == expectedReleaseVersion2.Id)!;
                    Assert.False(releaseOrder2.IsAmendment);
                    Assert.False(releaseOrder2.IsDraft);
                    Assert.False(releaseOrder2.IsLegacy);

                    Assert.Single(publicationViewModel.LegacyReleases);
                    Assert.Equal(_legacyRelease.Id, publicationViewModel.LegacyReleases[0].Id);
                    Assert.Equal(_legacyRelease.Description, _legacyRelease.Description);
                    Assert.Equal(_legacyRelease.Url, publicationViewModel.LegacyReleases[0].Url);
                    var releaseOrder3 = publicationViewModel.ReleaseOrders.Find(ro => ro.ReleaseId == _legacyRelease.Id)!;
                    Assert.False(releaseOrder3.IsAmendment);
                    Assert.False(releaseOrder3.IsDraft);
                    Assert.True(releaseOrder3.IsLegacy);

                    Assert.Equal(publication.Topic.Theme.Id, publicationViewModel.Topic.Theme.Id);
                    Assert.Equal(publication.Topic.Theme.Slug, publicationViewModel.Topic.Theme.Slug);
                    Assert.Equal(publication.Topic.Theme.Title, publicationViewModel.Topic.Theme.Title);
                    Assert.Equal(publication.Topic.Theme.Summary, publicationViewModel.Topic.Theme.Summary);

                    Assert.Equal(_contact.TeamName, publicationViewModel.Contact.TeamName);
                    Assert.Equal(_contact.TeamEmail, publicationViewModel.Contact.TeamEmail);
                    Assert.Equal(_contact.ContactName, publicationViewModel.Contact.ContactName);
                    Assert.Equal(_contact.ContactTelNo, publicationViewModel.Contact.ContactTelNo);

                    Assert.NotNull(publicationViewModel.ExternalMethodology);
                    Assert.Equal(_externalMethodology.Title, publicationViewModel.ExternalMethodology!.Title);
                    Assert.Equal(_externalMethodology.Url, publicationViewModel.ExternalMethodology.Url);
                }
            }

            [Fact]
            public async Task IsSuperseded_SupersedingPublicationHasPublishedRelease()
            {
                Publication supersedingPublication = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(_dataFixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1));

                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithSupersededBy(supersedingPublication)
                    .WithReleaseParents(_dataFixture
                        .DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithContact(_contact)
                    .WithExternalMethodology(_externalMethodology)
                    .WithLegacyReleases(ListOf(_legacyRelease))
                    .WithTopic(_dataFixture
                        .DefaultTopic()
                        .WithTheme(_dataFixture
                            .DefaultTheme()));

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    contentDbContext.Publications.Add(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var result = await service.Get(publication.Slug);

                    var publicationViewModel = result.AssertRight();

                    Assert.Equal(publication.Id, publicationViewModel.Id);
                    Assert.True(publicationViewModel.IsSuperseded);

                    Assert.NotNull(publicationViewModel.SupersededBy);
                    Assert.Equal(supersedingPublication.Id, publicationViewModel.SupersededBy!.Id);
                    Assert.Equal(supersedingPublication.Title, publicationViewModel.SupersededBy.Title);
                    Assert.Equal(supersedingPublication.Slug, publicationViewModel.SupersededBy.Slug);
                }
            }

            [Fact]
            public async Task IsSuperseded_SupersedingPublicationHasNoPublishedRelease()
            {
                Publication supersedingPublication = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(_dataFixture.DefaultReleaseParent(publishedVersions: 0, draftVersion: true)
                        .Generate(1));

                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithSupersededBy(supersedingPublication)
                    .WithReleaseParents(_dataFixture
                        .DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithContact(_contact)
                    .WithExternalMethodology(_externalMethodology)
                    .WithLegacyReleases(ListOf(_legacyRelease))
                    .WithTopic(_dataFixture
                        .DefaultTopic()
                        .WithTheme(_dataFixture
                            .DefaultTheme()));

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    contentDbContext.Publications.Add(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var result = await service.Get(publication.Slug);

                    var publicationViewModel = result.AssertRight();

                    Assert.Equal(publication.Id, publicationViewModel.Id);
                    Assert.False(publicationViewModel.IsSuperseded);
                    Assert.Null(publicationViewModel.SupersededBy);
                }
            }

            [Fact]
            public async Task PublicationHasNoPublishedRelease_ReturnsNotFound()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(_dataFixture
                        .DefaultReleaseParent(publishedVersions: 0, draftVersion: true)
                        .Generate(1))
                    .WithContact(_contact)
                    .WithExternalMethodology(_externalMethodology)
                    .WithLegacyReleases(ListOf(_legacyRelease))
                    .WithTopic(_dataFixture
                        .DefaultTopic()
                        .WithTheme(_dataFixture
                            .DefaultTheme()));

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    contentDbContext.Publications.Add(publication);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var result = await service.Get(publication.Slug);

                    result.AssertNotFound();
                }
            }

            [Fact]
            public async Task NoPublication_ReturnsNotFound()
            {
                var service = SetupPublicationService();

                var result = await service.Get("nonexistent-publication");

                result.AssertNotFound();
            }
        }

        public class GetPublicationTreeTests : PublicationServiceTests
        {
            [Fact]
            public async Task NoThemes()
            {
                var contextId = Guid.NewGuid().ToString();
                await using var context = InMemoryContentDbContext(contextId);
                var service = SetupPublicationService(context);

                var publicationTree = await service.GetPublicationTree();
                Assert.Empty(publicationTree);
            }

            [Fact]
            public async Task MultipleThemesTopics()
            {
                var (publication1, publication2, publication3) = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(_dataFixture
                        .DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .Generate(3)
                    .ToTuple3();

                var (theme1, theme2) = _dataFixture
                    .DefaultTheme()
                    .ForIndex(0, themeSetter => themeSetter.SetTopics(_dataFixture
                        .DefaultTopic()
                        .ForIndex(0, s => s.SetPublications(ListOf(publication1)))
                        .ForIndex(1, s => s.SetPublications(ListOf(publication2)))
                        .Generate(2)))
                    .ForIndex(1, themeSetter => themeSetter.SetTopics(_dataFixture
                        .DefaultTopic()
                        .ForIndex(0, s => s.SetPublications(ListOf(publication3)))
                        .Generate(1)))
                    .Generate(2)
                    .ToTuple2();

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Themes.AddRange(theme1, theme2);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = SetupPublicationService(context);

                    var publicationTree = await service.GetPublicationTree();

                    Assert.Equal(2, publicationTree.Count);
                    Assert.Equal(theme1.Title, publicationTree[0].Title);
                    Assert.Equal(theme1.Summary, publicationTree[0].Summary);

                    Assert.Equal(theme2.Title, publicationTree[1].Title);
                    Assert.Equal(theme2.Summary, publicationTree[1].Summary);

                    Assert.Equal(2, publicationTree[0].Topics.Count);
                    Assert.Equal(theme1.Topics[0].Title, publicationTree[0].Topics[0].Title);
                    Assert.Equal(theme1.Topics[1].Title, publicationTree[0].Topics[1].Title);

                    Assert.Single(publicationTree[1].Topics);
                    Assert.Equal(theme2.Topics[0].Title, publicationTree[1].Topics[0].Title);

                    var topicAPublications = publicationTree[0].Topics[0].Publications;

                    Assert.Single(topicAPublications);
                    Assert.Equal(publication1.Slug, topicAPublications[0].Slug);
                    Assert.Equal(publication1.Title, topicAPublications[0].Title);
                    Assert.False(topicAPublications[0].LatestReleaseHasData);
                    Assert.False(topicAPublications[0].AnyLiveReleaseHasData);

                    var topicBPublications = publicationTree[0].Topics[1].Publications;

                    Assert.Single(topicBPublications);
                    Assert.Equal(publication2.Slug, topicBPublications[0].Slug);
                    Assert.Equal(publication2.Title, topicBPublications[0].Title);
                    Assert.False(topicBPublications[0].LatestReleaseHasData);
                    Assert.False(topicBPublications[0].AnyLiveReleaseHasData);

                    var topicCPublications = publicationTree[1].Topics[0].Publications;

                    Assert.Single(topicCPublications);
                    Assert.Equal(publication3.Slug, topicCPublications[0].Slug);
                    Assert.Equal(publication3.Title, topicCPublications[0].Title);
                    Assert.False(topicCPublications[0].LatestReleaseHasData);
                    Assert.False(topicCPublications[0].AnyLiveReleaseHasData);
                }
            }

            [Fact]
            public async Task ThemesWithNoTopicsOrPublications_Excluded()
            {
                var (theme1, theme2, theme3) = _dataFixture
                    .DefaultTheme()
                    // Index 0 has no topics,
                    // Index 1 has a topic with a publication,
                    // Index 2 has a topic with no publications
                    .ForIndex(1, s => s.SetTopics(_dataFixture
                        .DefaultTopic()
                        .WithPublications(_dataFixture
                            .DefaultPublication()
                            .WithReleaseParents(_dataFixture
                                .DefaultReleaseParent(publishedVersions: 1)
                                .Generate(1))
                            .Generate(1))
                        .Generate(1)))
                    .ForIndex(2, s => s.SetTopics(_dataFixture
                        .DefaultTopic()
                        .Generate(1)))
                    .Generate(3)
                    .ToTuple3();

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Themes.AddRange(theme1, theme2, theme3);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = SetupPublicationService(context);

                    var publicationTree = await service.GetPublicationTree();

                    Assert.Single(publicationTree);
                    Assert.Equal(theme2.Title, publicationTree[0].Title);

                    Assert.Single(publicationTree[0].Topics);
                    Assert.Equal(theme2.Topics[0].Title, publicationTree[0].Topics[0].Title);

                    var publications = publicationTree[0].Topics[0].Publications;

                    Assert.Single(publications);
                    Assert.Equal(theme2.Topics[0].Publications[0].Title, publications[0].Title);
                }
            }

            [Fact]
            public async Task ThemesWithNoVisiblePublications_Excluded()
            {
                var themes = _dataFixture
                    .DefaultTheme()
                    // Index 0 has a publication with a published release,
                    // Index 1 has a publication with a published and unpublished release
                    // Index 2 has a publication with no releases
                    // Index 3 has a publication with an unpublished release
                    .ForIndex(0, s => s.SetTopics(_dataFixture
                        .DefaultTopic()
                        .WithPublications(_dataFixture
                            .DefaultPublication()
                            .WithReleaseParents(_dataFixture
                                .DefaultReleaseParent(publishedVersions: 1)
                                .Generate(1))
                            .Generate(1))
                        .Generate(1)))
                    .ForIndex(1, s => s.SetTopics(_dataFixture
                        .DefaultTopic()
                        .WithPublications(_dataFixture
                            .DefaultPublication()
                            .WithReleaseParents(ListOf<ReleaseParent>(
                                _dataFixture
                                    .DefaultReleaseParent(publishedVersions: 1),
                                _dataFixture
                                    .DefaultReleaseParent(publishedVersions: 0, draftVersion: true)
                            ))
                            .Generate(1))
                        .Generate(1)))
                    .ForIndex(2, s => s.SetTopics(_dataFixture
                        .DefaultTopic()
                        .WithPublications(_dataFixture
                            .DefaultPublication()
                            .Generate(1))
                        .Generate(1)))
                    .ForIndex(3, s => s.SetTopics(_dataFixture
                        .DefaultTopic()
                        .WithPublications(_dataFixture
                            .DefaultPublication()
                            .WithReleaseParents(_dataFixture
                                .DefaultReleaseParent(publishedVersions: 0, draftVersion: true)
                                .Generate(1))
                            .Generate(1))
                        .Generate(1)))
                    .GenerateList(4);

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Themes.AddRange(themes);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = SetupPublicationService(context);

                    var publicationTree = await service.GetPublicationTree();

                    // Expect only the first two themes to be included

                    Assert.Equal(2, publicationTree.Count);
                    Assert.Equal(themes[0].Title, publicationTree[0].Title);
                    Assert.Equal(themes[1].Title, publicationTree[1].Title);

                    Assert.Single(publicationTree[0].Topics);
                    Assert.Equal(themes[0].Topics[0].Title, publicationTree[0].Topics[0].Title);

                    Assert.Single(publicationTree[0].Topics[0].Publications);
                    Assert.Equal(themes[0].Topics[0].Publications[0].Title,
                        publicationTree[0].Topics[0].Publications[0].Title);

                    Assert.Single(publicationTree[1].Topics);
                    Assert.Equal(themes[1].Topics[0].Title, publicationTree[1].Topics[0].Title);

                    Assert.Single(publicationTree[1].Topics[0].Publications);
                    Assert.Equal(themes[1].Topics[0].Publications[0].Title,
                        publicationTree[1].Topics[0].Publications[0].Title);
                }
            }

            [Fact]
            public async Task PublicationsWithNoReleases_Excluded()
            {
                Theme theme = _dataFixture
                    .DefaultTheme()
                    .WithTopics(_dataFixture
                        .DefaultTopic()
                        .WithPublications(_dataFixture
                            .DefaultPublication()
                            // Index 0 has a published release
                            // Index 1 has no releases
                            .ForIndex(0, s => s.SetReleaseParents(_dataFixture
                                .DefaultReleaseParent(publishedVersions: 1)
                                .Generate(1)))
                            .Generate(2))
                        .Generate(1));

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Themes.Add(theme);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = SetupPublicationService(context);

                    var publicationTree = await service.GetPublicationTree();

                    var publications = publicationTree[0].Topics[0].Publications;

                    Assert.Single(publications);
                    Assert.Equal(theme.Topics[0].Publications[0].Title, publications[0].Title);
                }
            }

            [Fact]
            public async Task TopicsWithNoVisiblePublications_Excluded()
            {
                Theme theme = _dataFixture
                    .DefaultTheme()
                    .WithTopics(_dataFixture
                        .DefaultTopic()
                        // Index 0 has a publication with published release
                        // Index 1 has a publication with an unpublished release
                        .ForIndex(0, s => s.SetPublications(_dataFixture
                            .DefaultPublication()
                            .WithReleaseParents(_dataFixture
                                .DefaultReleaseParent(publishedVersions: 1)
                                .Generate(1))
                            .Generate(1)))
                        .ForIndex(1, s => s.SetPublications(_dataFixture
                            .DefaultPublication()
                            .WithReleaseParents(_dataFixture
                                .DefaultReleaseParent(publishedVersions: 0, draftVersion: true)
                                .Generate(1))
                            .Generate(1)))
                        .Generate(2));

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Themes.Add(theme);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = SetupPublicationService(context);

                    var publicationTree = await service.GetPublicationTree();

                    // Expect only the first topic to be included

                    Assert.Single(publicationTree);
                    Assert.Equal(theme.Title, publicationTree[0].Title);

                    Assert.Single(publicationTree[0].Topics);
                    Assert.Equal(theme.Topics[0].Title, publicationTree[0].Topics[0].Title);

                    Assert.Single(publicationTree[0].Topics[0].Publications);
                    Assert.Equal(theme.Topics[0].Publications[0].Title,
                        publicationTree[0].Topics[0].Publications[0].Title);
                }
            }

            [Fact]
            public async Task LatestReleaseHasData()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(_dataFixture
                        .DefaultReleaseParent(publishedVersions: 1)
                        .Generate(2));

                Theme theme = _dataFixture
                    .DefaultTheme()
                    .WithTopics(_dataFixture
                        .DefaultTopic()
                        .WithPublications(ListOf(publication))
                        .Generate(1));

                var releaseFiles = new List<ReleaseFile>
                {
                    // Latest release has no data
                    new()
                    {
                        Release = publication.Releases[1],
                        File = new File
                        {
                            Type = FileType.Data
                        }
                    },
                    // Older release has no data
                    new()
                    {
                        Release = publication.Releases[0],
                        File = new File
                        {
                            Type = FileType.Ancillary
                        }
                    }
                };

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Themes.Add(theme);
                    context.ReleaseFiles.AddRange(releaseFiles);

                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = SetupPublicationService(context);

                    var publicationTree = await service.GetPublicationTree();

                    Assert.Single(publicationTree);
                    Assert.Equal(theme.Title, publicationTree[0].Title);

                    Assert.Single(publicationTree[0].Topics);
                    Assert.Equal(theme.Topics[0].Title, publicationTree[0].Topics[0].Title);

                    var publications = publicationTree[0].Topics[0].Publications;

                    var resultPublication = Assert.Single(publications);
                    Assert.Equal(publication.Title, resultPublication.Title);
                    Assert.Equal(publication.Id, resultPublication.Id);
                    Assert.True(resultPublication.LatestReleaseHasData);
                    Assert.True(resultPublication.AnyLiveReleaseHasData);
                }
            }

            [Fact]
            public async Task PreviousReleaseHasData()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(_dataFixture
                        .DefaultReleaseParent(publishedVersions: 1)
                        .Generate(2));

                Theme theme = _dataFixture
                    .DefaultTheme()
                    .WithTopics(_dataFixture
                        .DefaultTopic()
                        .WithPublications(ListOf(publication))
                        .Generate(1));

                var releaseFiles = new List<ReleaseFile>
                {
                    // Latest release has no data
                    new()
                    {
                        Release = publication.Releases[1],
                        File = new File
                        {
                            Type = FileType.Ancillary
                        }
                    },
                    // Older release has data, so the publication is visible
                    new()
                    {
                        Release = publication.Releases[0],
                        File = new File
                        {
                            Type = FileType.Data
                        }
                    },
                };

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Themes.Add(theme);
                    context.ReleaseFiles.AddRange(releaseFiles);

                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = SetupPublicationService(context);

                    var publicationTree = await service.GetPublicationTree();

                    Assert.Single(publicationTree);
                    Assert.Equal(theme.Title, publicationTree[0].Title);

                    Assert.Single(publicationTree[0].Topics);
                    Assert.Equal(theme.Topics[0].Title, publicationTree[0].Topics[0].Title);

                    var publications = publicationTree[0].Topics[0].Publications;

                    var resultPublication = Assert.Single(publications);
                    Assert.Equal(publication.Title, resultPublication.Title);
                    Assert.Equal(publication.Id, resultPublication.Id);
                    Assert.False(resultPublication.LatestReleaseHasData);
                    Assert.True(resultPublication.AnyLiveReleaseHasData);
                }
            }

            [Fact]
            public async Task UnpublishedReleaseHasData()
            {
                Publication publication = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(ListOf<ReleaseParent>(
                        _dataFixture.DefaultReleaseParent(publishedVersions: 0, draftVersion: true),
                        _dataFixture.DefaultReleaseParent(publishedVersions: 1)));

                Theme theme = _dataFixture
                    .DefaultTheme()
                    .WithTopics(_dataFixture
                        .DefaultTopic()
                        .WithPublications(ListOf(publication))
                        .Generate(1));

                var releaseFiles = new List<ReleaseFile>
                {
                    // Latest release has no data
                    new()
                    {
                        Release = publication.Releases[1],
                        File = new File
                        {
                            Type = FileType.Ancillary
                        }
                    },
                    // Unpublished release has data but this shouldn't alter anything
                    new()
                    {
                        Release = publication.Releases[0],
                        File = new File
                        {
                            Type = FileType.Data
                        }
                    },
                };

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Themes.Add(theme);
                    context.ReleaseFiles.AddRange(releaseFiles);

                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = SetupPublicationService(context);

                    var publicationTree = await service.GetPublicationTree();

                    Assert.Single(publicationTree);
                    Assert.Equal(theme.Id, publicationTree[0].Id);

                    var topic = Assert.Single(publicationTree[0].Topics);
                    Assert.Equal(theme.Topics[0].Id, topic.Id);

                    var resultPublication = Assert.Single(topic.Publications);
                    Assert.Equal(publication.Id, resultPublication.Id);
                    Assert.False(resultPublication.LatestReleaseHasData);
                    Assert.False(resultPublication.AnyLiveReleaseHasData);
                }
            }

            [Fact]
            public async Task Superseded()
            {
                Publication supersedingPublication = _dataFixture
                    .DefaultPublication()
                    .WithReleaseParents(_dataFixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1));

                var (publication1, publication2) = _dataFixture
                    .DefaultPublication()
                    // Both publications have published releases
                    // Index 1 is superseded
                    .WithReleaseParents(_dataFixture
                        .DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .ForIndex(1, s => s.SetSupersededBy(supersedingPublication))
                    .Generate(2)
                    .ToTuple2();

                Theme theme = _dataFixture
                    .DefaultTheme()
                    .WithTopics(_dataFixture
                        .DefaultTopic()
                        // Publications are in random order
                        // to check that ordering is done by title
                        .WithPublications(ListOf(publication2, publication1))
                        .Generate(1));

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Themes.Add(theme);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = SetupPublicationService(context);

                    var publicationTree = await service.GetPublicationTree();

                    Assert.Single(publicationTree);
                    Assert.Equal(theme.Title, publicationTree[0].Title);

                    Assert.Single(publicationTree[0].Topics);
                    Assert.Equal(theme.Topics[0].Title, publicationTree[0].Topics[0].Title);

                    var publications = publicationTree[0].Topics[0].Publications;
                    Assert.Equal(2, publications.Count);

                    Assert.Equal(publication1.Title, publications[0].Title);
                    Assert.False(publications[0].IsSuperseded);

                    Assert.Equal(publication2.Title, publications[1].Title);
                    Assert.True(publications[1].IsSuperseded);

                    Assert.NotNull(publications[1].SupersededBy);
                    Assert.Equal(supersedingPublication.Id, publications[1].SupersededBy!.Id);
                    Assert.Equal(supersedingPublication.Title, publications[1].SupersededBy!.Title);
                    Assert.Equal(supersedingPublication.Slug, publications[1].SupersededBy!.Slug);
                }
            }
        }

        public class ListPublicationsTests : PublicationServiceTests
        {
            [Fact]
            public async Task Success()
            {
                var publicationA = new Publication
                {
                    Slug = "publication-a",
                    Title = "Publication A",
                    Summary = "Publication A summary",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = new DateTime(2020, 1, 1)
                    }
                };

                var publicationB = new Publication
                {
                    Slug = "publication-b",
                    Title = "Publication B",
                    Summary = "Publication B summary",
                    LatestPublishedRelease = new Release
                    {
                        Type = OfficialStatistics,
                        Published = new DateTime(2021, 1, 1)
                    }
                };

                var publicationC = new Publication
                {
                    Slug = "publication-c",
                    Title = "Publication C",
                    Summary = "Publication C summary",
                    LatestPublishedRelease = new Release
                    {
                        Type = AdHocStatistics,
                        Published = new DateTime(2022, 1, 1)
                    }
                };

                var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    },
                },
                new()
                {
                    Title = "Theme 2 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationC
                            }
                        }
                    },
                }
            };

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    await contentDbContext.Themes.AddRangeAsync(themes);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var pagedResult = (await service.ListPublications()).AssertRight();
                    var results = pagedResult.Results;

                    Assert.Equal(3, results.Count);

                    // Expect results sorted by title in ascending order

                    Assert.Equal(publicationA.Id, results[0].Id);
                    Assert.Equal("publication-a", results[0].Slug);
                    Assert.Equal(new DateTime(2020, 1, 1), results[0].Published);
                    Assert.Equal("Publication A", results[0].Title);
                    Assert.Equal("Publication A summary", results[0].Summary);
                    Assert.Equal("Theme 1 title", results[0].Theme);
                    Assert.Equal(NationalStatistics, results[0].Type);

                    Assert.Equal(publicationB.Id, results[1].Id);
                    Assert.Equal("publication-b", results[1].Slug);
                    Assert.Equal(new DateTime(2021, 1, 1), results[1].Published);
                    Assert.Equal("Publication B", results[1].Title);
                    Assert.Equal("Publication B summary", results[1].Summary);
                    Assert.Equal("Theme 1 title", results[1].Theme);
                    Assert.Equal(OfficialStatistics, results[1].Type);

                    Assert.Equal(publicationC.Id, results[2].Id);
                    Assert.Equal("publication-c", results[2].Slug);
                    Assert.Equal(new DateTime(2022, 1, 1), results[2].Published);
                    Assert.Equal("Publication C", results[2].Title);
                    Assert.Equal("Publication C summary", results[2].Summary);
                    Assert.Equal("Theme 2 title", results[2].Theme);
                    Assert.Equal(AdHocStatistics, results[2].Type);
                }
            }

            [Fact]
            public async Task ExcludesUnpublishedPublications()
            {
                // Published
                var publicationA = new Publication
                {
                    Title = "Publication A",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                // Not published (no published release)
                var publicationB = new Publication
                {
                    Title = "Publication B",
                    LatestPublishedRelease = null
                };

                var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    }
                }
            };

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    await contentDbContext.Themes.AddRangeAsync(themes);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var pagedResult = (await service.ListPublications()).AssertRight();
                    var results = pagedResult.Results;

                    Assert.Equal(publicationA.Id, results[0].Id);
                }
            }

            [Fact]
            public async Task ExcludesSupersededPublications()
            {
                // Published
                var publicationA = new Publication
                {
                    Title = "Publication A",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                // Published
                var publicationB = new Publication
                {
                    Title = "Publication B",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                // Not published (no published release)
                var publicationC = new Publication
                {
                    Title = "Publication C",
                    LatestPublishedReleaseId = null
                };

                // Not published (superseded by publicationB which is published)
                var publicationD = new Publication
                {
                    Title = "Publication D",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    },
                    SupersededBy = publicationB
                };

                // Published (superseded by publicationC but it's not published yet)
                var publicationE = new Publication
                {
                    Title = "Publication E",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    },
                    SupersededBy = publicationC
                };

                var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB,
                                publicationC,
                                publicationD,
                                publicationE
                            }
                        }
                    }
                }
            };

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    await contentDbContext.Themes.AddRangeAsync(themes);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var pagedResult = (await service.ListPublications()).AssertRight();
                    var results = pagedResult.Results;

                    Assert.Equal(publicationA.Id, results[0].Id);
                    Assert.Equal(publicationB.Id, results[1].Id);
                    Assert.Equal(publicationE.Id, results[2].Id);
                }
            }

            [Fact]
            public async Task FilterByTheme()
            {
                var publicationA = new Publication
                {
                    Title = "Publication A",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var publicationB = new Publication
                {
                    Title = "Publication B",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    },
                },
                new()
                {
                    Title = "Theme 2 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                new()
                                {
                                    Title = "Publication C",
                                    LatestPublishedRelease = new Release
                                    {
                                        Type = AdHocStatistics,
                                        Published = new DateTime(2022, 1, 1)
                                    }
                                }
                            }
                        }
                    },
                }
            };

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    await contentDbContext.Themes.AddRangeAsync(themes);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var pagedResult = (await service.ListPublications(
                        themeId: themes[0].Id
                    )).AssertRight();
                    var results = pagedResult.Results;

                    Assert.Equal(2, results.Count);

                    Assert.Equal(publicationA.Id, results[0].Id);
                    Assert.Equal("Theme 1 title", results[0].Theme);

                    Assert.Equal(publicationB.Id, results[1].Id);
                    Assert.Equal("Theme 1 title", results[1].Theme);
                }
            }

            [Fact]
            public async Task FilterByPublicationIds()
            {
                var publicationA = new Publication
                {
                    Title = "Publication A",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var publicationB = new Publication
                {
                    Title = "Publication B",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var publicationC = new Publication
                {
                    Title = "Publication C",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var publicationD = new Publication
                {
                    Title = "Publication D",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB,
                                publicationC,
                                publicationD
                            }
                        }
                    },
                }
            };

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    await contentDbContext.Themes.AddRangeAsync(themes);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var requestedPublicationIds = ListOf(publicationA.Id, publicationB.Id);

                    var pagedResult = (await service.ListPublications(
                        publicationIds: requestedPublicationIds
                    )).AssertRight();
                    var results = pagedResult.Results;

                    Assert.Equal(2, results.Count);

                    Assert.Equal(publicationA.Id, results[0].Id);

                    Assert.Equal(publicationB.Id, results[1].Id);
                }
            }

            [Fact]
            public async Task FilterByReleaseType()
            {
                var publicationA = new Publication
                {
                    Title = "Publication A",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var publicationB = new Publication
                {
                    Title = "Publication B",
                    LatestPublishedRelease = new Release
                    {
                        Type = OfficialStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var publicationC = new Publication
                {
                    Title = "Publication C",
                    LatestPublishedRelease = new Release
                    {
                        Type = AdHocStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    },
                },
                new()
                {
                    Title = "Theme 2 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationC
                            }
                        }
                    },
                }
            };

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    await contentDbContext.Themes.AddRangeAsync(themes);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var pagedResult = (await service.ListPublications(
                        releaseType: OfficialStatistics
                    )).AssertRight();
                    var results = pagedResult.Results;

                    Assert.Single(results);

                    Assert.Equal(publicationB.Id, results[0].Id);
                    Assert.Equal(OfficialStatistics, results[0].Type);
                }
            }

            [Fact]
            public async Task Search_SortByRelevance_Desc()
            {
                var releaseA = new Release
                {
                    Id = Guid.NewGuid(),
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                };

                var releaseB = new Release
                {
                    Id = Guid.NewGuid(),
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                };

                var releaseC = new Release
                {
                    Id = Guid.NewGuid(),
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                };

                var topic = new Topic
                {
                    Theme = new Theme
                    {
                        Title = "Theme title"
                    }
                };

                var publications = new List<Publication>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    LatestPublishedReleaseId = releaseB.Id,
                    LatestPublishedRelease = releaseB,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication C",
                    LatestPublishedReleaseId = releaseC.Id,
                    LatestPublishedRelease = releaseC,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    LatestPublishedReleaseId = releaseA.Id,
                    LatestPublishedRelease = releaseA,
                    Topic = topic
                },
            };

                var freeTextRanks = new List<FreeTextRank>
            {
                new(publications[1].Id, 100),
                new(publications[2].Id, 300),
                new(publications[0].Id, 200)
            };

                var contentDbContext = new Mock<ContentDbContext>();
                contentDbContext.Setup(context => context.Publications)
                    .Returns(publications.AsQueryable().BuildMockDbSet().Object);
                contentDbContext.Setup(context => context.PublicationsFreeTextTable("term"))
                    .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

                var service = SetupPublicationService(contentDbContext.Object);

                var pagedResult = (await service.ListPublications(
                    search: "term",
                    sort: null, // Sort should default to relevance
                    order: null // Order should default to descending
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by relevance in descending order

                Assert.Equal(publications[2].Id, results[0].Id);
                Assert.Equal(300, results[0].Rank);

                Assert.Equal(publications[0].Id, results[1].Id);
                Assert.Equal(200, results[1].Rank);

                Assert.Equal(publications[1].Id, results[2].Id);
                Assert.Equal(100, results[2].Rank);
            }

            [Fact]
            public async Task Search_SortByRelevance_Asc()
            {
                var releaseA = new Release
                {
                    Id = Guid.NewGuid(),
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                };

                var releaseB = new Release
                {
                    Id = Guid.NewGuid(),
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                };

                var releaseC = new Release
                {
                    Id = Guid.NewGuid(),
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                };

                var topic = new Topic
                {
                    Theme = new Theme
                    {
                        Title = "Theme title"
                    }
                };

                var publications = new List<Publication>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    LatestPublishedReleaseId = releaseB.Id,
                    LatestPublishedRelease = releaseB,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication C",
                    LatestPublishedReleaseId = releaseC.Id,
                    LatestPublishedRelease = releaseC,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    LatestPublishedReleaseId = releaseA.Id,
                    LatestPublishedRelease = releaseA,
                    Topic = topic
                },
            };

                var freeTextRanks = new List<FreeTextRank>
            {
                new(publications[1].Id, 100),
                new(publications[2].Id, 300),
                new(publications[0].Id, 200)
            };

                var contentDbContext = new Mock<ContentDbContext>();
                contentDbContext.Setup(context => context.Publications)
                    .Returns(publications.AsQueryable().BuildMockDbSet().Object);
                contentDbContext.Setup(context => context.PublicationsFreeTextTable("term"))
                    .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

                var service = SetupPublicationService(contentDbContext.Object);

                var pagedResult = (await service.ListPublications(
                    search: "term",
                    sort: null, // Sort should default to relevance
                    order: Asc
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by relevance in ascending order

                Assert.Equal(publications[1].Id, results[0].Id);
                Assert.Equal(100, results[0].Rank);

                Assert.Equal(publications[0].Id, results[1].Id);
                Assert.Equal(200, results[1].Rank);

                Assert.Equal(publications[2].Id, results[2].Id);
                Assert.Equal(300, results[2].Rank);
            }

            [Fact]
            public async Task SortByPublished_Desc()
            {
                var releaseA = new Release
                {
                    Type = NationalStatistics,
                    Published = new DateTime(2020, 1, 1)
                };

                var releaseB = new Release
                {
                    Type = NationalStatistics,
                    Published = new DateTime(2021, 1, 1)
                };

                var releaseC = new Release
                {
                    Type = NationalStatistics,
                    Published = new DateTime(2022, 1, 1)
                };

                var publicationA = new Publication
                {
                    Title = "Publication A",
                    LatestPublishedRelease = releaseA
                };

                var publicationB = new Publication
                {
                    Title = "Publication B",
                    LatestPublishedRelease = releaseB
                };

                var publicationC = new Publication
                {
                    Title = "Publication C",
                    LatestPublishedRelease = releaseC
                };

                var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    await contentDbContext.Themes.AddRangeAsync(themes);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var pagedResult = (await service.ListPublications(
                        sort: Published,
                        order: null // Order should default to descending
                    )).AssertRight();
                    var results = pagedResult.Results;

                    Assert.Equal(3, results.Count);

                    // Expect results sorted by published date in descending order

                    Assert.Equal(publicationC.Id, results[0].Id);
                    Assert.Equal(new DateTime(2022, 1, 1), results[0].Published);

                    Assert.Equal(publicationB.Id, results[1].Id);
                    Assert.Equal(new DateTime(2021, 1, 1), results[1].Published);

                    Assert.Equal(publicationA.Id, results[2].Id);
                    Assert.Equal(new DateTime(2020, 1, 1), results[2].Published);
                }
            }

            [Fact]
            public async Task SortByPublished_Asc()
            {
                var releaseA = new Release
                {
                    Type = NationalStatistics,
                    Published = new DateTime(2020, 1, 1)
                };

                var releaseB = new Release
                {
                    Type = NationalStatistics,
                    Published = new DateTime(2021, 1, 1)
                };

                var releaseC = new Release
                {
                    Type = NationalStatistics,
                    Published = new DateTime(2022, 1, 1)
                };

                var publicationA = new Publication
                {
                    Title = "Publication A",
                    LatestPublishedRelease = releaseA
                };

                var publicationB = new Publication
                {
                    Title = "Publication B",
                    LatestPublishedRelease = releaseB
                };

                var publicationC = new Publication
                {
                    Title = "Publication C",
                    LatestPublishedRelease = releaseC
                };

                var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    await contentDbContext.Themes.AddRangeAsync(themes);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var pagedResult = (await service.ListPublications(
                        sort: Published,
                        order: Asc
                    )).AssertRight();
                    var results = pagedResult.Results;

                    Assert.Equal(3, results.Count);

                    // Expect results sorted by published date in ascending order

                    Assert.Equal(publicationA.Id, results[0].Id);
                    Assert.Equal(new DateTime(2020, 1, 1), results[0].Published);

                    Assert.Equal(publicationB.Id, results[1].Id);
                    Assert.Equal(new DateTime(2021, 1, 1), results[1].Published);

                    Assert.Equal(publicationC.Id, results[2].Id);
                    Assert.Equal(new DateTime(2022, 1, 1), results[2].Published);
                }
            }

            [Fact]
            public async Task SortByTitle_Desc()
            {
                var publicationA = new Publication
                {
                    Title = "Publication A",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var publicationB = new Publication
                {
                    Title = "Publication B",
                    LatestPublishedRelease = new Release
                    {
                        Type = OfficialStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var publicationC = new Publication
                {
                    Title = "Publication C",
                    LatestPublishedRelease = new Release
                    {
                        Type = AdHocStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    await contentDbContext.Themes.AddRangeAsync(themes);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var pagedResult = (await service.ListPublications(
                        sort: null, // Sort should default to title
                        order: Desc
                    )).AssertRight();
                    var results = pagedResult.Results;

                    Assert.Equal(3, results.Count);

                    // Expect results sorted by title in descending order

                    Assert.Equal(publicationC.Id, results[0].Id);
                    Assert.Equal("Publication C", results[0].Title);

                    Assert.Equal(publicationB.Id, results[1].Id);
                    Assert.Equal("Publication B", results[1].Title);

                    Assert.Equal(publicationA.Id, results[2].Id);
                    Assert.Equal("Publication A", results[2].Title);
                }
            }

            [Fact]
            public async Task SortByTitle_Asc()
            {
                var publicationA = new Publication
                {
                    Title = "Publication A",
                    LatestPublishedRelease = new Release
                    {
                        Type = NationalStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var publicationB = new Publication
                {
                    Title = "Publication B",
                    LatestPublishedRelease = new Release
                    {
                        Type = OfficialStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var publicationC = new Publication
                {
                    Title = "Publication C",
                    LatestPublishedRelease = new Release
                    {
                        Type = AdHocStatistics,
                        Published = DateTime.UtcNow
                    }
                };

                var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    await contentDbContext.Themes.AddRangeAsync(themes);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
                {
                    var service = SetupPublicationService(contentDbContext);

                    var pagedResult = (await service.ListPublications(
                        sort: null, // Sort should default to title
                        order: null // Order should default to ascending
                    )).AssertRight();
                    var results = pagedResult.Results;

                    Assert.Equal(3, results.Count);

                    // Expect results sorted by title in ascending order

                    Assert.Equal(publicationA.Id, results[0].Id);
                    Assert.Equal("Publication A", results[0].Title);

                    Assert.Equal(publicationB.Id, results[1].Id);
                    Assert.Equal("Publication B", results[1].Title);

                    Assert.Equal(publicationC.Id, results[2].Id);
                    Assert.Equal("Publication C", results[2].Title);
                }
            }
        }

        private static PublicationService SetupPublicationService(
            ContentDbContext? contentDbContext = null,
            IPublicationRepository? publicationRepository = null,
            IReleaseRepository? releaseRepository = null)
        {
            contentDbContext ??= InMemoryContentDbContext();

            return new(
                contentDbContext,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                publicationRepository ?? new PublicationRepository(contentDbContext),
                releaseRepository ?? new ReleaseRepository(contentDbContext)
            );
        }
    }
}
