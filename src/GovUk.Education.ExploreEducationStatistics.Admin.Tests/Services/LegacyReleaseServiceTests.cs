using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

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
                            new LegacyRelease
                            {
                                Id = id,
                                Description = "Test description",
                                Url = "http://test.com",
                                Order = 1,
                            }
                        }
                });

                context.SaveChanges();

                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );

                // Service method under test
                var result = await legacyReleaseService.GetLegacyRelease(id);

                Assert.Equal("Test description", result.Right.Description);
                Assert.Equal("http://test.com", result.Right.Url);
                Assert.Equal(1, result.Right.Order);
                Assert.Equal(publicationId, result.Right.PublicationId);
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

                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );

                // Service method under test
                var result = await legacyReleaseService.CreateLegacyRelease(
                    new LegacyReleaseCreateViewModel()
                    {
                        Description = "Test description",
                        Url = "http://test.com",
                        PublicationId = publicationId
                    });

                Assert.Equal("Test description", result.Right.Description);
                Assert.Equal("http://test.com", result.Right.Url);
                Assert.Equal(1, result.Right.Order);
                Assert.Equal(publicationId, result.Right.PublicationId);

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

                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );

                // Service method under test
                var result = await legacyReleaseService.CreateLegacyRelease(
                    new LegacyReleaseCreateViewModel()
                    {
                        Description = "Test description 2",
                        Url = "http://test2.com",
                        PublicationId = publicationId
                    });

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

                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );

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

                Assert.Equal("Updated test description", result.Right.Description);
                Assert.Equal("http://updated-test.com", result.Right.Url);
                Assert.Equal(1, result.Right.Order);
                Assert.Equal(publicationId, result.Right.PublicationId);

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

                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );

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

                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );

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

                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );

                // Service method under test
                await legacyReleaseService.DeleteLegacyRelease(id);

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

                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );

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
    }
}
