using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseRedirectGeneratorExtensions
{
    public static Generator<ReleaseRedirect> DefaultReleaseRedirect(this DataFixture fixture) =>
        fixture.Generator<ReleaseRedirect>().WithDefaults();

    public static Generator<ReleaseRedirect> WithDefaults(
        this Generator<ReleaseRedirect> generator
    ) => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<ReleaseRedirect> SetDefaults(
        this InstanceSetters<ReleaseRedirect> setters
    ) => setters.SetDefault(p => p.Slug).SetDefault(p => p.Created);

    public static Generator<ReleaseRedirect> WithSlug(
        this Generator<ReleaseRedirect> generator,
        string slug
    ) => generator.ForInstance(s => s.SetSlug(slug));

    public static Generator<ReleaseRedirect> WithRelease(
        this Generator<ReleaseRedirect> generator,
        Release release
    ) => generator.ForInstance(s => s.SetRelease(release));

    public static Generator<ReleaseRedirect> WithReleaseId(
        this Generator<ReleaseRedirect> generator,
        Guid releaseId
    ) => generator.ForInstance(s => s.SetReleaseId(releaseId));

    public static InstanceSetters<ReleaseRedirect> SetSlug(
        this InstanceSetters<ReleaseRedirect> setters,
        string slug
    ) => setters.Set(rr => rr.Slug, slug);

    public static InstanceSetters<ReleaseRedirect> SetRelease(
        this InstanceSetters<ReleaseRedirect> setters,
        Release release
    ) => setters.Set(rr => rr.Release, release).SetReleaseId(release.Id);

    public static InstanceSetters<ReleaseRedirect> SetReleaseId(
        this InstanceSetters<ReleaseRedirect> setters,
        Guid releaseId
    ) => setters.Set(rr => rr.ReleaseId, releaseId);
}
