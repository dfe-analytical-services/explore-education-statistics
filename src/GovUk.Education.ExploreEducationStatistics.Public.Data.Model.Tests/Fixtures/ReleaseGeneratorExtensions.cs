using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class ReleaseGeneratorExtensions
{
    public static Generator<Release> DefaultDataSetVersionRelease(this DataFixture fixture) =>
        fixture.Generator<Release>().WithDefaults();

    public static Generator<Release> WithDefaults(this Generator<Release> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<Release> WithDataSetFileId(
        this Generator<Release> generator,
        Guid dataSeFileId
    ) => generator.ForInstance(s => s.SetDataSetFileId(dataSeFileId));

    public static Generator<Release> WithReleaseFileId(
        this Generator<Release> generator,
        Guid releaseFileId
    ) => generator.ForInstance(s => s.SetReleaseFileId(releaseFileId));

    public static Generator<Release> WithSlug(this Generator<Release> generator, string slug) =>
        generator.ForInstance(s => s.SetSlug(slug));

    public static Generator<Release> WithTitle(this Generator<Release> generator, string title) =>
        generator.ForInstance(s => s.SetTitle(title));

    public static InstanceSetters<Release> SetDefaults(this InstanceSetters<Release> setters) =>
        setters
            .SetDefault(r => r.DataSetFileId)
            .SetDefault(r => r.ReleaseFileId)
            .SetDefault(r => r.Slug)
            .SetDefault(r => r.Title);

    public static InstanceSetters<Release> SetDataSetFileId(
        this InstanceSetters<Release> instanceSetter,
        Guid dataSetFileId
    ) => instanceSetter.Set(r => r.DataSetFileId, dataSetFileId);

    public static InstanceSetters<Release> SetReleaseFileId(
        this InstanceSetters<Release> instanceSetter,
        Guid releaseFileId
    ) => instanceSetter.Set(r => r.ReleaseFileId, releaseFileId);

    public static InstanceSetters<Release> SetSlug(
        this InstanceSetters<Release> instanceSetter,
        string slug
    ) => instanceSetter.Set(r => r.Slug, slug);

    public static InstanceSetters<Release> SetTitle(
        this InstanceSetters<Release> instanceSetter,
        string title
    ) => instanceSetter.Set(r => r.Title, title);
}
