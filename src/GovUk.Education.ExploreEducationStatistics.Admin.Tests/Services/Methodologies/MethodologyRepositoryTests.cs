using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyRepositoryTests
    {
        [Fact]
        public async Task UserHasReleaseRoleAssociatedWithMethodology_ReleaseRoleLead()
        {
            var userId = Guid.NewGuid();
            var methodology = new Methodology
            {
                Title = "Test methodology",
            };
            var publication = new Publication
            {
                Methodology = methodology
            };
            var release = new Release
            {
                Publication = publication
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = ReleaseRole.Lead,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyRepository = new MethodologyRepository(contentDbContext);
                Assert.True(
                    await methodologyRepository.UserHasReleaseRoleAssociatedWithMethodology(userId, methodology));
            }
        }

        [Fact]
        public async Task GetMethodologiesForUser_ReleaseRoleLead()
        {
            var userId = Guid.NewGuid();
            var methodology = new Methodology
            {
                Title = "Test methodology",
            };
            var publication = new Publication
            {
                Methodology = methodology
            };
            var release = new Release
            {
                Publication = publication
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = ReleaseRole.Lead,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyRepository = new MethodologyRepository(contentDbContext);
                var methodologies = await methodologyRepository.GetMethodologiesForUser(userId);

                Assert.Single(methodologies);
                Assert.Equal(methodology.Id, methodologies[0].Id);
                Assert.Equal(methodology.Title, methodologies[0].Title);
            }
        }

        [Fact]
        public async Task UserHasReleaseRoleAssociatedWithMethodology_ReleaseRolePrerelease()
        {
            var userId = Guid.NewGuid();
            var methodology = new Methodology();
            var publication = new Publication
            {
                Methodology = methodology
            };
            var release = new Release
            {
                Publication = publication
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = ReleaseRole.PrereleaseViewer,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyRepository = new MethodologyRepository(contentDbContext);
                Assert.False(
                    await methodologyRepository.UserHasReleaseRoleAssociatedWithMethodology(userId, methodology));
            }
        }

        [Fact]
        public async Task GetMethodologiesForUser_ReleaseRolePrerelease()
        {
            var userId = Guid.NewGuid();
            var methodology = new Methodology();
            var publication = new Publication
            {
                Methodology = methodology
            };
            var release = new Release
            {
                Publication = publication
            };
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                Release = release,
                Role = ReleaseRole.PrereleaseViewer,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyRepository = new MethodologyRepository(contentDbContext);
                var methodologies = await methodologyRepository.GetMethodologiesForUser(userId);

                Assert.Empty(methodologies);
            }
        }
        
        [Fact]
        public async Task GetMethodologiesForUser_Multiple()
        {
            var userId = Guid.NewGuid();
            var methodology1 = new Methodology
            {
                Title = "Test methodology 1",
            };
            var publication1 = new Publication
            {
                Title = "Test pub 1",
                Methodology = methodology1
            };
            var release1 = new Release
            {
                Publication = publication1
            };
            var userReleaseRole1 = new UserReleaseRole
            {
                UserId = userId,
                Release = release1,
                Role = ReleaseRole.Lead,
            };

            var methodology2 = new Methodology
            {
                Title = "Test methodology 2",
            };
            var publication2 = new Publication
            {
                Title = "Test pub 2",
                Methodology = methodology2
            };
            var release2 = new Release
            {
                Publication = publication2
            };
            var userReleaseRole2 = new UserReleaseRole
            {
                UserId = userId,
                Release = release2,
                Role = ReleaseRole.Contributor,
            };
            
            var methodology3 = new Methodology
            {
                Title = "Ignored methodology 3",
            };
            var publication3 = new Publication
            {
                Title = "Test pub 3",
                Methodology = methodology3
            };
            var release3 = new Release
            {
                Publication = publication3
            };
            var userReleaseRole3 = new UserReleaseRole
            {
                UserId = Guid.NewGuid(),
                Release = release3,
                Role = ReleaseRole.Lead,
            };
            
            var methodology4 = new Methodology
            {
                Title = "Ignored methodology 4",
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseRole1);
                await contentDbContext.AddAsync(userReleaseRole2);
                await contentDbContext.AddAsync(userReleaseRole3);
                await contentDbContext.AddAsync(methodology4);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyRepository = new MethodologyRepository(contentDbContext);
                var methodologies = await methodologyRepository.GetMethodologiesForUser(userId);

                Assert.Equal(2, methodologies.Count);
                Assert.Contains(methodologies, m => m.Id == methodology1.Id);
                Assert.Contains(methodologies, m => m.Id == methodology2.Id);
                Assert.DoesNotContain(methodologies, m => m.Id == methodology3.Id);
                Assert.DoesNotContain(methodologies, m => m.Id == methodology4.Id);
            }
        }
    }
}
