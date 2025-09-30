using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseSeriesItemGeneratorExtensions
{
    public static Generator<ReleaseSeriesItem> DefaultReleaseSeriesItem(this DataFixture fixture)
        => fixture.Generator<ReleaseSeriesItem>().WithDefaults();

    public static Generator<ReleaseSeriesItem> DefaultLegacyReleaseSeriesItem(this DataFixture fixture)
        => fixture.Generator<ReleaseSeriesItem>().WithLegacyDefaults();

    public static Generator<ReleaseSeriesItem> WithDefaults(this Generator<ReleaseSeriesItem> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ReleaseSeriesItem> WithLegacyDefaults(this Generator<ReleaseSeriesItem> generator)
        => generator.ForInstance(s => s.SetLegacyDefaults());

    public static Generator<ReleaseSeriesItem> WithReleaseId(
        this Generator<ReleaseSeriesItem> generator,
        Guid? releaseId)
        => generator.ForInstance(s => s.SetReleaseId(releaseId));

    public static Generator<ReleaseSeriesItem> WithLegacyLinkUrl(
        this Generator<ReleaseSeriesItem> generator,
        string? legacyLinkUrl)
        => generator.ForInstance(s => s.SetLegacyLinkUrl(legacyLinkUrl));

    public static Generator<ReleaseSeriesItem> WithLegacyLinkDescription(
        this Generator<ReleaseSeriesItem> generator,
        string? legacyLinkDescription)
        => generator.ForInstance(s => s.SetLegacyLinkDescription(legacyLinkDescription));

    public static InstanceSetters<ReleaseSeriesItem> SetDefaults(this InstanceSetters<ReleaseSeriesItem> setters)
        => setters
            .SetDefault(rsi => rsi.Id);

    public static InstanceSetters<ReleaseSeriesItem> SetLegacyDefaults(this InstanceSetters<ReleaseSeriesItem> setters)
        => setters
            .SetDefaults()
            .SetDefault(rsi => rsi.LegacyLinkDescription)
            .SetDefault(rsi => rsi.LegacyLinkUrl);

    public static InstanceSetters<ReleaseSeriesItem> SetReleaseId(
        this InstanceSetters<ReleaseSeriesItem> instanceSetter,
        Guid? releaseId)
        => instanceSetter.Set(rsi => rsi.ReleaseId, releaseId);

    public static InstanceSetters<ReleaseSeriesItem> SetLegacyLinkUrl(
        this InstanceSetters<ReleaseSeriesItem> instanceSetter,
        string? legacyLinkUrl)
        => instanceSetter.Set(rsi => rsi.LegacyLinkUrl, legacyLinkUrl);

    public static InstanceSetters<ReleaseSeriesItem> SetLegacyLinkDescription(
        this InstanceSetters<ReleaseSeriesItem> instanceSetter,
        string? legacyLinkDescription)
        => instanceSetter.Set(rsi => rsi.LegacyLinkDescription, legacyLinkDescription);
}
