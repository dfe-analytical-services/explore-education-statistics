using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UserReleaseRoleGeneratorExtensions
{
    public static Generator<UserReleaseRole> DefaultUserReleaseRole(this DataFixture fixture)
        => fixture.Generator<UserReleaseRole>().WithDefaults();

    public static Generator<UserReleaseRole> WithDefaults(this Generator<UserReleaseRole> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static Generator<UserReleaseRole> WithReleaseVersion(this Generator<UserReleaseRole> generator,
        ReleaseVersion releaseVersion)
        => generator.ForInstance(d => d.SetReleaseVersion(releaseVersion));

    public static Generator<UserReleaseRole> WithReleaseVersions(this Generator<UserReleaseRole> generator,
        IEnumerable<ReleaseVersion> releaseVersions)
    {
        releaseVersions.ForEach((releaseVersion, index) =>
            generator.ForIndex(index, s => s.SetReleaseVersion(releaseVersion)));

        return generator;
    }

    public static Generator<UserReleaseRole> WithUser(this Generator<UserReleaseRole> generator, User user)
        => generator.ForInstance(d => d.SetUser(user));

    public static Generator<UserReleaseRole> WithRole(this Generator<UserReleaseRole> generator, ReleaseRole role)
        => generator.ForInstance(d => d.SetRole(role));

    public static Generator<UserReleaseRole> WithRoles(this Generator<UserReleaseRole> generator,
        IEnumerable<ReleaseRole> roles)
    {
        roles.ForEach((role, index) =>
            generator.ForIndex(index, s => s.SetRole(role)));

        return generator;
    }

    public static InstanceSetters<UserReleaseRole> SetDefaults(this InstanceSetters<UserReleaseRole> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.ReleaseVersionId)
            .SetDefault(p => p.UserId);

    public static InstanceSetters<UserReleaseRole> SetReleaseVersion(
        this InstanceSetters<UserReleaseRole> setters,
        ReleaseVersion releaseVersion)
        => setters.Set(d => d.ReleaseVersion, releaseVersion);

    public static InstanceSetters<UserReleaseRole> SetUser(
        this InstanceSetters<UserReleaseRole> setters,
        User user)
        => setters.Set(d => d.User, user);

    public static InstanceSetters<UserReleaseRole> SetRole(
        this InstanceSetters<UserReleaseRole> setters,
        ReleaseRole role)
        => setters.Set(d => d.Role, role);
}
