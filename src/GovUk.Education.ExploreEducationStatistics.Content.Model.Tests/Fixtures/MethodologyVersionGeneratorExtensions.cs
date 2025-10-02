using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class MethodologyVersionGeneratorExtensions
{
    public static Generator<MethodologyVersion> DefaultMethodologyVersion(this DataFixture fixture) =>
        fixture.Generator<MethodologyVersion>().WithDefaults();

    public static Generator<MethodologyVersion> WithDefaults(this Generator<MethodologyVersion> generator) =>
        generator.ForInstance(d => d.SetDefaults());

    public static Generator<MethodologyVersion> WithAlternativeSlug(
        this Generator<MethodologyVersion> generator,
        string alternativeSlug
    ) => generator.ForInstance(d => d.SetAlternativeSlug(alternativeSlug));

    public static Generator<MethodologyVersion> WithAlternativeTitle(
        this Generator<MethodologyVersion> generator,
        string alternativeTitle
    ) => generator.ForInstance(d => d.SetAlternativeTitle(alternativeTitle));

    public static Generator<MethodologyVersion> WithApprovalStatus(
        this Generator<MethodologyVersion> generator,
        MethodologyApprovalStatus approvalStatus
    ) => generator.ForInstance(d => d.SetApprovalStatus(approvalStatus));

    public static Generator<MethodologyVersion> WithApprovalStatuses(
        this Generator<MethodologyVersion> generator,
        IEnumerable<MethodologyApprovalStatus> approvalStatuses
    )
    {
        approvalStatuses.ForEach((status, index) => generator.ForIndex(index, s => s.SetApprovalStatus(status)));

        return generator;
    }

    public static Generator<MethodologyVersion> WithPublished(
        this Generator<MethodologyVersion> generator,
        DateTime published
    ) => generator.ForInstance(d => d.SetPublished(published));

    public static Generator<MethodologyVersion> WithPublishingStrategy(
        this Generator<MethodologyVersion> generator,
        MethodologyPublishingStrategy publishingStrategy
    ) => generator.ForInstance(d => d.SetPublishingStrategy(publishingStrategy));

    public static Generator<MethodologyVersion> WithScheduledWithReleaseVersion(
        this Generator<MethodologyVersion> generator,
        ReleaseVersion releaseVersion
    ) => generator.ForInstance(d => d.SetScheduledWithReleaseVersion(releaseVersion));

    public static Generator<MethodologyVersion> WithScheduledWithReleaseVersionId(
        this Generator<MethodologyVersion> generator,
        Guid releaseVersionId
    ) => generator.ForInstance(d => d.SetScheduledWithReleaseVersionId(releaseVersionId));

    public static Generator<MethodologyVersion> WithRedirects(
        this Generator<MethodologyVersion> generator,
        IEnumerable<MethodologyRedirect> methodologyRedirects
    ) => generator.ForInstance(s => s.SetRedirects(methodologyRedirects));

    public static Generator<MethodologyVersion> WithRedirects(
        this Generator<MethodologyVersion> generator,
        Func<SetterContext, IEnumerable<MethodologyRedirect>> methodologyRedirects
    ) => generator.ForInstance(s => s.SetRedirects(methodologyRedirects.Invoke));

    public static InstanceSetters<MethodologyVersion> SetDefaults(this InstanceSetters<MethodologyVersion> setters) =>
        setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Title)
            .SetDefault(p => p.Version)
            .SetApprovalStatus(MethodologyApprovalStatus.Draft);

    public static InstanceSetters<MethodologyVersion> SetAlternativeSlug(
        this InstanceSetters<MethodologyVersion> setters,
        string alternativeSlug
    ) => setters.Set(mv => mv.AlternativeSlug, alternativeSlug);

    public static InstanceSetters<MethodologyVersion> SetAlternativeTitle(
        this InstanceSetters<MethodologyVersion> setters,
        string alternativeTitle
    ) => setters.Set(mv => mv.AlternativeTitle, alternativeTitle);

    public static InstanceSetters<MethodologyVersion> SetApprovalStatus(
        this InstanceSetters<MethodologyVersion> setters,
        MethodologyApprovalStatus approvalStatus
    ) => setters.Set(mv => mv.Status, approvalStatus);

    public static InstanceSetters<MethodologyVersion> SetPublished(
        this InstanceSetters<MethodologyVersion> setters,
        DateTime published
    ) => setters.Set(mv => mv.Published, published);

    public static InstanceSetters<MethodologyVersion> SetPublishingStrategy(
        this InstanceSetters<MethodologyVersion> setters,
        MethodologyPublishingStrategy publishingStrategy
    ) => setters.Set(mv => mv.PublishingStrategy, publishingStrategy);

    public static InstanceSetters<MethodologyVersion> SetScheduledWithReleaseVersion(
        this InstanceSetters<MethodologyVersion> setters,
        ReleaseVersion releaseVersion
    ) =>
        setters
            .Set(mv => mv.ScheduledWithReleaseVersion, releaseVersion)
            .SetScheduledWithReleaseVersionId(releaseVersion.Id);

    public static InstanceSetters<MethodologyVersion> SetScheduledWithReleaseVersionId(
        this InstanceSetters<MethodologyVersion> setters,
        Guid releaseVersionId
    ) => setters.Set(mv => mv.ScheduledWithReleaseVersionId, releaseVersionId);

    public static InstanceSetters<MethodologyVersion> SetRedirects(
        this InstanceSetters<MethodologyVersion> setters,
        IEnumerable<MethodologyRedirect> methodologyRedirects
    ) => setters.SetRedirects(_ => methodologyRedirects);

    private static InstanceSetters<MethodologyVersion> SetRedirects(
        this InstanceSetters<MethodologyVersion> setters,
        Func<SetterContext, IEnumerable<MethodologyRedirect>> methodologyRedirects
    ) =>
        setters.Set(
            mv => mv.MethodologyRedirects,
            (_, methodologyVersion, context) =>
            {
                var list = methodologyRedirects.Invoke(context).ToList();

                list.ForEach(methodologyRedirect =>
                {
                    methodologyRedirect.MethodologyVersion = methodologyVersion;
                    methodologyRedirect.MethodologyVersionId = methodologyVersion.Id;
                });

                return list;
            }
        );
}
