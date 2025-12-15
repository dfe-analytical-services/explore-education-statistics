using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class UserPublicationRoleGeneratorExtensions
{
    public static Generator<UserPublicationRole> DefaultUserPublicationRole(this DataFixture fixture) =>
        fixture.Generator<UserPublicationRole>().WithDefaults();

    public static Generator<UserPublicationRole> WithDefaults(this Generator<UserPublicationRole> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<UserPublicationRole> WithPublication(
        this Generator<UserPublicationRole> generator,
        Publication publication
    ) => generator.ForInstance(s => s.SetPublication(publication));

    public static Generator<UserPublicationRole> WithPublicationId(
        this Generator<UserPublicationRole> generator,
        Guid publicationId
    ) => generator.ForInstance(s => s.SetPublicationId(publicationId));

    public static Generator<UserPublicationRole> WithPublications(
        this Generator<UserPublicationRole> generator,
        IEnumerable<Publication> publications
    )
    {
        publications.ForEach((publication, index) => generator.ForIndex(index, s => s.SetPublication(publication)));

        return generator;
    }

    public static Generator<UserPublicationRole> WithUser(this Generator<UserPublicationRole> generator, User user) =>
        generator.ForInstance(s => s.SetUser(user));

    public static Generator<UserPublicationRole> WithRole(
        this Generator<UserPublicationRole> generator,
        PublicationRole role
    ) => generator.ForInstance(s => s.SetRole(role));

    public static Generator<UserPublicationRole> WithRoles(
        this Generator<UserPublicationRole> generator,
        IEnumerable<PublicationRole> roles
    )
    {
        roles.ForEach((role, index) => generator.ForIndex(index, s => s.SetRole(role)));

        return generator;
    }

    public static InstanceSetters<UserPublicationRole> SetDefaults(this InstanceSetters<UserPublicationRole> setters) =>
        setters.SetDefault(upr => upr.Id).SetDefault(upr => upr.PublicationId).SetDefault(upr => upr.UserId);

    public static InstanceSetters<UserPublicationRole> SetPublication(
        this InstanceSetters<UserPublicationRole> setters,
        Publication publication
    ) => setters.Set(upr => upr.Publication, publication).SetPublicationId(publication.Id);

    public static InstanceSetters<UserPublicationRole> SetPublicationId(
        this InstanceSetters<UserPublicationRole> setters,
        Guid publicationId
    ) => setters.Set(upr => upr.PublicationId, publicationId);

    public static InstanceSetters<UserPublicationRole> SetUser(
        this InstanceSetters<UserPublicationRole> setters,
        User user
    ) => setters.Set(upr => upr.User, user).Set(upr => upr.UserId, user.Id);

    public static InstanceSetters<UserPublicationRole> SetRole(
        this InstanceSetters<UserPublicationRole> setters,
        PublicationRole role
    ) => setters.Set(upr => upr.Role, role);
}
