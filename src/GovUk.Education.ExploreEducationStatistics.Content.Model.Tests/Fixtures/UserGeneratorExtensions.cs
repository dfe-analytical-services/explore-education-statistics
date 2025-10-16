using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UserGeneratorExtensions
{
    public static Generator<User> DefaultUser(this DataFixture fixture) => fixture.Generator<User>().WithDefaults();

    public static Generator<User> DefaultUserWithPendingInvite(this DataFixture fixture) =>
        fixture.DefaultUser().WithFirstName(null).WithLastName(null).WithActive(false);

    public static Generator<User> DefaultUserWithExpiredInvite(this DataFixture fixture) =>
        fixture
            .DefaultUserWithPendingInvite()
            .WithCreated(DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1));

    public static Generator<User> DefaultSoftDeletedUser(this DataFixture fixture) =>
        fixture.DefaultUser().WithActive(false).WithSoftDeleted(DateTime.UtcNow).WithDeletedById(Guid.NewGuid());

    public static Generator<User> WithDefaults(this Generator<User> generator) =>
        generator.ForInstance(d => d.SetDefaults());

    public static Generator<User> WithId(this Generator<User> generator, Guid id) =>
        generator.ForInstance(d => d.SetId(id));

    public static Generator<User> WithFirstName(this Generator<User> generator, string? firstName) =>
        generator.ForInstance(d => d.SetFirstName(firstName));

    public static Generator<User> WithLastName(this Generator<User> generator, string? lastName) =>
        generator.ForInstance(d => d.SetLastName(lastName));

    public static Generator<User> WithEmail(this Generator<User> generator, string email) =>
        generator.ForInstance(d => d.SetEmail(email));

    public static Generator<User> WithSoftDeleted(this Generator<User> generator, DateTime? softDeletedDate) =>
        generator.ForInstance(d => d.SetSoftDeleted(softDeletedDate));

    public static Generator<User> WithDeletedById(this Generator<User> generator, Guid? deletedById) =>
        generator.ForInstance(d => d.SetDeletedById(deletedById));

    public static Generator<User> WithDeletedBy(this Generator<User> generator, User? deletedBy) =>
        generator.ForInstance(d => d.SetDeletedBy(deletedBy));

    public static Generator<User> WithActive(this Generator<User> generator, bool active) =>
        generator.ForInstance(d => d.SetActive(active));

    public static Generator<User> WithRoleId(this Generator<User> generator, string roleId) =>
        generator.ForInstance(d => d.SetRoleId(roleId));

    public static Generator<User> WithRole(this Generator<User> generator, IdentityRole role) =>
        generator.ForInstance(d => d.SetRole(role));

    public static Generator<User> WithCreated(this Generator<User> generator, DateTimeOffset created) =>
        generator.ForInstance(d => d.SetCreated(created));

    public static Generator<User> WithCreatedById(this Generator<User> generator, Guid createdById) =>
        generator.ForInstance(d => d.SetCreatedById(createdById));

    public static Generator<User> WithCreatedBy(this Generator<User> generator, User createdBy) =>
        generator.ForInstance(d => d.SetCreatedBy(createdBy));

    public static InstanceSetters<User> SetDefaults(this InstanceSetters<User> setters) =>
        setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.FirstName)
            .SetDefault(p => p.LastName)
            .Set(p => p.Email, f => f.Internet.Email().ToLower())
            .Set(p => p.Active, true)
            .Set(p => p.RoleId, "f9ddb43e-aa9e-41ed-837d-3062e130c425") // Default to "Analyst" Global Role
            .Set(u => u.Created, DateTimeOffset.UtcNow)
            .SetDefault(p => p.CreatedById);

    public static InstanceSetters<User> SetId(this InstanceSetters<User> setters, Guid id) =>
        setters.Set(d => d.Id, id);

    public static InstanceSetters<User> SetFirstName(this InstanceSetters<User> setters, string? firstName) =>
        setters.Set(d => d.FirstName, firstName);

    public static InstanceSetters<User> SetLastName(this InstanceSetters<User> setters, string? lastName) =>
        setters.Set(d => d.LastName, lastName);

    public static InstanceSetters<User> SetEmail(this InstanceSetters<User> setters, string email) =>
        setters.Set(d => d.Email, email);

    public static InstanceSetters<User> SetSoftDeleted(this InstanceSetters<User> setters, DateTime? softDeletedDate) =>
        setters.Set(d => d.SoftDeleted, softDeletedDate);

    public static InstanceSetters<User> SetDeletedById(this InstanceSetters<User> setters, Guid? deletedById) =>
        setters.Set(d => d.DeletedById, deletedById);

    public static InstanceSetters<User> SetDeletedBy(this InstanceSetters<User> setters, User? deletedBy) =>
        setters.Set(d => d.DeletedBy, deletedBy).Set(d => d.DeletedById, deletedBy?.Id);

    public static InstanceSetters<User> SetActive(this InstanceSetters<User> setters, bool active) =>
        setters.Set(d => d.Active, active);

    public static InstanceSetters<User> SetRoleId(this InstanceSetters<User> setters, string roleId) =>
        setters.Set(d => d.RoleId, roleId);

    public static InstanceSetters<User> SetRole(this InstanceSetters<User> setters, IdentityRole role) =>
        setters.Set(d => d.Role, role).Set(uri => uri.RoleId, role.Id);

    public static InstanceSetters<User> SetCreated(this InstanceSetters<User> setters, DateTimeOffset created) =>
        setters.Set(d => d.Created, created);

    public static InstanceSetters<User> SetCreatedById(this InstanceSetters<User> setters, Guid createdById) =>
        setters.Set(d => d.CreatedById, createdById);

    public static InstanceSetters<User> SetCreatedBy(this InstanceSetters<User> setters, User createdBy) =>
        setters.Set(d => d.CreatedBy, createdBy).Set(uri => uri.CreatedById, createdBy.Id);
}
