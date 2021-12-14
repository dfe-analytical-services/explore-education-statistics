#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
                var result = await repository.Create(
                    releaseId: releaseId,
                    email: "test@test.com",
                    releaseRole: Contributor,
                    emailSent: true,
                    createdById: createdById);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvite = contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .SingleOrDefault();

                Assert.NotNull(userReleaseInvite);
                Assert.Equal(releaseId, userReleaseInvite.ReleaseId);
                Assert.Equal("test@test.com", userReleaseInvite.Email);
                Assert.Equal(Contributor, userReleaseInvite.Role);
                Assert.True(userReleaseInvite.EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvite.Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvite.CreatedById);
                Assert.False(userReleaseInvite.Accepted);
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
                Accepted = true,
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
                var result = await repository.CreateManyIfNotExists(
                    releaseIds: ListOf(releaseId1, releaseId2,
                        existingReleaseInvite.ReleaseId),
                    email: "test@test.com",
                    releaseRole: Contributor,
                    emailSent: false,
                    createdById: createdById);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToList();

                Assert.Equal(3, userReleaseInvites.Count);

                Assert.Equal(existingReleaseInvite.Id, userReleaseInvites[0].Id);
                Assert.Equal(existingReleaseInvite.ReleaseId, userReleaseInvites[0].ReleaseId);
                Assert.Equal("test@test.com", userReleaseInvites[0].Email);
                Assert.Equal(Contributor, userReleaseInvites[0].Role);
                Assert.True(userReleaseInvites[0].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[0].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvites[0].CreatedById);
                Assert.True(userReleaseInvites[0].Accepted);

                Assert.Equal(releaseId1, userReleaseInvites[1].ReleaseId);
                Assert.Equal("test@test.com", userReleaseInvites[1].Email);
                Assert.Equal(Contributor, userReleaseInvites[1].Role);
                Assert.False(userReleaseInvites[1].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[1].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvites[1].CreatedById);
                Assert.False(userReleaseInvites[1].Accepted);

                Assert.Equal(releaseId2, userReleaseInvites[2].ReleaseId);
                Assert.Equal("test@test.com", userReleaseInvites[2].Email);
                Assert.Equal(Contributor, userReleaseInvites[2].Role);
                Assert.False(userReleaseInvites[2].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[2].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvites[2].CreatedById);
                Assert.False(userReleaseInvites[2].Accepted);
            }
        }

        [Fact]
        public async Task MarkEmailAsSent()
        {
            var userReleaseInvite = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseId = Guid.NewGuid(),
                Role = Contributor,
                EmailSent = false,
                Created = new DateTime(2000, 1, 1),
                CreatedById = Guid.NewGuid(),
                Accepted = false,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(userReleaseInvite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new UserReleaseInviteRepository(contentDbContext);
                await repository.MarkEmailAsSent(userReleaseInvite);

                var updatedInvite = contentDbContext
                    .UserReleaseInvites
                    .Single();

                Assert.Equal(userReleaseInvite.Id, updatedInvite.Id);
                Assert.Equal(userReleaseInvite.Email, updatedInvite.Email);
                Assert.Equal(userReleaseInvite.Role, updatedInvite.Role);
                Assert.Equal(userReleaseInvite.CreatedById, updatedInvite.CreatedById);
                Assert.Equal(userReleaseInvite.Created, updatedInvite.Created);
                Assert.Equal(userReleaseInvite.Accepted, updatedInvite.Accepted);

                Assert.True(updatedInvite.EmailSent);
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
        public async Task RemoveMany()
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

            var invite3 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseId = Guid.NewGuid(),
                Role = Contributor,
            };

            var invite4 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseId = invite1.ReleaseId,
                Role = Lead,
            };

            var invite5 = new UserReleaseInvite
            {
                Email = "test_different@test.com",
                ReleaseId = invite1.ReleaseId,
                Role = Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(invite1, invite2, invite3, invite4, invite5);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new UserReleaseInviteRepository(contentDbContext);
                await repository.RemoveMany(
                    ListOf(invite1.ReleaseId, invite3.ReleaseId),
                    "test@test.com", Contributor);

                var remainingInvites = contentDbContext.UserReleaseInvites.ToList();

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
