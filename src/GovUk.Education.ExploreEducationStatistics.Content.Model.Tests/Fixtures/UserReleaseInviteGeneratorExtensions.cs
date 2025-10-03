using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UserReleaseInviteGeneratorExtensions
{
    public static Generator<UserReleaseInvite> DefaultUserReleaseInvite(this DataFixture fixture) =>
        fixture.Generator<UserReleaseInvite>().WithDefaults();

    public static Generator<UserReleaseInvite> WithDefaults(this Generator<UserReleaseInvite> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<UserReleaseInvite> WithReleaseVersion(
        this Generator<UserReleaseInvite> generator,
        ReleaseVersion releaseVersion
    ) => generator.ForInstance(s => s.SetReleaseVersion(releaseVersion));

    public static Generator<UserReleaseInvite> WithRole(
        this Generator<UserReleaseInvite> generator,
        ReleaseRole role
    ) => generator.ForInstance(s => s.SetRole(role));

    public static Generator<UserReleaseInvite> WithRoles(
        this Generator<UserReleaseInvite> generator,
        IEnumerable<ReleaseRole> roles
    )
    {
        roles.ForEach((role, index) => generator.ForIndex(index, s => s.SetRole(role)));

        return generator;
    }

    public static Generator<UserReleaseInvite> WithEmail(this Generator<UserReleaseInvite> generator, string email) =>
        generator.ForInstance(s => s.SetEmail(email));

    public static Generator<UserReleaseInvite> WithEmailSent(
        this Generator<UserReleaseInvite> generator,
        bool emailSent
    ) => generator.ForInstance(s => s.SetEmailSent(emailSent));

    public static Generator<UserReleaseInvite> WithCreatedById(
        this Generator<UserReleaseInvite> generator,
        Guid createdById
    ) => generator.ForInstance(s => s.SetCreatedById(createdById));

    public static InstanceSetters<UserReleaseInvite> SetDefaults(this InstanceSetters<UserReleaseInvite> setters) =>
        setters
            .SetDefault(uri => uri.Id)
            .SetDefault(uri => uri.ReleaseVersionId)
            .SetDefault(uri => uri.Email)
            .SetDefault(uri => uri.Role)
            .SetDefault(uri => uri.Created)
            .SetDefault(uri => uri.Updated)
            .SetDefault(uri => uri.CreatedById);

    public static InstanceSetters<UserReleaseInvite> SetReleaseVersion(
        this InstanceSetters<UserReleaseInvite> setters,
        ReleaseVersion releaseVersion
    ) => setters.Set(uri => uri.ReleaseVersion, releaseVersion).Set(uri => uri.ReleaseVersionId, releaseVersion.Id);

    public static InstanceSetters<UserReleaseInvite> SetRole(
        this InstanceSetters<UserReleaseInvite> setters,
        ReleaseRole role
    ) => setters.Set(uri => uri.Role, role);

    public static InstanceSetters<UserReleaseInvite> SetEmail(
        this InstanceSetters<UserReleaseInvite> setters,
        string email
    ) => setters.Set(uri => uri.Email, email);

    public static InstanceSetters<UserReleaseInvite> SetEmailSent(
        this InstanceSetters<UserReleaseInvite> setters,
        bool emailSent
    ) => setters.Set(uri => uri.EmailSent, emailSent);

    public static InstanceSetters<UserReleaseInvite> SetCreatedById(
        this InstanceSetters<UserReleaseInvite> setters,
        Guid createdById
    ) => setters.Set(uri => uri.CreatedById, createdById);
}
