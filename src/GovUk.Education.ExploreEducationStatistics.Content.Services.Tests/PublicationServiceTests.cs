#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Mappings;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class PublicationServiceTests
    {
        [Fact]
        public async Task Get()
        {
            var release2000Version0Id = Guid.NewGuid();
            var publication = new Publication
            {
                Title = "Publication Title",
                Slug = "publication-slug",
                Releases = new List<Release>
                {
                    new Release // latest published release
                    {
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 1,
                        PreviousVersionId = release2000Version0Id,
                    },
                    new Release
                    {
                        Id = release2000Version0Id,
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 0,
                    },
                    new Release // not published
                    {
                        ReleaseName = "2001",
                        Slug = "2001",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = null,
                        Version = 0,
                    },
                    new Release // published so appears in ListReleases result
                    {
                        ReleaseName = "1999",
                        Slug = "1999",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 0,
                    },
                },
                LegacyReleases = new List<LegacyRelease>
                {
                    new LegacyRelease
                    {
                        Description = "Legacy release description",
                        Url = "http://legacy.release.com",
                        Order = 0,
                    }
                },
                Topic = new Topic
                {
                    Title = "Test topic",
                    Slug = "test-topic",
                    Theme = new Theme
                    {
                        Title = "Test theme",
                        Slug = "test-theme",
                    }
                },
                Contact = new Contact
                {
                    TeamName = "Team name",
                    TeamEmail = "team@email.com",
                    ContactName = "Contact name",
                    ContactTelNo = "1234",
                },
                ExternalMethodology = new ExternalMethodology
                {
                    Title = "External methodology title",
                    Url = "http://external.methodology.com",
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
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

                Assert.Equal(2, publicationViewModel.Releases.Count);
                Assert.Equal(publication.Releases[0].Id, publicationViewModel.LatestReleaseId);

                Assert.Equal(publication.Releases[0].Id, publicationViewModel.Releases[0].Id);
                Assert.Equal(publication.Releases[0].Slug, publicationViewModel.Releases[0].Slug);
                Assert.Equal(publication.Releases[0].Title, publicationViewModel.Releases[0].Title);

                Assert.Equal(publication.Releases[3].Id, publicationViewModel.Releases[1].Id);
                Assert.Equal(publication.Releases[3].Slug, publicationViewModel.Releases[1].Slug);
                Assert.Equal(publication.Releases[3].Title, publicationViewModel.Releases[1].Title);

                Assert.Single(publication.LegacyReleases);
                Assert.Equal(publication.LegacyReleases[0].Id, publicationViewModel.LegacyReleases[0].Id);
                Assert.Equal(publication.LegacyReleases[0].Description,
                    publicationViewModel.LegacyReleases[0].Description);
                Assert.Equal(publication.LegacyReleases[0].Url, publicationViewModel.LegacyReleases[0].Url);

                Assert.Equal(publication.Topic.Theme.Title, publicationViewModel.Topic.Theme.Title);

                Assert.Equal(publication.Contact.TeamName, publicationViewModel.Contact.TeamName);
                Assert.Equal(publication.Contact.TeamEmail, publicationViewModel.Contact.TeamEmail);
                Assert.Equal(publication.Contact.ContactName, publicationViewModel.Contact.ContactName);
                Assert.Equal(publication.Contact.ContactTelNo, publicationViewModel.Contact.ContactTelNo);

                Assert.Equal(publication.ExternalMethodology.Title, publicationViewModel.ExternalMethodology.Title);
                Assert.Equal(publication.ExternalMethodology.Url, publicationViewModel.ExternalMethodology.Url);
            }
        }

        [Fact]
        public async Task Get_NoPublication()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get("nonexistant-publication");

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Get_PublicationHasNoLiveLatestRelease()
        {
            const string publicationSlug = "publication-slug";

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(new Publication
                {
                    Slug = publicationSlug,
                    Releases = new List<Release>
                    {
                        new Release // not published
                        {
                            ReleaseName = "2000",
                            Slug = "2000",
                            TimePeriodCoverage = TimeIdentifier.AcademicYear,
                            Published = null,
                        },
                    }
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publicationSlug);

                result.AssertNotFound();
            }
        }

        private static PublicationService SetupPublicationService(
            ContentDbContext? contentDbContext = null,
            IMapper? mapper = null)
        {
            return new(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                contentDbContext is null
                    ? Mock.Of<IPersistenceHelper<ContentDbContext>>()
                    : new PersistenceHelper<ContentDbContext>(contentDbContext),
                mapper ?? MapperUtils.MapperForProfile<MappingProfiles>()
            );
        }
    }
}
