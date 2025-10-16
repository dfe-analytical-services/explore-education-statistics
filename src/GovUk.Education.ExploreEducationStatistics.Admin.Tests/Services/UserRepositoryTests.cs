#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using LinqToDB;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserRepositoryTests
{
    private readonly DataFixture _dataFixture = new();

    public class FindPendingUserInviteByEmailTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ReturnsNull()
        {
            var user = _dataFixture.DefaultUser()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindPendingUserInviteByEmail(user.Email);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_ReturnsUser()
        {
            var user = _dataFixture.DefaultUserWithPendingInvite()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindPendingUserInviteByEmail(user.Email);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_ReturnsNull()
        {
            var user = _dataFixture.DefaultUserWithExpiredInvite()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindPendingUserInviteByEmail(user.Email);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task DifferentCase_ReturnsUser()
        {
            var user = _dataFixture.DefaultUserWithPendingInvite()
                .WithEmail("test@test.com")
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindPendingUserInviteByEmail("TEST@TEST.COM");

                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_ReturnsNull()
        {
            var user = _dataFixture.DefaultSoftDeletedUser()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindPendingUserInviteByEmail(user.Email);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = SetupUserRepository(contentDbContext);
            var result = await repository.FindPendingUserInviteByEmail("test@test.com");
            Assert.Null(result);
        }
    }

    public class FindActiveUserByEmailTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ReturnsUser()
        {
            var user = _dataFixture.DefaultUser()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindActiveUserByEmail(user.Email);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_ReturnsNull()
        {
            var user = _dataFixture.DefaultUserWithPendingInvite()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindActiveUserByEmail(user.Email);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_ReturnsNull()
        {
            var user = _dataFixture.DefaultUserWithExpiredInvite()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindActiveUserByEmail(user.Email);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task DifferentCase_ReturnsUser()
        {
            var user = _dataFixture.DefaultUser()
                .WithEmail("test@test.com")
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindActiveUserByEmail("TEST@TEST.COM");

                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_ReturnsNull()
        {
            var user = _dataFixture.DefaultSoftDeletedUser()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindActiveUserByEmail(user.Email);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = SetupUserRepository(contentDbContext);
            var result = await repository.FindActiveUserByEmail("test@test.com");
            Assert.Null(result);
        }
    }

    public class FindActiveUserByIdTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ReturnsUser()
        {
            var user = _dataFixture.DefaultUser()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindActiveUserById(user.Id);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_ReturnsNull()
        {
            var user = _dataFixture.DefaultUserWithPendingInvite()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindActiveUserById(user.Id);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_ReturnsNull()
        {
            var user = _dataFixture.DefaultUserWithExpiredInvite()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindActiveUserById(user.Id);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_ReturnsNull()
        {
            var user = _dataFixture.DefaultSoftDeletedUser()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindActiveUserById(user.Id);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = SetupUserRepository(contentDbContext);
            var result = await repository.FindActiveUserById(Guid.NewGuid());
            Assert.Null(result);
        }
    }

    public class FindUserByEmailTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ReturnsUser()
        {
            var user = _dataFixture.DefaultUser()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindUserByEmail(user.Email);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_ReturnsUser()
        {
            var user = _dataFixture.DefaultUserWithPendingInvite()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindUserByEmail(user.Email);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_ReturnsUser()
        {
            var user = _dataFixture.DefaultUserWithExpiredInvite()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindUserByEmail(user.Email);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task DifferentCase_ReturnsUser()
        {
            var user = _dataFixture.DefaultUser()
                .WithEmail("test@test.com")
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindUserByEmail("TEST@TEST.COM");

                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_ReturnsNull()
        {
            var user = _dataFixture.DefaultSoftDeletedUser()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);
                var result = await repository.FindUserByEmail(user.Email);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = SetupUserRepository(contentDbContext);
            var result = await repository.FindUserByEmail("test@test.com");
            Assert.Null(result);
        }
    }

    public class FindDeletedUserPlaceholderTests : UserRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var user = _dataFixture.DefaultSoftDeletedUser()
                .WithEmail(User.DeletedUserPlaceholderEmail)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                var result = await repository.FindDeletedUserPlaceholder();

                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
                Assert.Equal(User.DeletedUserPlaceholderEmail, result.Email);
            }
        }

        [Fact]
        public async Task DeletedUserDoesNotExist_ThrowsException()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = SetupUserRepository(contentDbContext);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.FindDeletedUserPlaceholder());
        }
    }

    public class SoftDeleteUserTests : UserRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            var user = _dataFixture.DefaultUser()
                .Generate();
            var deletedById = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            // Ensure initial state is correct
            Assert.True(user.Active);
            Assert.Null(user.SoftDeleted);
            Assert.Null(user.DeletedById);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                await repository.SoftDeleteUser(user, deletedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users
                    .SingleAsync(u => u.Id == user.Id);

                Assert.False(updatedUser.Active);
                updatedUser.SoftDeleted.AssertUtcNow();
                Assert.Equal(deletedById, updatedUser.DeletedById);
            }
        }
    }

    public class CreateOrUpdateTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ThrowsException()
        {
            var user = _dataFixture.DefaultUser()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                await Assert.ThrowsAsync<InvalidOperationException>(async () 
                    => await repository.CreateOrUpdate(
                        email: user.Email,
                        role: GlobalRoles.Role.Analyst,
                        createdById: Guid.NewGuid(),
                        createdDate: DateTimeOffset.UtcNow));
            }
        }

        [Theory]
        [InlineData(GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.PrereleaseUser)]
        [InlineData(GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst)]
        [InlineData(GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.Analyst, GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.Analyst)]
        [InlineData(GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst)]
        [InlineData(GlobalRoles.Role.Analyst, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.Analyst, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        public async Task UserWithPendingInvite_UpdatesUser(
            GlobalRoles.Role oldRole, 
            GlobalRoles.Role newRole, 
            GlobalRoles.Role expectedUpdatedRole)
        {
            var oldCreatedById = Guid.NewGuid();
            var newCreatedById = Guid.NewGuid();
            var oldCreatedDate = DateTimeOffset.UtcNow.AddDays(-2);
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(-1);

            var user = _dataFixture.DefaultUserWithPendingInvite()
                .WithRoleId(oldRole.GetEnumValue())
                .WithCreatedById(oldCreatedById)
                .WithCreated(oldCreatedDate)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                var result = await repository.CreateOrUpdate(
                    email: user.Email,
                    role: newRole,
                    createdById: newCreatedById,
                    createdDate: newCreatedDate);

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, result.Id);
                Assert.Equal(user.Email, result.Email);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
                Assert.False(result.Active);
                Assert.Equal(oldCreatedById, result.CreatedById);
                Assert.Equal(oldCreatedDate, result.Created);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);

                // The role should only be updated if the new one outranks (or equals) the existing one
                Assert.Equal(expectedUpdatedRole.GetEnumValue(), result.RoleId);

            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users
                    .SingleAsync(u => u.Id == user.Id);

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, updatedUser.Id);
                Assert.Equal(user.Email, updatedUser.Email);
                Assert.Null(updatedUser.FirstName);
                Assert.Null(updatedUser.LastName);
                Assert.False(updatedUser.Active);
                Assert.Equal(oldCreatedById, updatedUser.CreatedById);
                Assert.Equal(oldCreatedDate, updatedUser.Created);
                Assert.Null(updatedUser.SoftDeleted);
                Assert.Null(updatedUser.DeletedById);

                // The role should only be updated if the new one outranks (or equals) the existing one
                Assert.Equal(expectedUpdatedRole.GetEnumValue(), updatedUser.RoleId);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_CreatedDateNotSupplied_LeavesUserUntouched()
        {
            var oldCreatedDate = DateTimeOffset.UtcNow.AddDays(-2);

            var user = _dataFixture.DefaultUserWithPendingInvite()
                .WithCreated(oldCreatedDate)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                var result = await repository.CreateOrUpdate(
                    email: user.Email,
                    role: GlobalRoles.Role.Analyst,
                    createdById: Guid.NewGuid());

                result.Created.AssertEqual(oldCreatedDate);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users
                    .SingleAsync(u => u.Id == user.Id);

                updatedUser.Created.AssertEqual(oldCreatedDate);
            }
        }

        [Theory]
        [InlineData(GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.PrereleaseUser)]
        [InlineData(GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst)]
        [InlineData(GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.Analyst, GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.PrereleaseUser)]
        [InlineData(GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst)]
        [InlineData(GlobalRoles.Role.Analyst, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.PrereleaseUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        public async Task UserWithExpiredInvite_UpdatesUser(
            GlobalRoles.Role oldRole,
            GlobalRoles.Role newRole,
            GlobalRoles.Role expectedUpdatedRole)
        {
            var oldCreatedById = Guid.NewGuid();
            var newCreatedById = Guid.NewGuid();
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(-1);

            var user = _dataFixture.DefaultUserWithExpiredInvite()
                .WithRoleId(oldRole.GetEnumValue())
                .WithCreatedById(oldCreatedById)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                var result = await repository.CreateOrUpdate(
                    email: user.Email,
                    role: newRole,
                    createdById: newCreatedById,
                    createdDate: newCreatedDate);

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, result.Id);
                Assert.Equal(user.Email, result.Email);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
                Assert.False(result.Active);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);

                // These fields should always be updated for expired invites being re-invited
                Assert.Equal(expectedUpdatedRole.GetEnumValue(), result.RoleId);
                Assert.Equal(newCreatedById, result.CreatedById);
                Assert.Equal(newCreatedDate, result.Created);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users
                    .SingleAsync(u => u.Id == user.Id);

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, updatedUser.Id);
                Assert.Equal(user.Email, updatedUser.Email);
                Assert.Null(updatedUser.FirstName);
                Assert.Null(updatedUser.LastName);
                Assert.False(updatedUser.Active);
                Assert.Null(updatedUser.SoftDeleted);
                Assert.Null(updatedUser.DeletedById);

                // These fields should always be updated for expired invites being re-invited
                Assert.Equal(expectedUpdatedRole.GetEnumValue(), updatedUser.RoleId);
                Assert.Equal(newCreatedById, updatedUser.CreatedById);
                Assert.Equal(newCreatedDate, updatedUser.Created);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_CreatedDateNotSupplied_UpdatesUserCreatedDateToNow()
        {
            var user = _dataFixture.DefaultUserWithExpiredInvite()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                var result = await repository.CreateOrUpdate(
                    email: user.Email,
                    role: GlobalRoles.Role.Analyst,
                    createdById: Guid.NewGuid());

                result.Created.AssertUtcNow();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users
                    .SingleAsync(u => u.Id == user.Id);

                updatedUser.Created.AssertUtcNow();
            }
        }

        [Theory]
        [InlineData(GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.PrereleaseUser)]
        [InlineData(GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst)]
        [InlineData(GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.Analyst, GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.PrereleaseUser)]
        [InlineData(GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst)]
        [InlineData(GlobalRoles.Role.Analyst, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.PrereleaseUser, GlobalRoles.Role.PrereleaseUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.Analyst, GlobalRoles.Role.Analyst)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        public async Task SoftDeletedUser_UpdatesUser(
            GlobalRoles.Role oldRole,
            GlobalRoles.Role newRole,
            GlobalRoles.Role expectedUpdatedRole)
        {
            var oldCreatedById = Guid.NewGuid();
            var newCreatedById = Guid.NewGuid();
            var oldCreatedDate = DateTimeOffset.UtcNow.AddDays(-2);
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(-1);

            var user = _dataFixture.DefaultSoftDeletedUser()
                .WithRoleId(oldRole.GetEnumValue())
                .WithCreatedById(oldCreatedById)
                .WithCreated(oldCreatedDate)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                var result = await repository.CreateOrUpdate(
                    email: user.Email,
                    role: newRole,
                    createdById: newCreatedById,
                    createdDate: newCreatedDate);

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, result.Id);
                Assert.Equal(user.Email, result.Email);
                Assert.False(result.Active);
 
                // These fields should always be updated for soft-deleted users being re-invited
                Assert.Equal(expectedUpdatedRole.GetEnumValue(), result.RoleId);
                Assert.Equal(newCreatedById, result.CreatedById);
                Assert.Equal(newCreatedDate, result.Created);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users
                    .SingleAsync(u => u.Id == user.Id);

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, updatedUser.Id);
                Assert.Equal(user.Email, updatedUser.Email);
                Assert.False(updatedUser.Active);

                // These fields should always be updated for soft-deleted users being re-invited
                Assert.Equal(expectedUpdatedRole.GetEnumValue(), updatedUser.RoleId);
                Assert.Equal(newCreatedById, updatedUser.CreatedById);
                Assert.Equal(newCreatedDate, updatedUser.Created);
                Assert.Null(updatedUser.SoftDeleted);
                Assert.Null(updatedUser.DeletedById);
                Assert.Null(updatedUser.FirstName);
                Assert.Null(updatedUser.LastName);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_CreatedDateNotSupplied_UpdatesUserCreatedDateToNow()
        {
            var oldCreatedDate = DateTimeOffset.UtcNow.AddDays(-2);

            var user = _dataFixture.DefaultSoftDeletedUser()
                .WithCreated(oldCreatedDate)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                var result = await repository.CreateOrUpdate(
                    email: user.Email,
                    role: GlobalRoles.Role.Analyst,
                    createdById: Guid.NewGuid());

                result.Created.AssertUtcNow();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users
                    .SingleAsync(u => u.Id == user.Id);

                updatedUser.Created.AssertUtcNow();
            }
        }

        [Fact]
        public async Task UserDoesNotExist_CreatesNewUser()
        {
            var email = "TEST@test.com";
            var createdById = Guid.NewGuid();
            var createdDate = DateTimeOffset.UtcNow.AddDays(-1);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                var result = await repository.CreateOrUpdate(
                    email: email,
                    role: GlobalRoles.Role.Analyst,
                    createdById: createdById,
                    createdDate: createdDate);

                Assert.Equal(email.ToLower(), result.Email);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
                Assert.False(result.Active);
                Assert.Equal(GlobalRoles.Role.Analyst.GetEnumValue(), result.RoleId);
                Assert.Equal(createdById, result.CreatedById);
                Assert.Equal(createdDate, result.Created);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var newUser = await contentDbContext.Users
                    .SingleAsync();

                Assert.Equal(email.ToLower(), newUser.Email);
                Assert.Null(newUser.FirstName);
                Assert.Null(newUser.LastName);
                Assert.False(newUser.Active);
                Assert.Equal(GlobalRoles.Role.Analyst.GetEnumValue(), newUser.RoleId);
                Assert.Equal(createdById, newUser.CreatedById);
                Assert.Equal(createdDate, newUser.Created);
                Assert.Null(newUser.SoftDeleted);
                Assert.Null(newUser.DeletedById);
            }
        }

        [Fact]
        public async Task SuppliedCreatedDateInFuture_ThrowsException()
        {
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(1);

            var user = _dataFixture.DefaultUserWithPendingInvite()
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Users.AddAsync(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                await Assert.ThrowsAsync<ArgumentException>(async ()
                    => await repository.CreateOrUpdate(
                        email: user.Email,
                        role: GlobalRoles.Role.Analyst,
                        createdById: Guid.NewGuid(),
                        createdDate: newCreatedDate));
            }
        }

        [Fact]
        public async Task UserDoesNotExist_CreatedDateNotSupplied_CreatesNewUserWithCreatedDateNow()
        {
            var email = "test@test.com";
            var createdById = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = SetupUserRepository(contentDbContext);

                var result = await repository.CreateOrUpdate(
                    email: email,
                    role: GlobalRoles.Role.Analyst,
                    createdById: createdById);

                result.Created.AssertUtcNow();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var newUser = await contentDbContext.Users
                    .SingleAsync();

                newUser.Created.AssertUtcNow();
            }
        }
    }

    private static UserRepository SetupUserRepository(
        ContentDbContext contentDbContext)
    {
        return new(contentDbContext);
    }
}
