#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UserReleaseRoleGeneratorExtensions
{
    public static Generator<UserReleaseRole> DefaultUserReleaseRole(this DataFixture fixture)
        => fixture.Generator<UserReleaseRole>().WithDefaults();

    public static Generator<UserReleaseRole> WithDefaults(this Generator<UserReleaseRole> generator)
        => generator.ForInstance(d => d.SetDefaults());
    
    public static Generator<UserReleaseRole> WithRelease(this Generator<UserReleaseRole> generator, Release release)
        => generator.ForInstance(d => d.SetRelease(release));
    
    public static Generator<UserReleaseRole> WithReleases(this Generator<UserReleaseRole> generator, IEnumerable<Release> releases)
    {
        releases.ForEach((release, index) => 
            generator.ForIndex(index, s => s.SetRelease(release)));
        
        return generator;    
    }
    
    public static Generator<UserReleaseRole> WithUser(this Generator<UserReleaseRole> generator, User user)
        => generator.ForInstance(d => d.SetUser(user));

    public static Generator<UserReleaseRole> WithRole(this Generator<UserReleaseRole> generator, ReleaseRole role)
        => generator.ForInstance(d => d.SetRole(role));

    public static Generator<UserReleaseRole> WithRoles(this Generator<UserReleaseRole> generator, IEnumerable<ReleaseRole> roles)
    {
        roles.ForEach((role, index) => 
            generator.ForIndex(index, s => s.SetRole(role)));
        
        return generator;    
    }
    
    public static InstanceSetters<UserReleaseRole> SetDefaults(this InstanceSetters<UserReleaseRole> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.ReleaseId)
            .SetDefault(p => p.UserId);
    
    public static InstanceSetters<UserReleaseRole> SetRelease(
        this InstanceSetters<UserReleaseRole> setters,
        Release release)
        => setters.Set(d => d.Release, release);
    
    public static InstanceSetters<UserReleaseRole> SetUser(
        this InstanceSetters<UserReleaseRole> setters,
        User user)
        => setters.Set(d => d.User, user);
    
    public static InstanceSetters<UserReleaseRole> SetRole(
        this InstanceSetters<UserReleaseRole> setters,
        ReleaseRole role)
        => setters.Set(d => d.Role, role);
}
