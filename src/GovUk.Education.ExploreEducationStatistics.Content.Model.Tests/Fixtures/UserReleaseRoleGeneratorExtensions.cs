using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UserReleaseRoleGeneratorExtensions
{
    public static Generator<UserReleaseRole> DefaultUserReleaseRole(this DataFixture fixture) =>
        fixture.Generator<UserReleaseRole>().WithDefaults();

    public static Generator<UserReleaseRole> WithDefaults(this Generator<UserReleaseRole> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<UserReleaseRole> WithReleaseVersion(
        this Generator<UserReleaseRole> generator,
        ReleaseVersion releaseVersion
    ) => generator.ForInstance(s => s.SetReleaseVersion(releaseVersion));

    public static Generator<UserReleaseRole> WithReleaseVersionId(
        this Generator<UserReleaseRole> generator,
        Guid releaseVersionId
    ) => generator.ForInstance(s => s.SetReleaseVersionId(releaseVersionId));

    public static Generator<UserReleaseRole> WithReleaseVersions(
        this Generator<UserReleaseRole> generator,
        IEnumerable<ReleaseVersion> releaseVersions
    )
    {
        releaseVersions.ForEach(
            (releaseVersion, index) => generator.ForIndex(index, s => s.SetReleaseVersion(releaseVersion))
        );

        return generator;
    }

    public static Generator<UserReleaseRole> WithUser(this Generator<UserReleaseRole> generator, User user) =>
        generator.ForInstance(s => s.SetUser(user));

    public static Generator<UserReleaseRole> WithUserId(this Generator<UserReleaseRole> generator, Guid userId) =>
        generator.ForInstance(s => s.SetUserId(userId));

    public static Generator<UserReleaseRole> WithRole(this Generator<UserReleaseRole> generator, ReleaseRole role) =>
        generator.ForInstance(s => s.SetRole(role));

    public static Generator<UserReleaseRole> WithRoles(
        this Generator<UserReleaseRole> generator,
        IEnumerable<ReleaseRole> roles
    )
    {
        roles.ForEach((role, index) => generator.ForIndex(index, s => s.SetRole(role)));

        return generator;
    }

    public static Generator<UserReleaseRole> WithCreated(this Generator<UserReleaseRole> generator, DateTime created) =>
        generator.ForInstance(s => s.SetCreated(created));

    public static Generator<UserReleaseRole> WithCreatedBy(this Generator<UserReleaseRole> generator, User createdBy) =>
        generator.ForInstance(s => s.SetCreatedBy(createdBy));

    public static Generator<UserReleaseRole> WithCreatedById(
        this Generator<UserReleaseRole> generator,
        Guid createdById
    ) => generator.ForInstance(s => s.SetCreatedById(createdById));

    public static Generator<UserReleaseRole> WithEmailSent(
        this Generator<UserReleaseRole> generator,
        DateTimeOffset emailSent
    ) => generator.ForInstance(s => s.SetEmailSent(emailSent));

    public static InstanceSetters<UserReleaseRole> SetDefaults(this InstanceSetters<UserReleaseRole> setters) =>
        setters
            .SetDefault(urr => urr.Id)
            .SetDefault(urr => urr.ReleaseVersionId)
            .SetDefault(urr => urr.UserId)
            .SetDefault(urr => urr.Created);

    public static InstanceSetters<UserReleaseRole> SetReleaseVersion(
        this InstanceSetters<UserReleaseRole> setters,
        ReleaseVersion releaseVersion
    ) => setters.Set(urr => urr.ReleaseVersion, releaseVersion).SetReleaseVersionId(releaseVersion.Id);

    public static InstanceSetters<UserReleaseRole> SetReleaseVersionId(
        this InstanceSetters<UserReleaseRole> setters,
        Guid releaseVersionId
    ) => setters.Set(urr => urr.ReleaseVersionId, releaseVersionId);

    public static InstanceSetters<UserReleaseRole> SetUser(this InstanceSetters<UserReleaseRole> setters, User user) =>
        setters.Set(urr => urr.User, user).SetUserId(user.Id);

    public static InstanceSetters<UserReleaseRole> SetUserId(
        this InstanceSetters<UserReleaseRole> setters,
        Guid userId
    ) => setters.Set(urr => urr.UserId, userId);

    public static InstanceSetters<UserReleaseRole> SetRole(
        this InstanceSetters<UserReleaseRole> setters,
        ReleaseRole role
    ) => setters.Set(urr => urr.Role, role);

    public static InstanceSetters<UserReleaseRole> SetCreated(
        this InstanceSetters<UserReleaseRole> setters,
        DateTime created
    ) => setters.Set(urr => urr.Created, created);

    public static InstanceSetters<UserReleaseRole> SetCreatedBy(
        this InstanceSetters<UserReleaseRole> setters,
        User createdBy
    ) => setters.Set(urr => urr.CreatedBy, createdBy).SetCreatedById(createdBy.Id);

    public static InstanceSetters<UserReleaseRole> SetCreatedById(
        this InstanceSetters<UserReleaseRole> setters,
        Guid createdById
    ) => setters.Set(urr => urr.CreatedById, createdById);

    public static InstanceSetters<UserReleaseRole> SetEmailSent(
        this InstanceSetters<UserReleaseRole> setters,
        DateTimeOffset emailSent
    ) => setters.Set(urr => urr.EmailSent, emailSent);
}
