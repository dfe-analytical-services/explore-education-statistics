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

    public static InstanceSetters<UserReleaseRole> SetDefaults(this InstanceSetters<UserReleaseRole> setters) =>
        setters.SetDefault(urr => urr.Id).SetDefault(urr => urr.ReleaseVersionId).SetDefault(urr => urr.UserId);

    public static InstanceSetters<UserReleaseRole> SetReleaseVersion(
        this InstanceSetters<UserReleaseRole> setters,
        ReleaseVersion releaseVersion
    ) => setters.Set(urr => urr.ReleaseVersion, releaseVersion).Set(urr => urr.ReleaseVersionId, releaseVersion.Id);

    public static InstanceSetters<UserReleaseRole> SetUser(this InstanceSetters<UserReleaseRole> setters, User user) =>
        setters.Set(urr => urr.User, user).Set(urr => urr.UserId, user.Id);

    public static InstanceSetters<UserReleaseRole> SetRole(
        this InstanceSetters<UserReleaseRole> setters,
        ReleaseRole role
    ) => setters.Set(urr => urr.Role, role);
}
