#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UserPublicationRoleGeneratorExtensions
{
    public static Generator<UserPublicationRole> DefaultUserPublicationRole(this DataFixture fixture)
        => fixture.Generator<UserPublicationRole>().WithDefaults();

    public static Generator<UserPublicationRole> WithDefaults(this Generator<UserPublicationRole> generator)
        => generator.ForInstance(d => d.SetDefaults());
    
    public static Generator<UserPublicationRole> WithPublication(this Generator<UserPublicationRole> generator, Publication Publication)
        => generator.ForInstance(d => d.SetPublication(Publication));
    
    public static Generator<UserPublicationRole> WithPublications(this Generator<UserPublicationRole> generator, IEnumerable<Publication> Publications)
    {
        Publications.ForEach((Publication, index) => 
            generator.ForIndex(index, s => s.SetPublication(Publication)));
        
        return generator;    
    }
    
    public static Generator<UserPublicationRole> WithUser(this Generator<UserPublicationRole> generator, User user)
        => generator.ForInstance(d => d.SetUser(user));

    public static Generator<UserPublicationRole> WithRole(this Generator<UserPublicationRole> generator, PublicationRole role)
        => generator.ForInstance(d => d.SetRole(role));

    public static Generator<UserPublicationRole> WithRoles(this Generator<UserPublicationRole> generator, IEnumerable<PublicationRole> roles)
    {
        roles.ForEach((role, index) => 
            generator.ForIndex(index, s => s.SetRole(role)));
        
        return generator;    
    }
    
    public static InstanceSetters<UserPublicationRole> SetDefaults(this InstanceSetters<UserPublicationRole> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.PublicationId)
            .SetDefault(p => p.UserId);
    
    public static InstanceSetters<UserPublicationRole> SetPublication(
        this InstanceSetters<UserPublicationRole> setters,
        Publication Publication)
        => setters.Set(d => d.Publication, Publication);
    
    public static InstanceSetters<UserPublicationRole> SetUser(
        this InstanceSetters<UserPublicationRole> setters,
        User user)
        => setters.Set(d => d.User, user);
    
    public static InstanceSetters<UserPublicationRole> SetRole(
        this InstanceSetters<UserPublicationRole> setters,
        PublicationRole role)
        => setters.Set(d => d.Role, role);
}
