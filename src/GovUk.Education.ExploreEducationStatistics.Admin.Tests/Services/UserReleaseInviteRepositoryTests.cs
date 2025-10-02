#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserReleaseInviteRepositoryTests
{
    private readonly DataFixture _fixture = new();

    public class CreateTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var releaseVersionId = Guid.NewGuid();
            var createdById = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);
                await repository.Create(
                    releaseVersionId: releaseVersionId,
                    email: "test@test.com",
                    releaseRole: ReleaseRole.Contributor,
                    emailSent: true,
                    createdById: createdById
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvite = await contentDbContext.UserReleaseInvites.AsQueryable().SingleOrDefaultAsync();

                Assert.NotNull(userReleaseInvite);
                Assert.Equal(releaseVersionId, userReleaseInvite.ReleaseVersionId);
                Assert.Equal("test@test.com", userReleaseInvite.Email);
                Assert.Equal(ReleaseRole.Contributor, userReleaseInvite.Role);
                Assert.True(userReleaseInvite.EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvite.Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvite.CreatedById);
            }
        }
    }

    public class CreateManyIfNotExistsTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var createdById = Guid.NewGuid();
            var releaseVersionId1 = Guid.NewGuid();
            var releaseVersionId2 = Guid.NewGuid();
            var existingReleaseInvite = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
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
                var repository = CreateRepository(contentDbContext);
                await repository.CreateManyIfNotExists(
                    releaseVersionIds: ListOf(
                        releaseVersionId1,
                        releaseVersionId2,
                        existingReleaseInvite.ReleaseVersionId
                    ),
                    email: "test@test.com",
                    releaseRole: ReleaseRole.Contributor,
                    emailSent: false,
                    createdById: createdById
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = await contentDbContext.UserReleaseInvites.AsQueryable().ToListAsync();

                Assert.Equal(3, userReleaseInvites.Count);

                Assert.Equal(existingReleaseInvite.Id, userReleaseInvites[0].Id);
                Assert.Equal(existingReleaseInvite.ReleaseVersionId, userReleaseInvites[0].ReleaseVersionId);
                Assert.Equal("test@test.com", userReleaseInvites[0].Email);
                Assert.Equal(ReleaseRole.Contributor, userReleaseInvites[0].Role);
                Assert.True(userReleaseInvites[0].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[0].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvites[0].CreatedById);

                Assert.Equal(releaseVersionId1, userReleaseInvites[1].ReleaseVersionId);
                Assert.Equal("test@test.com", userReleaseInvites[1].Email);
                Assert.Equal(ReleaseRole.Contributor, userReleaseInvites[1].Role);
                Assert.False(userReleaseInvites[1].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[1].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvites[1].CreatedById);

                Assert.Equal(releaseVersionId2, userReleaseInvites[2].ReleaseVersionId);
                Assert.Equal("test@test.com", userReleaseInvites[2].Email);
                Assert.Equal(ReleaseRole.Contributor, userReleaseInvites[2].Role);
                Assert.False(userReleaseInvites[2].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[2].Created).Milliseconds, 0, 1500);
                Assert.Equal(createdById, userReleaseInvites[2].CreatedById);
            }
        }
    }

    public class UserHasInviteTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task UserHasInvite_True()
        {
            var invite = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(invite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);
                var result = await repository.UserHasInvite(invite.ReleaseVersionId, invite.Email, invite.Role);
                Assert.True(result);
            }
        }

        [Fact]
        public async Task UserHasInvite_False()
        {
            await using var contentDbContext = InMemoryApplicationDbContext(Guid.NewGuid().ToString());
            var repository = CreateRepository(contentDbContext);
            var result = await repository.UserHasInvite(Guid.Empty, "test@test.com", ReleaseRole.Contributor);
            Assert.False(result);
        }
    }

    public class UserHasInvitesTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task UserHasInvites_True()
        {
            var invite1 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
            };

            var invite2 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(invite1, invite2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);
                var result = await repository.UserHasInvites(
                    ListOf(invite1.ReleaseVersionId, invite2.ReleaseVersionId),
                    "test@test.com",
                    ReleaseRole.Contributor
                );
                Assert.True(result);
            }
        }

        [Fact]
        public async Task UserHasInvites_False()
        {
            var invite1 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
            };

            var invite2 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersionId = Guid.NewGuid(),
                Role = ReleaseRole.Contributor,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(invite1, invite2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);
                var result = await repository.UserHasInvites(
                    ListOf(invite1.ReleaseVersionId, invite2.ReleaseVersionId, Guid.NewGuid()),
                    "test@test.com",
                    ReleaseRole.Contributor
                );
                Assert.False(result);
            }
        }
    }

    public class RemoveTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task TargetInviteExists_RemovesTargetedInvites()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var targetRole = ReleaseRole.Approver;
            var otherRole = ReleaseRole.Contributor;
            var targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var userReleaseInvites = _fixture
                .DefaultUserReleaseInvite()
                // This invite should be removed
                .ForIndex(0, s => s.SetReleaseVersion(targetReleaseVersion))
                .ForIndex(0, s => s.SetEmail(targetEmail))
                .ForIndex(0, s => s.SetRole(targetRole))
                // This invite is for a different release version and should not be removed
                .ForIndex(1, s => s.SetReleaseVersion(otherReleaseVersion))
                .ForIndex(1, s => s.SetEmail(targetEmail))
                .ForIndex(1, s => s.SetRole(targetRole))
                // This invite is for a different email and should not be removed
                .ForIndex(2, s => s.SetReleaseVersion(targetReleaseVersion))
                .ForIndex(2, s => s.SetEmail(otherEmail))
                .ForIndex(2, s => s.SetRole(targetRole))
                // This invite is for a different role and should not be removed
                .ForIndex(3, s => s.SetReleaseVersion(targetReleaseVersion))
                .ForIndex(3, s => s.SetEmail(targetEmail))
                .ForIndex(3, s => s.SetRole(otherRole))
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(userReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.Remove(
                    releaseVersionId: targetReleaseVersion.Id,
                    email: targetEmail,
                    role: targetRole
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                Assert.Equal(3, remainingInvites.Count);

                Assert.Equal(otherReleaseVersion.Id, remainingInvites[0].ReleaseVersionId);
                Assert.Equal(targetEmail, remainingInvites[0].Email);
                Assert.Equal(targetRole, remainingInvites[0].Role);

                Assert.Equal(targetReleaseVersion.Id, remainingInvites[1].ReleaseVersionId);
                Assert.Equal(otherEmail, remainingInvites[1].Email);
                Assert.Equal(targetRole, remainingInvites[1].Role);

                Assert.Equal(targetReleaseVersion.Id, remainingInvites[2].ReleaseVersionId);
                Assert.Equal(targetEmail, remainingInvites[2].Email);
                Assert.Equal(otherRole, remainingInvites[2].Role);
            }
        }

        [Fact]
        public async Task InviteDoesNotExist_DoesNothing()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.Remove(
                    releaseVersionId: Guid.NewGuid(),
                    email: "test1@test.com",
                    role: ReleaseRole.Approver
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserPublicationInvites.ToListAsync();

                Assert.Empty(remainingInvites);
            }
        }
    }

    public class RemoveManyTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task TargetInvitesExist_RemovesTargetedInvites()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var targetRole = ReleaseRole.Approver;
            var otherRole = ReleaseRole.Contributor;
            var targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var userReleaseInvites = _fixture
                .DefaultUserReleaseInvite()
                // These 2 invites should be removed
                .ForIndex(0, s => s.SetReleaseVersion(targetReleaseVersion))
                .ForIndex(0, s => s.SetEmail(targetEmail))
                .ForIndex(0, s => s.SetRole(targetRole))
                .ForIndex(1, s => s.SetReleaseVersion(targetReleaseVersion))
                .ForIndex(1, s => s.SetEmail(targetEmail))
                .ForIndex(1, s => s.SetRole(targetRole))
                // This invite is for a different release version and should not be removed
                .ForIndex(2, s => s.SetReleaseVersion(otherReleaseVersion))
                .ForIndex(2, s => s.SetEmail(targetEmail))
                .ForIndex(2, s => s.SetRole(targetRole))
                // This invite is for a different email and should not be removed
                .ForIndex(3, s => s.SetReleaseVersion(targetReleaseVersion))
                .ForIndex(3, s => s.SetEmail(otherEmail))
                .ForIndex(3, s => s.SetRole(targetRole))
                // This invite is for a different role and should not be removed
                .ForIndex(4, s => s.SetReleaseVersion(targetReleaseVersion))
                .ForIndex(4, s => s.SetEmail(targetEmail))
                .ForIndex(4, s => s.SetRole(otherRole))
                .GenerateList(5);

            var userReleaseInvitesToRemove = new[] { userReleaseInvites[0], userReleaseInvites[1] };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(userReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveMany(userReleaseInvitesToRemove);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                Assert.Equal(3, remainingInvites.Count);

                Assert.Equal(otherReleaseVersion.Id, remainingInvites[0].ReleaseVersionId);
                Assert.Equal(targetEmail, remainingInvites[0].Email);
                Assert.Equal(targetRole, remainingInvites[0].Role);

                Assert.Equal(targetReleaseVersion.Id, remainingInvites[1].ReleaseVersionId);
                Assert.Equal(otherEmail, remainingInvites[1].Email);
                Assert.Equal(targetRole, remainingInvites[1].Role);

                Assert.Equal(targetReleaseVersion.Id, remainingInvites[2].ReleaseVersionId);
                Assert.Equal(targetEmail, remainingInvites[2].Email);
                Assert.Equal(otherRole, remainingInvites[2].Role);
            }
        }

        [Fact]
        public async Task TargetInviteDoesNotExist_ThrowsException()
        {
            var existingUserReleaseInvite = _fixture
                .DefaultUserReleaseInvite()
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithEmail("test@test.com")
                .WithRole(ReleaseRole.Approver)
                .Generate();

            var targetUserReleaseInvite = _fixture
                .DefaultUserReleaseInvite()
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithEmail("test@test.com")
                .WithRole(ReleaseRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.Add(existingUserReleaseInvite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
                    await repository.RemoveMany([targetUserReleaseInvite])
                );
            }
        }

        [Fact]
        public async Task EmptyList_DoesNothing()
        {
            var existingUserReleaseInvite = _fixture
                .DefaultUserReleaseInvite()
                .WithReleaseVersion(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                )
                .WithEmail("test@test.com")
                .WithRole(ReleaseRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.Add(existingUserReleaseInvite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveMany([]);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                var remainingInvite = Assert.Single(remainingInvites);

                Assert.Equal(existingUserReleaseInvite.ReleaseVersionId, remainingInvite.ReleaseVersionId);
                Assert.Equal(existingUserReleaseInvite.Email, remainingInvite.Email);
                Assert.Equal(existingUserReleaseInvite.Role, remainingInvite.Role);
            }
        }
    }

    public class RemoveByPublicationTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task TargetPublicationHasInvites_RemovesTargetInvites()
        {
            var email1 = "test1@test.com";
            var email2 = "test2@test.com";
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetPublication = _fixture.DefaultPublication().Generate();
            var otherPublication = _fixture.DefaultPublication().Generate();
            var targetReleaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(otherPublication))
                .Generate();

            var expectedUserReleaseInvitesToRemove = new List<UserReleaseInvite>();
            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var role in allRoles)
            {
                var targetedUserReleaseInvites = new[]
                {
                    // Create a user release invite for EACH ROLE for each TARGET release version and EACH EMAIL
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithEmail(email1)
                        .WithRole(role)
                        .Generate(),
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithEmail(email2)
                        .WithRole(role)
                        .Generate(),
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithEmail(email1)
                        .WithRole(role)
                        .Generate(),
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithEmail(email2)
                        .WithRole(role)
                        .Generate(),
                };

                expectedUserReleaseInvitesToRemove.AddRange(targetedUserReleaseInvites);

                allUserReleaseInvites.AddRange(
                    [
                        .. targetedUserReleaseInvites,
                        // Create a user release invite for EACH ROLE for the OTHER release version and EACH EMAIL
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email1)
                            .WithRole(role)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email2)
                            .WithRole(role)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByPublication(publicationId: targetPublication.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                var expectedNumberOfInvitesToRemove = allRoles.Count * 4; // 2 release versions + 2 emails
                var expectedNumberOfRemainingInvites = allUserReleaseInvites.Count - expectedNumberOfInvitesToRemove;
                Assert.Equal(expectedNumberOfRemainingInvites, remainingInvites.Count);

                Assert.DoesNotContain(
                    remainingInvites,
                    invite =>
                        expectedUserReleaseInvitesToRemove.Any(i =>
                            invite.ReleaseVersionId == i.ReleaseVersionId
                            && invite.Email == i.Email
                            && invite.Role == i.Role
                        )
                );
            }
        }

        [Theory]
        [InlineData(new[] { ReleaseRole.Approver })]
        [InlineData(new[] { ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.PrereleaseViewer })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer })]
        public async Task TargetPublicationAndRolesCombinationHasInvites_RemovesTargetInvites(
            ReleaseRole[] targetRolesToInclude
        )
        {
            var email1 = "test1@test.com";
            var email2 = "test2@test.com";
            var otherRoles = EnumUtil.GetEnums<ReleaseRole>().Except(targetRolesToInclude);
            var targetPublication = _fixture.DefaultPublication().Generate();
            var otherPublication = _fixture.DefaultPublication().Generate();
            var targetReleaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(otherPublication))
                .Generate();

            var expectedUserReleaseInvitesToRemove = new List<UserReleaseInvite>();
            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var targetRole in targetRolesToInclude)
            {
                var targetedUserReleaseInvites = new[]
                {
                    // Create a user release invite for each TARGET role for each TARGET release version and EACH email
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithEmail(email1)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithEmail(email2)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithEmail(email1)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithEmail(email2)
                        .WithRole(targetRole)
                        .Generate(),
                };

                expectedUserReleaseInvitesToRemove.AddRange(targetedUserReleaseInvites);

                allUserReleaseInvites.AddRange(
                    [
                        .. targetedUserReleaseInvites,
                        // Create a user release invite for each TARGET role for the OTHER release version and EACH email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email1)
                            .WithRole(targetRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email2)
                            .WithRole(targetRole)
                            .Generate(),
                    ]
                );
            }

            foreach (var otherRole in otherRoles)
            {
                allUserReleaseInvites.AddRange(
                    [
                        // Create a user release invite for each OTHER role for each TARGET release version and EACH email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion1)
                            .WithEmail(email1)
                            .WithRole(otherRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion1)
                            .WithEmail(email2)
                            .WithRole(otherRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion2)
                            .WithEmail(email1)
                            .WithRole(otherRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion2)
                            .WithEmail(email2)
                            .WithRole(otherRole)
                            .Generate(),
                        // Create a user release invite for each OTHER role for the OTHER release version and EACH email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email1)
                            .WithRole(otherRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email2)
                            .WithRole(otherRole)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByPublication(
                    publicationId: targetPublication.Id,
                    rolesToInclude: targetRolesToInclude
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                var expectedNumberOfInvitesToRemove = targetRolesToInclude.Length * 4; // 2 release versions + 2 emails
                var expectedNumberOfRemainingInvites = allUserReleaseInvites.Count - expectedNumberOfInvitesToRemove;
                Assert.Equal(expectedNumberOfRemainingInvites, remainingInvites.Count);

                Assert.DoesNotContain(
                    remainingInvites,
                    invite =>
                        expectedUserReleaseInvitesToRemove.Any(i =>
                            invite.ReleaseVersionId == i.ReleaseVersionId
                            && invite.Email == i.Email
                            && invite.Role == i.Role
                        )
                );
            }
        }

        [Fact]
        public async Task TargetPublicationHasNoInvites_DoesNothing()
        {
            var email1 = "test1@test.com";
            var email2 = "test2@test.com";
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetPublication = _fixture.DefaultPublication().Generate();
            var otherPublication = _fixture.DefaultPublication().Generate();
            var targetReleaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(otherPublication))
                .Generate();

            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var role in allRoles)
            {
                allUserReleaseInvites.AddRange(
                    [
                        // Create a user release invite for EACH ROLE for the OTHER release version and EACH EMAIL
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email1)
                            .WithRole(role)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email2)
                            .WithRole(role)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByPublication(publicationId: targetPublication.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                Assert.Equal(allUserReleaseInvites.Count, remainingInvites.Count);
            }
        }
    }

    public class RemoveByPublicationAndEmailTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task TargetPublicationAndEmailCombinationHasInvites_RemovesTargetInvites()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetPublication = _fixture.DefaultPublication().Generate();
            var otherPublication = _fixture.DefaultPublication().Generate();
            var targetReleaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(otherPublication))
                .Generate();

            var expectedUserReleaseInvitesToRemove = new List<UserReleaseInvite>();
            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var role in allRoles)
            {
                var targetedUserReleaseInvites = new[]
                {
                    // Create a user release invite for EACH ROLE for each TARGET release version and TARGET email
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithEmail(targetEmail)
                        .WithRole(role)
                        .Generate(),
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithEmail(targetEmail)
                        .WithRole(role)
                        .Generate(),
                };

                expectedUserReleaseInvitesToRemove.AddRange(targetedUserReleaseInvites);

                allUserReleaseInvites.AddRange(
                    [
                        .. targetedUserReleaseInvites,
                        // Create a user release invite for EACH ROLE for each TARGET release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion1)
                            .WithEmail(otherEmail)
                            .WithRole(role)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion2)
                            .WithEmail(otherEmail)
                            .WithRole(role)
                            .Generate(),
                        // Create a user release invite for EACH ROLE for the OTHER release version and TARGET email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(targetEmail)
                            .WithRole(role)
                            .Generate(),
                        // Create a user release invite for EACH ROLE for the OTHER release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(role)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByPublicationAndEmail(publicationId: targetPublication.Id, email: targetEmail);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                var expectedNumberOfInvitesToRemove = allRoles.Count * 2; // 2 release versions
                var expectedNumberOfRemainingInvites = allUserReleaseInvites.Count - expectedNumberOfInvitesToRemove;
                Assert.Equal(expectedNumberOfRemainingInvites, remainingInvites.Count);

                Assert.DoesNotContain(
                    remainingInvites,
                    invite =>
                        expectedUserReleaseInvitesToRemove.Any(i =>
                            invite.ReleaseVersionId == i.ReleaseVersionId
                            && invite.Email == i.Email
                            && invite.Role == i.Role
                        )
                );
            }
        }

        [Theory]
        [InlineData(new[] { ReleaseRole.Approver })]
        [InlineData(new[] { ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.PrereleaseViewer })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer })]
        public async Task TargetPublicationAndEmailAndRolesCombinationHasInvites_RemovesTargetInvites(
            ReleaseRole[] targetRolesToInclude
        )
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var otherRoles = EnumUtil.GetEnums<ReleaseRole>().Except(targetRolesToInclude);
            var targetPublication = _fixture.DefaultPublication().Generate();
            var otherPublication = _fixture.DefaultPublication().Generate();
            var targetReleaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(otherPublication))
                .Generate();

            var expectedUserReleaseInvitesToRemove = new List<UserReleaseInvite>();
            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var targetRole in targetRolesToInclude)
            {
                var targetedUserReleaseInvites = new[]
                {
                    // Create a user release invite for each TARGET role for each TARGET release version and TARGET email
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion1)
                        .WithEmail(targetEmail)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion2)
                        .WithEmail(targetEmail)
                        .WithRole(targetRole)
                        .Generate(),
                };

                expectedUserReleaseInvitesToRemove.AddRange(targetedUserReleaseInvites);

                allUserReleaseInvites.AddRange(
                    [
                        .. targetedUserReleaseInvites,
                        // Create a user release invite for each TARGET role for each TARGET release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion1)
                            .WithEmail(otherEmail)
                            .WithRole(targetRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion2)
                            .WithEmail(otherEmail)
                            .WithRole(targetRole)
                            .Generate(),
                        // Create a user release invite for each TARGET role for the OTHER release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(targetRole)
                            .Generate(),
                    ]
                );
            }

            foreach (var otherRole in otherRoles)
            {
                allUserReleaseInvites.AddRange(
                    [
                        // Create a user release invite for each OTHER role for each TARGET release version and TARGET email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion1)
                            .WithEmail(targetEmail)
                            .WithRole(otherRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion2)
                            .WithEmail(targetEmail)
                            .WithRole(otherRole)
                            .Generate(),
                        // Create a user release invite for each OTHER role for each TARGET release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion1)
                            .WithEmail(otherEmail)
                            .WithRole(otherRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion2)
                            .WithEmail(otherEmail)
                            .WithRole(otherRole)
                            .Generate(),
                        // Create a user release invite for each OTHER role for the OTHER release version and TARGET email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(targetEmail)
                            .WithRole(otherRole)
                            .Generate(),
                        // Create a user release invite for each OTHER role for the OTHER release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(otherRole)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByPublicationAndEmail(
                    publicationId: targetPublication.Id,
                    email: targetEmail,
                    rolesToInclude: targetRolesToInclude
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                var expectedNumberOfInvitesToRemove = targetRolesToInclude.Length * 2; // 2 release versions
                var expectedNumberOfRemainingInvites = allUserReleaseInvites.Count - expectedNumberOfInvitesToRemove;
                Assert.Equal(expectedNumberOfRemainingInvites, remainingInvites.Count);

                Assert.DoesNotContain(
                    remainingInvites,
                    invite =>
                        expectedUserReleaseInvitesToRemove.Any(i =>
                            invite.ReleaseVersionId == i.ReleaseVersionId
                            && invite.Email == i.Email
                            && invite.Role == i.Role
                        )
                );
            }
        }

        [Fact]
        public async Task TargetPublicationAndEmailCombinationHasNoInvites_DoesNothing()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetPublication = _fixture.DefaultPublication().Generate();
            var otherPublication = _fixture.DefaultPublication().Generate();
            var targetReleaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var targetReleaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(targetPublication))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(otherPublication))
                .Generate();

            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var role in allRoles)
            {
                allUserReleaseInvites.AddRange(
                    [
                        // Create a user release invite for EACH ROLE for the OTHER release version and OTHER EMAIL
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(role)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByPublicationAndEmail(publicationId: targetPublication.Id, email: targetEmail);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                Assert.Equal(allUserReleaseInvites.Count, remainingInvites.Count);
            }
        }

        [Fact]
        public async Task EmailIsNull_ThrowsException()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = CreateRepository(contentDbContext);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await repository.RemoveByPublicationAndEmail(publicationId: Guid.NewGuid(), email: null!)
            );
        }
    }

    public class RemoveByReleaseVersionTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task TargetReleaseVersionHasInvites_RemovesTargetInvites()
        {
            var email1 = "test1@test.com";
            var email2 = "test2@test.com";
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var expectedUserReleaseInvitesToRemove = new List<UserReleaseInvite>();
            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var role in allRoles)
            {
                var targetedUserReleaseInvites = new[]
                {
                    // Create a user release invite for EACH ROLE for the TARGET release version and EACH EMAIL
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithEmail(email1)
                        .WithRole(role)
                        .Generate(),
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithEmail(email2)
                        .WithRole(role)
                        .Generate(),
                };

                expectedUserReleaseInvitesToRemove.AddRange(targetedUserReleaseInvites);

                allUserReleaseInvites.AddRange(
                    [
                        .. targetedUserReleaseInvites,
                        // Create a user release invite for EACH ROLE for the OTHER release version and EACH EMAIL
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email1)
                            .WithRole(role)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email2)
                            .WithRole(role)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByReleaseVersion(releaseVersionId: targetReleaseVersion.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                var expectedNumberOfInvitesToRemove = allRoles.Count * 2; // 2 emails
                var expectedNumberOfRemainingInvites = allUserReleaseInvites.Count - expectedNumberOfInvitesToRemove;
                Assert.Equal(expectedNumberOfRemainingInvites, remainingInvites.Count);

                Assert.DoesNotContain(
                    remainingInvites,
                    invite =>
                        expectedUserReleaseInvitesToRemove.Any(i =>
                            invite.ReleaseVersionId == i.ReleaseVersionId
                            && invite.Email == i.Email
                            && invite.Role == i.Role
                        )
                );
            }
        }

        [Theory]
        [InlineData(new[] { ReleaseRole.Approver })]
        [InlineData(new[] { ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.PrereleaseViewer })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer })]
        public async Task TargetReleaseVersionAndRolesCombinationHasInvites_RemovesTargetInvites(
            ReleaseRole[] targetRolesToInclude
        )
        {
            var email1 = "test1@test.com";
            var email2 = "test2@test.com";
            var otherRoles = EnumUtil.GetEnums<ReleaseRole>().Except(targetRolesToInclude);
            var targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var expectedUserReleaseInvitesToRemove = new List<UserReleaseInvite>();
            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var targetRole in targetRolesToInclude)
            {
                var targetedUserReleaseInvites = new[]
                {
                    // Create a user release invite for each TARGET role for the TARGET release version and EACH EMAIL
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithEmail(email1)
                        .WithRole(targetRole)
                        .Generate(),
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithEmail(email2)
                        .WithRole(targetRole)
                        .Generate(),
                };

                expectedUserReleaseInvitesToRemove.AddRange(targetedUserReleaseInvites);

                allUserReleaseInvites.AddRange(
                    [
                        .. targetedUserReleaseInvites,
                        // Create a user release invite for each TARGET role for the OTHER release version and EACH EMAIL
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email1)
                            .WithRole(targetRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email2)
                            .WithRole(targetRole)
                            .Generate(),
                    ]
                );
            }

            foreach (var otherRole in otherRoles)
            {
                allUserReleaseInvites.AddRange(
                    [
                        // Create a user release invite for each OTHER role for the TARGET release version and EACH EMAIL
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion)
                            .WithEmail(email1)
                            .WithRole(otherRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion)
                            .WithEmail(email2)
                            .WithRole(otherRole)
                            .Generate(),
                        // Create a user release invite for each OTHER role for the OTHER release version and EACH EMAIL
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email1)
                            .WithRole(otherRole)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email2)
                            .WithRole(otherRole)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByReleaseVersion(
                    releaseVersionId: targetReleaseVersion.Id,
                    rolesToInclude: targetRolesToInclude
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                var expectedNumberOfInvitesToRemove = targetRolesToInclude.Length * 2; // 2 emails
                var expectedNumberOfRemainingInvites = allUserReleaseInvites.Count - expectedNumberOfInvitesToRemove;
                Assert.Equal(expectedNumberOfRemainingInvites, remainingInvites.Count);

                Assert.DoesNotContain(
                    remainingInvites,
                    invite =>
                        expectedUserReleaseInvitesToRemove.Any(i =>
                            invite.ReleaseVersionId == i.ReleaseVersionId
                            && invite.Email == i.Email
                            && invite.Role == i.Role
                        )
                );
            }
        }

        [Fact]
        public async Task TargetReleaseVersionHasNoInvites_DoesNothing()
        {
            var email1 = "test1@test.com";
            var email2 = "test2@test.com";
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var role in allRoles)
            {
                allUserReleaseInvites.AddRange(
                    [
                        // Create a user release invite for EACH ROLE for the OTHER release version and EACH EMAIL
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email1)
                            .WithRole(role)
                            .Generate(),
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(email2)
                            .WithRole(role)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByReleaseVersion(releaseVersionId: targetReleaseVersion.Id);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                Assert.Equal(allUserReleaseInvites.Count, remainingInvites.Count);
            }
        }
    }

    public class RemoveByReleaseVersionAndEmailTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task TargetReleaseVersionAndEmailCombinationHasInvites_RemovesTargetInvites()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var expectedUserReleaseInvitesToRemove = new List<UserReleaseInvite>();
            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var role in allRoles)
            {
                var targetedUserReleaseInvites = new[]
                {
                    // Create a user release invite for EACH ROLE for the TARGET release version and TARGET email
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithEmail(targetEmail)
                        .WithRole(role)
                        .Generate(),
                };

                expectedUserReleaseInvitesToRemove.AddRange(targetedUserReleaseInvites);

                allUserReleaseInvites.AddRange(
                    [
                        .. targetedUserReleaseInvites,
                        // Create a user release invite for EACH ROLE for the TARGET release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(role)
                            .Generate(),
                        // Create a user release invite for EACH ROLE for the OTHER release version and TARGET email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(targetEmail)
                            .WithRole(role)
                            .Generate(),
                        // Create a user release invite for EACH ROLE for the OTHER release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(role)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByReleaseVersionAndEmail(
                    releaseVersionId: targetReleaseVersion.Id,
                    email: targetEmail
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                var expectedNumberOfInvitesToRemove = allRoles.Count;
                var expectedNumberOfRemainingInvites = allUserReleaseInvites.Count - expectedNumberOfInvitesToRemove;
                Assert.Equal(expectedNumberOfRemainingInvites, remainingInvites.Count);

                Assert.DoesNotContain(
                    remainingInvites,
                    invite =>
                        expectedUserReleaseInvitesToRemove.Any(i =>
                            invite.ReleaseVersionId == i.ReleaseVersionId
                            && invite.Email == i.Email
                            && invite.Role == i.Role
                        )
                );
            }
        }

        [Theory]
        [InlineData(new[] { ReleaseRole.Approver })]
        [InlineData(new[] { ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.PrereleaseViewer })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor })]
        [InlineData(new[] { ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer })]
        public async Task TargetReleaseVersionAndEmailAndRolesCombinationHasInvites_RemovesTargetInvites(
            ReleaseRole[] targetRolesToInclude
        )
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var otherRoles = EnumUtil.GetEnums<ReleaseRole>().Except(targetRolesToInclude);
            var targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var expectedUserReleaseInvitesToRemove = new List<UserReleaseInvite>();
            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var targetRole in targetRolesToInclude)
            {
                var targetedUserReleaseInvites = new[]
                {
                    // Create a user release invite for each TARGET role for the TARGET release version and TARGET email
                    _fixture
                        .DefaultUserReleaseInvite()
                        .WithReleaseVersion(targetReleaseVersion)
                        .WithEmail(targetEmail)
                        .WithRole(targetRole)
                        .Generate(),
                };

                expectedUserReleaseInvitesToRemove.AddRange(targetedUserReleaseInvites);

                allUserReleaseInvites.AddRange(
                    [
                        .. targetedUserReleaseInvites,
                        // Create a user release invite for each TARGET role for the TARGET release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(targetRole)
                            .Generate(),
                        // Create a user release invite for each TARGET role for the OTHER release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(targetRole)
                            .Generate(),
                    ]
                );
            }

            foreach (var otherRole in otherRoles)
            {
                allUserReleaseInvites.AddRange(
                    [
                        // Create a user release invite for each OTHER role for the TARGET release version and TARGET email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion)
                            .WithEmail(targetEmail)
                            .WithRole(otherRole)
                            .Generate(),
                        // Create a user release invite for each OTHER role for the TARGET release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(otherRole)
                            .Generate(),
                        // Create a user release invite for each OTHER role for the OTHER release version and TARGET email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(targetReleaseVersion)
                            .WithEmail(targetEmail)
                            .WithRole(otherRole)
                            .Generate(),
                        // Create a user release invite for each OTHER role for the OTHER release version and OTHER email
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(otherRole)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByReleaseVersionAndEmail(
                    releaseVersionId: targetReleaseVersion.Id,
                    email: targetEmail,
                    rolesToInclude: targetRolesToInclude
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                var expectedNumberOfInvitesToRemove = targetRolesToInclude.Length;
                var expectedNumberOfRemainingInvites = allUserReleaseInvites.Count - expectedNumberOfInvitesToRemove;
                Assert.Equal(expectedNumberOfRemainingInvites, remainingInvites.Count);

                Assert.DoesNotContain(
                    remainingInvites,
                    invite =>
                        expectedUserReleaseInvitesToRemove.Any(i =>
                            invite.ReleaseVersionId == i.ReleaseVersionId
                            && invite.Email == i.Email
                            && invite.Role == i.Role
                        )
                );
            }
        }

        [Fact]
        public async Task TargetReleaseVersionAndEmailCombinationHasNoInvites_DoesNothing()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var allRoles = EnumUtil.GetEnums<ReleaseRole>();
            var targetReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var otherReleaseVersion = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var allUserReleaseInvites = new List<UserReleaseInvite>();

            foreach (var role in allRoles)
            {
                allUserReleaseInvites.AddRange(
                    [
                        // Create a user release invite for EACH ROLE for the OTHER release version and OTHER EMAIL
                        _fixture
                            .DefaultUserReleaseInvite()
                            .WithReleaseVersion(otherReleaseVersion)
                            .WithEmail(otherEmail)
                            .WithRole(role)
                            .Generate(),
                    ]
                );
            }

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(allUserReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByReleaseVersionAndEmail(
                    releaseVersionId: targetReleaseVersion.Id,
                    email: targetEmail
                );
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                Assert.Equal(allUserReleaseInvites.Count, remainingInvites.Count);
            }
        }

        [Fact]
        public async Task EmailIsNull_ThrowsException()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = CreateRepository(contentDbContext);

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await repository.RemoveByReleaseVersionAndEmail(releaseVersionId: Guid.NewGuid(), email: null!)
            );
        }
    }

    public class RemoveByUserEmailTests : UserReleaseInviteRepositoryTests
    {
        [Fact]
        public async Task TargetUserHasInvites_RemovesTargetInvites()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var role1 = ReleaseRole.Approver;
            var role2 = ReleaseRole.Contributor;
            var releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var userReleaseInvites = _fixture
                .DefaultUserReleaseInvite()
                // These 2 invites should be removed
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(0, s => s.SetEmail(targetEmail))
                .ForIndex(0, s => s.SetRole(role1))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(1, s => s.SetEmail(targetEmail))
                .ForIndex(1, s => s.SetRole(role2))
                // These invites are for a different email and should not be removed
                .ForIndex(2, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(2, s => s.SetEmail(otherEmail))
                .ForIndex(2, s => s.SetRole(role1))
                .ForIndex(3, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(3, s => s.SetEmail(otherEmail))
                .ForIndex(3, s => s.SetRole(role2))
                .GenerateList(4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(userReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByUserEmail(targetEmail);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                Assert.Equal(2, remainingInvites.Count);

                Assert.Equal(releaseVersion1.Id, remainingInvites[0].ReleaseVersionId);
                Assert.Equal(otherEmail, remainingInvites[0].Email);
                Assert.Equal(role1, remainingInvites[0].Role);

                Assert.Equal(releaseVersion2.Id, remainingInvites[1].ReleaseVersionId);
                Assert.Equal(otherEmail, remainingInvites[1].Email);
                Assert.Equal(role2, remainingInvites[1].Role);
            }
        }

        [Fact]
        public async Task TargetUserHasNoInvites_DoesNothing()
        {
            var targetEmail = "test1@test.com";
            var otherEmail = "test2@test.com";
            var role1 = ReleaseRole.Approver;
            var role2 = ReleaseRole.Contributor;
            var releaseVersion1 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();
            var releaseVersion2 = _fixture
                .DefaultReleaseVersion()
                .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
                .Generate();

            var userReleaseInvites = _fixture
                .DefaultUserReleaseInvite()
                // These invites are for a different email and should not be removed
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(0, s => s.SetEmail(otherEmail))
                .ForIndex(0, s => s.SetRole(role1))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2))
                .ForIndex(1, s => s.SetEmail(otherEmail))
                .ForIndex(1, s => s.SetRole(role2))
                .GenerateList(2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.AddRange(userReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = CreateRepository(contentDbContext);

                await repository.RemoveByUserEmail(targetEmail);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var remainingInvites = await contentDbContext.UserReleaseInvites.ToListAsync();

                Assert.Equal(2, remainingInvites.Count);

                Assert.Equal(releaseVersion1.Id, remainingInvites[0].ReleaseVersionId);
                Assert.Equal(otherEmail, remainingInvites[0].Email);
                Assert.Equal(role1, remainingInvites[0].Role);

                Assert.Equal(releaseVersion2.Id, remainingInvites[1].ReleaseVersionId);
                Assert.Equal(otherEmail, remainingInvites[1].Email);
                Assert.Equal(role2, remainingInvites[1].Role);
            }
        }
    }

    private static UserReleaseInviteRepository CreateRepository(ContentDbContext contentDbContext)
    {
        return new UserReleaseInviteRepository(
            contentDbContext: contentDbContext,
            logger: Mock.Of<ILogger<UserReleaseInviteRepository>>()
        );
    }
}
