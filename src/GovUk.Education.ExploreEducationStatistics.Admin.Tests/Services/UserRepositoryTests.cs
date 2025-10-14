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

                // While this test could fail using the in-memory db if the comparison was accidentally case sensitive,
                // it wouldn't fail using SqlServer which is using a case insensitive collation by default.
                // C# equality is translated directly to SQL equality, which won't be case-sensitive with our config.
                // EF 5.0 will allow setting a different collation making this test worth while.
                // See https://docs.microsoft.com/en-us/ef/core/miscellaneous/collations-and-case-sensitivity
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

    public class FindByEmailTests : UserRepositoryTests
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
                var result = await repository.FindByEmail(user.Email);
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
                var result = await repository.FindByEmail(user.Email);
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
                var result = await repository.FindByEmail(user.Email);
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
                var result = await repository.FindByEmail("TEST@TEST.COM");

                // While this test could fail using the in-memory db if the comparison was accidentally case sensitive,
                // it wouldn't fail using SqlServer which is using a case insensitive collation by default.
                // C# equality is translated directly to SQL equality, which won't be case-sensitive with our config.
                // EF 5.0 will allow setting a different collation making this test worth while.
                // See https://docs.microsoft.com/en-us/ef/core/miscellaneous/collations-and-case-sensitivity
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
                var result = await repository.FindByEmail(user.Email);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = SetupUserRepository(contentDbContext);
            var result = await repository.FindByEmail("test@test.com");
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

        [Fact]
        public async Task UserWithPendingInvite_UpdatesUser()
        {
            var oldCreatedById = Guid.NewGuid();
            var newCreatedById = Guid.NewGuid();
            var oldCreatedDate = DateTimeOffset.UtcNow.AddDays(-2);
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(-1);

            var user = _dataFixture.DefaultUserWithPendingInvite()
                .WithRoleId(GlobalRoles.Role.PrereleaseUser.GetEnumValue())
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
                    role: GlobalRoles.Role.Analyst,
                    createdById: newCreatedById,
                    createdDate: newCreatedDate);

                Assert.Equal(user.Id, result.Id);
                Assert.Equal(user.Email, result.Email);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
                Assert.False(result.Active);
                Assert.Equal(GlobalRoles.Role.Analyst.GetEnumValue(), result.RoleId);
                Assert.Equal(newCreatedById, result.CreatedById);
                Assert.Equal(newCreatedDate, result.Created);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users
                    .SingleAsync(u => u.Id == user.Id);

                Assert.Equal(user.Id, updatedUser.Id);
                Assert.Equal(user.Email, updatedUser.Email);
                Assert.Null(updatedUser.FirstName);
                Assert.Null(updatedUser.LastName);
                Assert.False(updatedUser.Active);
                Assert.Equal(GlobalRoles.Role.Analyst.GetEnumValue(), updatedUser.RoleId);
                Assert.Equal(newCreatedById, updatedUser.CreatedById);
                Assert.Equal(newCreatedDate, updatedUser.Created);
                Assert.Null(updatedUser.SoftDeleted);
                Assert.Null(updatedUser.DeletedById);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_UpdatesUser()
        {
            var oldCreatedById = Guid.NewGuid();
            var newCreatedById = Guid.NewGuid();
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(-1);

            var user = _dataFixture.DefaultUserWithExpiredInvite()
                .WithRoleId(GlobalRoles.Role.PrereleaseUser.GetEnumValue())
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
                    role: GlobalRoles.Role.Analyst,
                    createdById: newCreatedById,
                    createdDate: newCreatedDate);

                Assert.Equal(user.Id, result.Id);
                Assert.Equal(user.Email, result.Email);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
                Assert.False(result.Active);
                Assert.Equal(GlobalRoles.Role.Analyst.GetEnumValue(), result.RoleId);
                Assert.Equal(newCreatedById, result.CreatedById);
                Assert.Equal(newCreatedDate, result.Created);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users
                    .SingleAsync(u => u.Id == user.Id);

                Assert.Equal(user.Id, updatedUser.Id);
                Assert.Equal(user.Email, updatedUser.Email);
                Assert.Null(updatedUser.FirstName);
                Assert.Null(updatedUser.LastName);
                Assert.False(updatedUser.Active);
                Assert.Equal(GlobalRoles.Role.Analyst.GetEnumValue(), updatedUser.RoleId);
                Assert.Equal(newCreatedById, updatedUser.CreatedById);
                Assert.Equal(newCreatedDate, updatedUser.Created);
                Assert.Null(updatedUser.SoftDeleted);
                Assert.Null(updatedUser.DeletedById);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_ReturnsNull()
        {
            var oldCreatedById = Guid.NewGuid();
            var newCreatedById = Guid.NewGuid();
            var oldCreatedDate = DateTimeOffset.UtcNow.AddDays(-2);
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(-1);

            var user = _dataFixture.DefaultSoftDeletedUser()
                .WithRoleId(GlobalRoles.Role.PrereleaseUser.GetEnumValue())
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
                    role: GlobalRoles.Role.Analyst,
                    createdById: newCreatedById,
                    createdDate: newCreatedDate);

                Assert.Equal(user.Id, result.Id);
                Assert.Equal(user.Email, result.Email);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
                Assert.False(result.Active);
                Assert.Equal(GlobalRoles.Role.Analyst.GetEnumValue(), result.RoleId);
                Assert.Equal(newCreatedById, result.CreatedById);
                Assert.Equal(newCreatedDate, result.Created);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users
                    .SingleAsync(u => u.Id == user.Id);

                Assert.Equal(user.Id, updatedUser.Id);
                Assert.Equal(user.Email, updatedUser.Email);
                Assert.Null(updatedUser.FirstName);
                Assert.Null(updatedUser.LastName);
                Assert.False(updatedUser.Active);
                Assert.Equal(GlobalRoles.Role.Analyst.GetEnumValue(), updatedUser.RoleId);
                Assert.Equal(newCreatedById, updatedUser.CreatedById);
                Assert.Equal(newCreatedDate, updatedUser.Created);
                Assert.Null(updatedUser.SoftDeleted);
                Assert.Null(updatedUser.DeletedById);
            }
        }

        [Fact]
        public async Task CreatedDateNotSupplied_UpdatesUserCreatedDateToNow()
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
