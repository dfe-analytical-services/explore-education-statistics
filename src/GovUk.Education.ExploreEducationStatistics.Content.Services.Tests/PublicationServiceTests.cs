#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    [Collection(CacheServiceTests)]
    public class PublicationServiceTests : CacheServiceTestFixture
    {
        private const string PublicationSlug = "publication-slug";

        private static readonly PublicationViewModel PublicationViewModel = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task GetCachedPublication_PublicationIsCached()
        {
            BlobCacheAttribute.AddService("public", BlobCacheService.Object);

            var cacheKey = new PublicationCacheKey(PublicationSlug);

            BlobCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(PublicationViewModel)))
                .ReturnsAsync(PublicationViewModel);

            var service = SetupPublicationService();

            var result = await service.GetCachedPublication(PublicationSlug);

            VerifyAllMocks(BlobCacheService);

            result.AssertRight(PublicationViewModel);
        }

        [Fact]
        public async Task GetCachedPublication_PublicationIsNotCached()
        {
            BlobCacheAttribute.AddService("public", BlobCacheService.Object);

            var cacheKey = new PublicationCacheKey(PublicationSlug);

            BlobCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(PublicationViewModel)))
                .ReturnsAsync(null);

            // Value returned by stub of the un-cached method should be set in the cache
            BlobCacheService
                .Setup(s => s.SetItem<object>(cacheKey, PublicationViewModel))
                .Returns(Task.CompletedTask);

            // Setup service with a stub of the un-cached method to return the resource
            var service = SetupPublicationServiceStub();

            var result = await service.GetCachedPublication(PublicationSlug);

            VerifyAllMocks(BlobCacheService);

            result.AssertRight(PublicationViewModel);
        }

        [Fact]
        public async Task UpdatePublication()
        {
            BlobCacheAttribute.AddService("public", BlobCacheService.Object);

            var cacheKey = new PublicationCacheKey(PublicationSlug);

            // Value returned by stub of the un-cached method should be set in the cache
            BlobCacheService
                .Setup(s => s.SetItem<object>(cacheKey, PublicationViewModel))
                .Returns(Task.CompletedTask);

            // Setup service with a stub of the un-cached method to return the resource
            var service = SetupPublicationServiceStub();

            var result = await service.UpdateCachedPublication(PublicationSlug);

            // There should be no attempt on the cache service to get the cached resource

            VerifyAllMocks(BlobCacheService);

            result.AssertRight(PublicationViewModel);
        }

        [Fact]
        public async Task Get()
        {
            var release2000Version0Id = Guid.NewGuid();
            var supersedingPublication = new Publication
            {
                Releases = new List<Release>
                {
                    new()
                    {
                        Published = null,
                    },
                }
            };
            var publication = new Publication
            {
                Title = "Publication Title",
                Slug = PublicationSlug,
                SupersededBy = supersedingPublication,
                Releases = new List<Release>
                {
                    new () // latest published release
                    {
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 1,
                        PreviousVersionId = release2000Version0Id,
                    },
                    new ()
                    {
                        Id = release2000Version0Id,
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 0,
                    },
                    new () // not published
                    {
                        ReleaseName = "2001",
                        Slug = "2001",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = null,
                        Version = 0,
                    },
                    new () // published so appears in ListReleases result
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
                    new ()
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
                Assert.False(publicationViewModel.IsSuperseded); // because superseding publication has no Live releases

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
        public async Task Get_IsSuperseded()
        {
            var release2000Version0Id = Guid.NewGuid();
            var supersedingPublication = new Publication
            {
                Releases = new List<Release>
                {
                    new ()
                    {
                        Published = DateTime.UtcNow,
                    },
                }
            };
            var publication = new Publication
            {
                Title = "Publication Title",
                Slug = PublicationSlug,
                SupersededBy = supersedingPublication,
                Releases = new List<Release>
                {
                    new ()
                    {
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 1,
                        PreviousVersionId = release2000Version0Id,
                    },
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
                Assert.True(publicationViewModel.IsSuperseded);

                Assert.Single(publicationViewModel.Releases);
                Assert.Equal(publication.Releases[0].Id, publicationViewModel.LatestReleaseId);
            }
        }

        [Fact]
        public async Task Get_NoPublication()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get("nonexistent-publication");

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Get_PublicationHasNoLiveLatestRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(new Publication
                {
                    Slug = PublicationSlug,
                    Releases = new List<Release>
                    {
                        new () // not published
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

                var result = await service.Get(PublicationSlug);

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
                    : new PersistenceHelper<ContentDbContext>(contentDbContext)
            );
        }

        private static PublicationService SetupPublicationServiceStub(
            ContentDbContext? contentDbContext = null,
            IMapper? mapper = null)
        {
            return new PublicationServiceStub(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                contentDbContext is null
                    ? Mock.Of<IPersistenceHelper<ContentDbContext>>()
                    : new PersistenceHelper<ContentDbContext>(contentDbContext)
            );
        }

        private class PublicationServiceStub : PublicationService
        {
            public PublicationServiceStub(ContentDbContext contentDbContext,
                IPersistenceHelper<ContentDbContext> contentPersistenceHelper) : base(
                contentDbContext,
                contentPersistenceHelper)
            {
            }

            public override Task<Either<ActionResult, PublicationViewModel>> Get(string publicationSlug)
            {
                // Stub a response value to allow testing cached methods which use this
                return Task.FromResult(new Either<ActionResult, PublicationViewModel>(PublicationViewModel));
            }
        }
    }
}
