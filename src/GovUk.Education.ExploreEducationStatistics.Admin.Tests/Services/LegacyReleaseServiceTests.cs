#nullable enable
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class LegacyReleaseServiceTests
    {
        [Fact]
        public async Task GetLegacyRelease()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext())
            {
                context.Add(new Publication
                {
                    Id = publicationId,
                    LegacyReleases = new List<LegacyRelease>
                        {
                            new()
                            {
                                Id = id,
                                Description = "Test description",
                                Url = "http://test.com",
                                Order = 1,
                            }
                        }
                });

                await context.SaveChangesAsync();

                var legacyReleaseService = BuildLegacyReleaseService(context);

                // Service method under test
                var result = await legacyReleaseService.GetLegacyRelease(id);

                Assert.Equal("Test description", result.Right.Description);
                Assert.Equal("http://test.com", result.Right.Url);
                Assert.Equal(1, result.Right.Order);
            }
        }

        [Fact]
        public async Task GetLegacyReleases()
        {
            var publication = new Publication
            {
                LegacyReleases = new List<LegacyRelease>
                {
                    new()
                    {
                        Description = "Release 1",
                        Url = "https://test-1.com",
                        Order = 1
                    },
                    new()
                    {
                        Description = "Release 3",
                        Url = "https://test-3.com",
                        Order = 3
                    },
                    new()
                    {
                        Description = "Release 2",
                        Url = "https://test-2.com",
                        Order = 2
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildLegacyReleaseService(contentDbContext);

                var result = await service.GetLegacyReleases(publication.Id);
                var viewModels = result.AssertRight();

                Assert.Equal(3, viewModels.Count);

                Assert.Equal("Release 3", viewModels[0].Description);
                Assert.Equal("https://test-3.com", viewModels[0].Url);
                Assert.Equal(3, viewModels[0].Order);

                Assert.Equal("Release 2", viewModels[1].Description);
                Assert.Equal("https://test-2.com", viewModels[1].Url);
                Assert.Equal(2, viewModels[1].Order);

                Assert.Equal("Release 1", viewModels[2].Description);
                Assert.Equal("https://test-1.com", viewModels[2].Url);
                Assert.Equal(1, viewModels[2].Order);
            }
        }

        [Fact]
        public async Task CreateLegacyRelease()
        {
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext())
            {
                context.Add(new Publication
                {
                    Id = publicationId,
                });

                context.SaveChanges();

                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock => mock.DeleteItem(It.IsAny<PublicationCacheKey>()))
                    .Returns(Task.CompletedTask);

                var legacyReleaseService = BuildLegacyReleaseService(
                    context: context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                // Service method under test
                var result = await legacyReleaseService.CreateLegacyRelease(
                    new LegacyReleaseCreateViewModel()
                    {
                        Description = "Test description",
                        Url = "http://test.com",
                        PublicationId = publicationId
                    });


                VerifyAllMocks(publicBlobCacheService);

                Assert.Equal("Test description", result.Right.Description);
                Assert.Equal("http://test.com", result.Right.Url);
                Assert.Equal(1, result.Right.Order);

                var savedLegacyRelease = context.LegacyReleases.Single(release => release.Id == result.Right.Id);

                Assert.Equal("Test description", savedLegacyRelease.Description);
                Assert.Equal("http://test.com", savedLegacyRelease.Url);
                Assert.Equal(1, savedLegacyRelease.Order);
                Assert.Equal(publicationId, savedLegacyRelease.PublicationId);
            }
        }

        [Fact]
        public async Task CreateLegacyRelease_WithExisting()
        {
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext())
            {
                context.Add(new Publication
                {
                    Id = publicationId,
                    LegacyReleases = new List<LegacyRelease>
                        {
                            new LegacyRelease
                            {
                                Id = Guid.NewGuid(),
                                Description = "Test description 1",
                                Url = "http://test1.com",
                                Order = 1,
                                PublicationId = publicationId,
                            }
                        }
                });

                context.SaveChanges();

                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock => mock.DeleteItem(It.IsAny<PublicationCacheKey>()))
                    .Returns(Task.CompletedTask);

                var legacyReleaseService = BuildLegacyReleaseService(
                    context: context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                // Service method under test
                var result = await legacyReleaseService.CreateLegacyRelease(
                    new LegacyReleaseCreateViewModel()
                    {
                        Description = "Test description 2",
                        Url = "http://test2.com",
                        PublicationId = publicationId
                    });

                VerifyAllMocks(publicBlobCacheService);

                var legacyRelease = context.LegacyReleases.Single(release => release.Id == result.Right.Id);

                Assert.Equal("Test description 2", legacyRelease.Description);
                Assert.Equal("http://test2.com", legacyRelease.Url);
                Assert.Equal(2, legacyRelease.Order);
                Assert.Equal(publicationId, legacyRelease.PublicationId);
            }
        }

        [Fact]
        public async Task UpdateLegacyRelease()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext())
            {
                context.Add(new Publication
                {
                    Id = publicationId,
                    LegacyReleases = new List<LegacyRelease>
                        {
                            new LegacyRelease
                            {
                                Id = id,
                                Description = "Test description",
                                Url = "http://test.com",
                                Order = 2,
                            },
                        }
                });

                context.SaveChanges();

                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock => mock.DeleteItem(It.IsAny<PublicationCacheKey>()))
                    .Returns(Task.CompletedTask);

                var legacyReleaseService = BuildLegacyReleaseService(
                    context: context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                // Service method under test
                var result = await legacyReleaseService.UpdateLegacyRelease(
                    id,
                    new LegacyReleaseUpdateViewModel()
                    {
                        Description = "Updated test description",
                        Url = "http://updated-test.com",
                        Order = 1,
                        PublicationId = publicationId,
                    });

                VerifyAllMocks(publicBlobCacheService);

                Assert.Equal("Updated test description", result.Right.Description);
                Assert.Equal("http://updated-test.com", result.Right.Url);
                Assert.Equal(1, result.Right.Order);

                var savedLegacyRelease = context.LegacyReleases.Single(release => release.Id == result.Right.Id);

                Assert.Equal("Updated test description", savedLegacyRelease.Description);
                Assert.Equal("http://updated-test.com", savedLegacyRelease.Url);
                Assert.Equal(1, savedLegacyRelease.Order);
                Assert.Equal(publicationId, savedLegacyRelease.PublicationId);
            }
        }

        [Fact]
        public async Task UpdateLegacyRelease_ReordersWithExisting()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext())
            {
                context.Add(new Publication
                {
                    Id = publicationId,
                    LegacyReleases = new List<LegacyRelease>
                        {
                            new LegacyRelease
                            {
                                Description = "Test description 1",
                                Url = "http://test1.com",
                                Order = 1,
                            },
                            new LegacyRelease
                            {
                                Description = "Test description 2",
                                Url = "http://test2.com",
                                Order = 2,
                            },
                            new LegacyRelease
                            {
                                Id = id,
                                Description = "Test description 3",
                                Url = "http://test3.com",
                                Order = 3,
                            }
                        }
                });

                context.SaveChanges();

                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock => mock.DeleteItem(It.IsAny<PublicationCacheKey>()))
                    .Returns(Task.CompletedTask);

                var legacyReleaseService = BuildLegacyReleaseService(
                    context: context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                // Service method under test
                await legacyReleaseService.UpdateLegacyRelease(
                    id,
                    new LegacyReleaseUpdateViewModel()
                    {
                        Description = "Updated test description 3",
                        Url = "http://updated-test3.com",
                        Order = 1,
                        PublicationId = publicationId,
                    });

                VerifyAllMocks(publicBlobCacheService);

                var legacyReleases = context.LegacyReleases
                    .AsQueryable()
                    .OrderBy(release => release.Order)
                    .ToList();

                Assert.Equal("Updated test description 3", legacyReleases[0].Description);
                Assert.Equal(1, legacyReleases[0].Order);

                Assert.Equal("Test description 1", legacyReleases[1].Description);
                Assert.Equal(2, legacyReleases[1].Order);

                Assert.Equal("Test description 2", legacyReleases[2].Description);
                Assert.Equal(3, legacyReleases[2].Order);
            }
        }

        /// Test that we do not leave gaps in the order values when
        /// updating a legacy release with an `Order` larger than
        /// the number of legacy releases.
        [Fact]
        public async Task UpdateLegacyRelease_ReordersWithoutGaps()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext())
            {
                context.Add(new Publication
                {
                    Id = publicationId,
                    LegacyReleases = new List<LegacyRelease>
                        {
                            new LegacyRelease
                            {
                                Id = id,
                                Description = "Test description 1",
                                Url = "http://test1.com",
                                Order = 1,
                            },
                            new LegacyRelease
                            {
                                Description = "Test description 2",
                                Url = "http://test2.com",
                                Order = 2,
                            },
                            new LegacyRelease
                            {
                                Description = "Test description 3",
                                Url = "http://test3.com",
                                Order = 3,
                            }
                        }
                });

                context.SaveChanges();

                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock => mock.DeleteItem(It.IsAny<PublicationCacheKey>()))
                    .Returns(Task.CompletedTask);

                var legacyReleaseService = BuildLegacyReleaseService(
                    context: context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                // Service method under test
                await legacyReleaseService.UpdateLegacyRelease(
                    id,
                    new LegacyReleaseUpdateViewModel()
                    {
                        Description = "Updated test description 1",
                        Url = "http://updated-test1.com",
                        Order = 5,
                        PublicationId = publicationId,
                    });

                VerifyAllMocks(publicBlobCacheService);

                var legacyReleases = context.LegacyReleases
                    .AsQueryable()
                    .OrderBy(release => release.Order)
                    .ToList();

                Assert.Equal("Test description 2", legacyReleases[0].Description);
                Assert.Equal(1, legacyReleases[0].Order);

                Assert.Equal("Test description 3", legacyReleases[1].Description);
                Assert.Equal(2, legacyReleases[1].Order);

                Assert.Equal("Updated test description 1", legacyReleases[2].Description);
                Assert.Equal(3, legacyReleases[2].Order);
            }
        }

        [Fact]
        public async Task DeleteLegacyRelease()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext())
            {
                context.Add(new Publication
                {
                    Id = publicationId,
                    LegacyReleases = new List<LegacyRelease>
                        {
                            new LegacyRelease
                            {
                                Id = id,
                                Description = "Test description",
                                Url = "http://test.com",
                                Order = 2,
                            },
                        }
                });

                context.SaveChanges();

                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock => mock.DeleteItem(It.IsAny<PublicationCacheKey>()))
                    .Returns(Task.CompletedTask);

                var legacyReleaseService = BuildLegacyReleaseService(
                    context: context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                // Service method under test
                await legacyReleaseService.DeleteLegacyRelease(id);

                VerifyAllMocks(publicBlobCacheService);
                Assert.Empty(context.LegacyReleases);
                Assert.Empty(
                    context.Publications
                        .Single(publication => publication.Id == publicationId)
                        .LegacyReleases
                );
            }
        }

        [Fact]
        public async Task DeleteLegacyRelease_ReordersWithExisting()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext())
            {
                context.Add(new Publication
                {
                    Id = publicationId,
                    LegacyReleases = new List<LegacyRelease>
                        {
                            new LegacyRelease
                            {
                                Id = id,
                                Description = "Test description 1",
                                Url = "http://test1.com",
                                Order = 1,
                            },
                            new LegacyRelease
                            {
                                Id = Guid.NewGuid(),
                                Description = "Test description 2",
                                Url = "http://test2.com",
                                Order = 2,
                            },
                            new LegacyRelease
                            {
                                Id = Guid.NewGuid(),
                                Description = "Test description 3",
                                Url = "http://test3.com",
                                Order = 3,
                            }
                        }
                });

                context.SaveChanges();

                var publicBlobCacheService = new Mock<IBlobCacheService>(Strict);

                publicBlobCacheService.Setup(mock => mock.DeleteItem(It.IsAny<PublicationCacheKey>()))
                    .Returns(Task.CompletedTask);

                var legacyReleaseService = BuildLegacyReleaseService(
                    context: context,
                    publicBlobCacheService: publicBlobCacheService.Object);

                // Service method under test
                await legacyReleaseService.DeleteLegacyRelease(id);

                var legacyReleases = context.LegacyReleases
                    .AsQueryable()
                    .OrderBy(release => release.Order)
                    .ToList();

                Assert.Equal(2, legacyReleases.Count);
                Assert.Equal(2,
                    context.Publications
                        .Single(publication => publication.Id == publicationId)
                        .LegacyReleases
                        .Count
                );

                Assert.Equal("Test description 2", legacyReleases[0].Description);
                Assert.Equal(1, legacyReleases[0].Order);

                Assert.Equal("Test description 3", legacyReleases[1].Description);
                Assert.Equal(2, legacyReleases[1].Order);
            }
        }

        private LegacyReleaseService BuildLegacyReleaseService(
            ContentDbContext context,
            IMapper? mapper = null,
            IUserService? userService = null,
            IBlobCacheService? publicBlobCacheService = null)
        {
                return new LegacyReleaseService(
                    context,
                    mapper ?? AdminMapper(),
                    userService ?? AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context),
                    publicBlobCacheService ?? Mock.Of<IBlobCacheService>(Strict)
                );
        }
    }
}
