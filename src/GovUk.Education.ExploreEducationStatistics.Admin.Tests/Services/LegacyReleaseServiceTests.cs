using System.Collections.Generic;
using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
        public async void GetLegacyRelease()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();
            
            using (var context = InMemoryApplicationDbContext("GetLegacyRelease"))
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
            }

            using (var context = InMemoryApplicationDbContext("GetLegacyRelease"))
            {
                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );
                
                // Service method under test
                var result = await legacyReleaseService.GetLegacyRelease(id);

                var legacyRelease = context.LegacyReleases.Single(release => release.Id == result.Right.Id);

                Assert.Equal("Test description", legacyRelease.Description);
                Assert.Equal("http://test.com", legacyRelease.Url);
                Assert.Equal(1, legacyRelease.Order);
                Assert.Equal(publicationId, legacyRelease.PublicationId);
            }
        }

        [Fact]
        public async void CreateLegacyRelease()
        {
            var publicationId = Guid.NewGuid();
            
            using (var context = InMemoryApplicationDbContext("CreateLegacyRelease"))
            {
                context.Add(new Publication 
                {
                    Id = publicationId,
                });

                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("CreateLegacyRelease"))
            {
                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );
                
                // Service method under test
                var result = await legacyReleaseService.CreateLegacyRelease(
                    new CreateLegacyReleaseViewModel()
                    {
                        Description = "Test description",
                        Url = "http://test.com",
                        PublicationId = publicationId
                    });

                var legacyRelease = context.LegacyReleases.Single(release => release.Id == result.Right.Id);

                Assert.Equal("Test description", legacyRelease.Description);
                Assert.Equal("http://test.com", legacyRelease.Url);
                Assert.Equal(1, legacyRelease.Order);
                Assert.Equal(publicationId, legacyRelease.PublicationId);
            }
        }

        [Fact]
        public async void CreateLegacyRelease_WithExisting()
        {
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext("CreateLegacyRelease_WithExisting"))
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
            }

            using (var context = InMemoryApplicationDbContext("CreateLegacyRelease_WithExisting"))
            {
                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );
                
                // Service method under test
                var result = await legacyReleaseService.CreateLegacyRelease(
                    new CreateLegacyReleaseViewModel()
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
        public async void UpdateLegacyRelease()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext("UpdateLegacyRelease"))
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
            }

            using (var context = InMemoryApplicationDbContext("UpdateLegacyRelease"))
            {
                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );
                
                // Service method under test
                var result = await legacyReleaseService.UpdateLegacyRelease(
                    id,
                    new UpdateLegacyReleaseViewModel()
                    {
                        Description = "Updated test description",
                        Url = "http://updated-test.com",
                        Order = 1,
                        PublicationId = publicationId,
                    });

                var legacyRelease = context.LegacyReleases.Single(release => release.Id == result.Right.Id);

                Assert.Equal("Updated test description", legacyRelease.Description);
                Assert.Equal("http://updated-test.com", legacyRelease.Url);
                Assert.Equal(1, legacyRelease.Order);
                Assert.Equal(publicationId, legacyRelease.PublicationId);
            }
        }

        [Fact]
        public async void UpdateLegacyRelease_ReordersWithExisting()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext("UpdateLegacyRelease_ReordersWithExisting"))
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
                                Id = id,
                                Description = "Test description 3",
                                Url = "http://test3.com",
                                Order = 3,
                            }
                        }
                });

                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("UpdateLegacyRelease_ReordersWithExisting"))
            {
                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );
                
                // Service method under test
                var result = await legacyReleaseService.UpdateLegacyRelease(
                    id,
                    new UpdateLegacyReleaseViewModel()
                    {
                        Description = "Updated test description 3",
                        Url = "http://updated-test3.com",
                        Order = 1,
                        PublicationId = publicationId,
                    });

                var legacyReleases = context.LegacyReleases
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
        
        [Fact]
        public async void DeleteLegacyRelease()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext("DeleteLegacyRelease"))
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
            }

            using (var context = InMemoryApplicationDbContext("DeleteLegacyRelease"))
            {
                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );
                
                // Service method under test
                var result = await legacyReleaseService.DeleteLegacyRelease(id);

                Assert.Empty(context.LegacyReleases);
                Assert.Empty(
                    context.Publications
                        .Single(publication => publication.Id == publicationId)
                        .LegacyReleases
                );
            }
        }

        [Fact]
        public async void DeleteLegacyRelease_ReordersWithExisting()
        {
            var id = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            using (var context = InMemoryApplicationDbContext("DeleteLegacyRelease_ReordersWithExisting"))
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
            }

            using (var context = InMemoryApplicationDbContext("DeleteLegacyRelease_ReordersWithExisting"))
            {
                var legacyReleaseService = new LegacyReleaseService(
                    context,
                    AdminMapper(),
                    MockUtils.AlwaysTrueUserService().Object,
                    new PersistenceHelper<ContentDbContext>(context)
                );
                
                // Service method under test
                var result = await legacyReleaseService.DeleteLegacyRelease(id);

                var legacyReleases = context.LegacyReleases
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