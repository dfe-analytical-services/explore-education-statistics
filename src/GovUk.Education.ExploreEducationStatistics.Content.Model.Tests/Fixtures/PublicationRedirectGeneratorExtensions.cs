using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class PublicationRedirectGeneratorExtensions
{
    public static Generator<PublicationRedirect> DefaultPublicationRedirect(this DataFixture fixture)
        => fixture.Generator<PublicationRedirect>().WithDefaults();

    public static Generator<PublicationRedirect> WithDefaults(this Generator<PublicationRedirect> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<PublicationRedirect> SetDefaults(this InstanceSetters<PublicationRedirect> setters)
        => setters
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Created);

    public static Generator<PublicationRedirect> WithSlug(
        this Generator<PublicationRedirect> generator,
        string slug)
        => generator.ForInstance(s => s.SetSlug(slug));

    public static Generator<PublicationRedirect> WithPublication(
        this Generator<PublicationRedirect> generator,
        Publication publication)
        => generator.ForInstance(s => s.SetPublication(publication));

    public static Generator<PublicationRedirect> WithPublicationId(
        this Generator<PublicationRedirect> generator,
        Guid publicationId)
        => generator.ForInstance(s => s.SetPublicationId(publicationId));

    public static InstanceSetters<PublicationRedirect> SetSlug(
        this InstanceSetters<PublicationRedirect> setters,
        string slug)
        => setters.Set(pr => pr.Slug, slug);

    public static InstanceSetters<PublicationRedirect> SetPublication(
        this InstanceSetters<PublicationRedirect> setters,
        Publication publication)
        => setters.Set(pr => pr.Publication, publication)
            .SetPublicationId(publication.Id);

    public static InstanceSetters<PublicationRedirect> SetPublicationId(
        this InstanceSetters<PublicationRedirect> setters,
        Guid publicationId)
        => setters.Set(pr => pr.PublicationId, publicationId);
}
