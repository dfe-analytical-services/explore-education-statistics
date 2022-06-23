#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task Create()
        {
            var releaseId = Guid.NewGuid();
            var createdById = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new UserReleaseInviteRepository(contentDbContext);
                await repository.Create(
                    releaseId: releaseId,
                    email: "test@test.com",
                    releaseRole: Contributor,
                    emailSent: true,
                    createdById: createdById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvite = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .SingleOrDefaultAsync();

                Assert.NotNull(userReleaseInvite);
                Assert.Equal(releaseId, userReleaseInvite.ReleaseId);
                Assert.Equal("test@test.com", userReleaseInvite.Email);
                Assert.Equal(Contributor, userReleaseInvite.Role);
                Assert.True(userReleaseInvite.EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvite.Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvite.CreatedById);
            }
        }

        [Fact]
        public async Task CreateManyIfNotExists()
        {
            var createdById = Guid.NewGuid();
            var releaseId1 = Guid.NewGuid();
            var releaseId2 = Guid.NewGuid();
            var existingReleaseInvite = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseId = Guid.NewGuid(),
                Role = Contributor,
                EmailSent = true,
                CreatedById = createdById,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(existingReleaseInvite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new UserReleaseInviteRepository(contentDbContext);
                await repository.CreateManyIfNotExists(
                    releaseIds: ListOf(releaseId1, releaseId2,
                        existingReleaseInvite.ReleaseId),
                    email: "test@test.com",
                    releaseRole: Contributor,
                    emailSent: false,
                    createdById: createdById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Equal(3, userReleaseInvites.Count);

                Assert.Equal(existingReleaseInvite.Id, userReleaseInvites[0].Id);
                Assert.Equal(existingReleaseInvite.ReleaseId, userReleaseInvites[0].ReleaseId);
                Assert.Equal("test@test.com", userReleaseInvites[0].Email);
                Assert.Equal(Contributor, userReleaseInvites[0].Role);
                Assert.True(userReleaseInvites[0].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[0].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvites[0].CreatedById);

                Assert.Equal(releaseId1, userReleaseInvites[1].ReleaseId);
                Assert.Equal("test@test.com", userReleaseInvites[1].Email);
                Assert.Equal(Contributor, userReleaseInvites[1].Role);
                Assert.False(userReleaseInvites[1].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[1].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvites[1].CreatedById);

                Assert.Equal(releaseId2, userReleaseInvites[2].ReleaseId);
                Assert.Equal("test@test.com", userReleaseInvites[2].Email);
                Assert.Equal(Contributor, userReleaseInvites[2].Role);
                Assert.False(userReleaseInvites[2].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[2].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvites[2].CreatedById);
            }
        }

        [Fact]
        public async Task UserHasInvite_True()
        {
            var invite = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseId = Guid.NewGuid(),
                Role = Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(invite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new UserReleaseInviteRepository(contentDbContext);
                var result = await repository.UserHasInvite(invite.ReleaseId, invite.Email, invite.Role);
                Assert.True(result);
            }
        }

        [Fact]
        public async Task UserHasInvite_False()
        {
            await using var contentDbContext = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
            var repository = new UserReleaseInviteRepository(contentDbContext);
            var result = await repository.UserHasInvite(Guid.Empty, "test@test.com", Contributor);
            Assert.False(result);
        }

        [Fact]
        public async Task UserHasInvites_True()
        {
            var invite1 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseId = Guid.NewGuid(),
                Role = Contributor,
            };

            var invite2 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseId = Guid.NewGuid(),
                Role = Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(invite1, invite2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new UserReleaseInviteRepository(contentDbContext);
                var result = await repository.UserHasInvites(
                    ListOf(invite1.ReleaseId, invite2.ReleaseId),
                    "test@test.com", Contributor);
                Assert.True(result);
            }
        }

        [Fact]
        public async Task UserHasInvites_False()
        {
            var invite1 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseId = Guid.NewGuid(),
                Role = Contributor,
            };

            var invite2 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseId = Guid.NewGuid(),
                Role = Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(invite1, invite2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new UserReleaseInviteRepository(contentDbContext);
                var result = await repository.UserHasInvites(
                    ListOf(invite1.ReleaseId, invite2.ReleaseId, Guid.NewGuid()),
                    "test@test.com", Contributor);
                Assert.False(result);
            }
        }

        [Fact]
        public async Task RemoveByPublication()
        {
            var release1 = new Release();
            var release2 = new Release();
            var release3 = new Release();
            var publication = new Publication
            {
                Releases = ListOf(release1, release3),
            };
            var invite1 = new UserReleaseInvite
            {
                Email = "test@test.com",
                Release = release1,
                Role = Contributor,
            };

            // not attached to publication
            var invite2 = new UserReleaseInvite
            {
                Email = "test@test.com",
                Release = release2,
                Role = Contributor,
            };

            var invite3 = new UserReleaseInvite
            {
                Email = "test@test.com",
                Release = release3,
                Role = Contributor,
            };

            // not Contributor
            var invite4 = new UserReleaseInvite
            {
                Email = "test@test.com",
                Release = release1,
                Role = Lead,
            };

            // different email address
            var invite5 = new UserReleaseInvite
            {
                Email = "test_different@test.com",
                Release = release1,
                Role = Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(publication,
                    invite1, invite2, invite3, invite4, invite5);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new UserReleaseInviteRepository(contentDbContext);
                await repository.RemoveByPublication(
                    publication, "test@test.com", Contributor);

                var remainingInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Equal(3, remainingInvites.Count);

                Assert.Equal(invite2.Id, remainingInvites[0].Id);
                Assert.Equal(invite2.Email, remainingInvites[0].Email);
                Assert.Equal(invite2.ReleaseId, remainingInvites[0].ReleaseId);
                Assert.Equal(invite2.Role, remainingInvites[0].Role);

                Assert.Equal(invite4.Id, remainingInvites[1].Id);
                Assert.Equal(invite4.Email, remainingInvites[1].Email);
                Assert.Equal(invite4.ReleaseId, remainingInvites[1].ReleaseId);
                Assert.Equal(invite4.Role, remainingInvites[1].Role);

                Assert.Equal(invite5.Id, remainingInvites[2].Id);
                Assert.Equal(invite5.Email, remainingInvites[2].Email);
                Assert.Equal(invite5.ReleaseId, remainingInvites[2].ReleaseId);
                Assert.Equal(invite5.Role, remainingInvites[2].Role);
            }
        }
    }
}
