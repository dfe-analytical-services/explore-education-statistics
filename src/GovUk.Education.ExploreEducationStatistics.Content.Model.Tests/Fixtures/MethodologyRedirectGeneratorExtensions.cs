using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class MethodologyRedirectGeneratorExtensions
{
    public static Generator<MethodologyRedirect> DefaultMethodologyRedirect(this DataFixture fixture)
        => fixture.Generator<MethodologyRedirect>().WithDefaults();

    public static Generator<MethodologyRedirect> WithDefaults(this Generator<MethodologyRedirect> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<MethodologyRedirect> SetDefaults(this InstanceSetters<MethodologyRedirect> setters)
        => setters
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Created);

    public static Generator<MethodologyRedirect> WithSlug(
        this Generator<MethodologyRedirect> generator,
        string slug)
        => generator.ForInstance(s => s.SetSlug(slug));

    public static Generator<MethodologyRedirect> WithMethodologyVersion(
        this Generator<MethodologyRedirect> generator,
        MethodologyVersion methodologyVersion)
        => generator.ForInstance(s => s.SetMethodologyVersion(methodologyVersion));

    public static Generator<MethodologyRedirect> WithMethodologyVersionId(
        this Generator<MethodologyRedirect> generator,
        Guid methodologyVersionId)
        => generator.ForInstance(s => s.SetMethodologyVersionId(methodologyVersionId));

    public static InstanceSetters<MethodologyRedirect> SetSlug(
        this InstanceSetters<MethodologyRedirect> setters,
        string slug)
        => setters.Set(mr => mr.Slug, slug);

    public static InstanceSetters<MethodologyRedirect> SetMethodologyVersion(
        this InstanceSetters<MethodologyRedirect> setters,
        MethodologyVersion methodologyVersion)
        => setters.Set(mr => mr.MethodologyVersion, methodologyVersion)
            .SetMethodologyVersionId(methodologyVersion.Id);

    public static InstanceSetters<MethodologyRedirect> SetMethodologyVersionId(
        this InstanceSetters<MethodologyRedirect> setters,
        Guid methodologyVersionId)
        => setters.Set(mr => mr.MethodologyVersionId, methodologyVersionId);
}
