using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UserGeneratorExtensions
{
    public static Generator<User> DefaultUser(this DataFixture fixture)
        => fixture.Generator<User>().WithDefaults();

    public static Generator<User> WithDefaults(this Generator<User> generator)
        => generator.ForInstance(i => i.SetDefaults());

    public static Generator<User> WithId(this Generator<User> generator, Guid id)
        => generator.ForInstance(i => i.SetId(id));

    public static Generator<User> WithFirstName(this Generator<User> generator, string firstName)
        => generator.ForInstance(i => i.SetFirstName(firstName));

    public static Generator<User> WithLastName(this Generator<User> generator, string lastName)
        => generator.ForInstance(i => i.SetLastName(lastName));

    public static Generator<User> WithEmail(this Generator<User> generator, string email)
        => generator.ForInstance(i => i.SetEmail(email));

    public static Generator<User> WithSoftDeleted(this Generator<User> generator, DateTime? softDeletedDate)
        => generator.ForInstance(i => i.SetSoftDeleted(softDeletedDate));

    public static Generator<User> WithDeletedById(this Generator<User> generator, Guid? deletedById)
        => generator.ForInstance(i => i.SetDeletedById(deletedById));

    public static Generator<User> WithDeletedBy(this Generator<User> generator, User? deletedBy)
        => generator.ForInstance(i => i.SetDeletedBy(deletedBy));

    public static Generator<User> WithActive(this Generator<User> generator, bool active)
        => generator.ForInstance(i => i.SetActive(active));

    public static Generator<User> WithRoleId(this Generator<User> generator, string roleId)
        => generator.ForInstance(i => i.SetRoleId(roleId));

    public static Generator<User> WithRole(this Generator<User> generator, IdentityRole role)
        => generator.ForInstance(i => i.SetRole(role));

    public static Generator<User> WithCreated(this Generator<User> generator, DateTimeOffset created)
        => generator.ForInstance(i => i.SetCreated(created));

    public static Generator<User> WithCreatedById(this Generator<User> generator, Guid createdById)
        => generator.ForInstance(i => i.SetCreatedById(createdById));

    public static Generator<User> WithCreatedBy(this Generator<User> generator, User createdBy)
        => generator.ForInstance(i => i.SetCreatedBy(createdBy));

    public static InstanceSetters<User> SetDefaults(this InstanceSetters<User> setters)
        => setters
            .SetDefault(u => u.Id)
            .SetDefault(u => u.FirstName)
            .SetDefault(u => u.LastName)
            .Set(u => u.Email, f => f.Internet.Email())
            .Set(u => u.Active, true)
            .SetDefault(u => u.RoleId)
            .Set(u => u.Created, f => f.Date.Past())
            .SetDefault(u => u.CreatedById);

    public static InstanceSetters<User> SetId(
        this InstanceSetters<User> setters,
        Guid id)
        => setters.Set(u => u.Id, id);

    public static InstanceSetters<User> SetFirstName(
        this InstanceSetters<User> setters,
        string firstName)
        => setters.Set(u => u.FirstName, firstName);

    public static InstanceSetters<User> SetLastName(
        this InstanceSetters<User> setters,
        string lastName)
        => setters.Set(u => u.LastName, lastName);

    public static InstanceSetters<User> SetEmail(
        this InstanceSetters<User> setters,
        string email)
        => setters.Set(u => u.Email, email);

    public static InstanceSetters<User> SetSoftDeleted(
        this InstanceSetters<User> setters,
        DateTime? softDeletedDate)
        => setters.Set(u => u.SoftDeleted, softDeletedDate);

    public static InstanceSetters<User> SetDeletedById(
        this InstanceSetters<User> setters,
        Guid? deletedById)
        => setters.Set(u => u.DeletedById, deletedById);

    public static InstanceSetters<User> SetDeletedBy(
        this InstanceSetters<User> setters,
        User? deletedBy)
        => setters
            .Set(u => u.DeletedBy, deletedBy)
            .Set(u => u.DeletedById, deletedBy?.Id);

    public static InstanceSetters<User> SetActive(
        this InstanceSetters<User> setters,
        bool active)
        => setters.Set(u => u.Active, active);

    public static InstanceSetters<User> SetRoleId(
        this InstanceSetters<User> setters,
        string roleId)
        => setters.Set(u => u.RoleId, roleId);

    public static InstanceSetters<User> SetRole(
        this InstanceSetters<User> setters,
        IdentityRole role)
        => setters
            .Set(u => u.Role, role)
            .Set(u => u.RoleId, role.Id);

    public static InstanceSetters<User> SetCreated(
        this InstanceSetters<User> setters,
        DateTimeOffset created)
        => setters.Set(u => u.Created, created);

    public static InstanceSetters<User> SetCreatedById(
        this InstanceSetters<User> setters,
        Guid createdById)
        => setters.Set(u => u.CreatedById, createdById);

    public static InstanceSetters<User> SetCreatedBy(
        this InstanceSetters<User> setters,
        User createdBy)
        => setters
            .Set(u => u.CreatedBy, createdBy)
            .Set(u => u.CreatedById, createdBy.Id);
}
