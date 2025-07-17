using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UserPublicationInviteGeneratorExtensions
{
    public static Generator<UserPublicationInvite> DefaultUserPublicationInvite(this DataFixture fixture)
        => fixture.Generator<UserPublicationInvite>().WithDefaults();

    public static Generator<UserPublicationInvite> WithDefaults(this Generator<UserPublicationInvite> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<UserPublicationInvite> WithPublication(this Generator<UserPublicationInvite> generator,
        Publication publication)
        => generator.ForInstance(s => s.SetPublication(publication));

    public static Generator<UserPublicationInvite> WithRole(this Generator<UserPublicationInvite> generator, PublicationRole role)
        => generator.ForInstance(s => s.SetRole(role));

    public static Generator<UserPublicationInvite> WithRoles(this Generator<UserPublicationInvite> generator,
        IEnumerable<PublicationRole> roles)
    {
        roles.ForEach((role, index) =>
            generator.ForIndex(index, s => s.SetRole(role)));

        return generator;
    }

    public static Generator<UserPublicationInvite> WithEmail(
        this Generator<UserPublicationInvite> generator, 
        string email)
        => generator.ForInstance(s => s.SetEmail(email));

    public static Generator<UserPublicationInvite> WithCreatedById(
        this Generator<UserPublicationInvite> generator,
        Guid createdById)
        => generator.ForInstance(s => s.SetCreatedById(createdById));

    public static InstanceSetters<UserPublicationInvite> SetDefaults(this InstanceSetters<UserPublicationInvite> setters)
        => setters
            .SetDefault(uri => uri.Id)
            .SetDefault(uri => uri.PublicationId)
            .SetDefault(uri => uri.Email)
            .SetDefault(uri => uri.Role)
            .SetDefault(uri => uri.Created)
            .SetDefault(uri => uri.CreatedById);

    public static InstanceSetters<UserPublicationInvite> SetPublication(
        this InstanceSetters<UserPublicationInvite> setters,
        Publication publication)
        => setters
            .Set(uri => uri.Publication, publication)
            .Set(uri => uri.PublicationId, publication.Id);

    public static InstanceSetters<UserPublicationInvite> SetRole(
        this InstanceSetters<UserPublicationInvite> setters,
        PublicationRole role)
        => setters.Set(uri => uri.Role, role);

    public static InstanceSetters<UserPublicationInvite> SetEmail(
        this InstanceSetters<UserPublicationInvite> setters,
        string email)
        => setters.Set(uri => uri.Email, email);

    public static InstanceSetters<UserPublicationInvite> SetCreatedById(
        this InstanceSetters<UserPublicationInvite> setters,
        Guid createdById)
        => setters.Set(uri => uri.CreatedById, createdById);
}
