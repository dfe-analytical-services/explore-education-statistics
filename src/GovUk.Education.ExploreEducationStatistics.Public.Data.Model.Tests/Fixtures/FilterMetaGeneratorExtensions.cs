using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterMetaGeneratorExtensions
{
    public static Generator<FilterMeta> DefaultFilterMeta(this DataFixture fixture)
        => fixture.Generator<FilterMeta>().WithDefaults();

    public static Generator<FilterMeta> DefaultFilterMeta(this DataFixture fixture, int options)
        => fixture.Generator<FilterMeta>()
            .WithDefaults()
            .WithOptions(fixture.DefaultFilterOptionMeta().GenerateList(options));

    public static Generator<FilterMeta> WithDefaults(this Generator<FilterMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterMeta> WithDataSetVersion(this Generator<FilterMeta> generator, DataSetVersion dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<FilterMeta> WithPublicId(this Generator<FilterMeta> generator, string identifier)
        => generator.ForInstance(s => s.SetPublicId(identifier));

    public static Generator<FilterMeta> WithLabel(this Generator<FilterMeta> generator, string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<FilterMeta> WithHint(this Generator<FilterMeta> generator, string hint)
        => generator.ForInstance(s => s.SetHint(hint));

    public static Generator<FilterMeta> WithOptions(
        this Generator<FilterMeta> generator,
        IEnumerable<FilterOptionMeta> options)
        => generator.ForInstance(s => s.SetOptions(options));

    public static Generator<FilterMeta> WithOptions(
        this Generator<FilterMeta> generator,
        Func<IEnumerable<FilterOptionMeta>> options)
        => generator.ForInstance(s => s.SetOptions(options));

    public static Generator<FilterMeta> WithOptionLinks(
        this Generator<FilterMeta> generator,
        Func<IEnumerable<FilterOptionMetaLink>> links)
        => generator.ForInstance(s => s.SetOptionLinks(links));

    public static InstanceSetters<FilterMeta> SetDefaults(this InstanceSetters<FilterMeta> setters)
        => setters
            .SetDefault(m => m.PublicId)
            .SetDefault(m => m.Label)
            .SetDefault(m => m.Hint);

    public static InstanceSetters<FilterMeta> SetDataSetVersion(
        this InstanceSetters<FilterMeta> setters,
        DataSetVersion dataSetVersion)
        => setters
            .Set(m => m.DataSetVersion, dataSetVersion)
            .Set(m => m.DataSetVersionId, dataSetVersion.Id);

    public static InstanceSetters<FilterMeta> SetPublicId(this InstanceSetters<FilterMeta> setters, string publicId)
        => setters.Set(m => m.PublicId, publicId);

    public static InstanceSetters<FilterMeta> SetLabel(this InstanceSetters<FilterMeta> setters, string label)
        => setters.Set(m => m.Label, label);

    public static InstanceSetters<FilterMeta> SetHint(this InstanceSetters<FilterMeta> setters, string hint)
        => setters.Set(m => m.Hint, hint);

    public static InstanceSetters<FilterMeta> SetOptions(
        this InstanceSetters<FilterMeta> setters,
        IEnumerable<FilterOptionMeta> options)
        => setters.Set(m => m.Options, () => options);

    public static InstanceSetters<FilterMeta> SetOptions(
        this InstanceSetters<FilterMeta> setters,
        Func<IEnumerable<FilterOptionMeta>> options)
        => setters.Set(m => m.Options, _ => options().ToList());
    
    public static InstanceSetters<FilterMeta> SetOptionLinks(
        this InstanceSetters<FilterMeta> setters,
        Func<IEnumerable<FilterOptionMetaLink>> links)
        => setters.Set(m => m.OptionLinks, _ => links().ToList());
}
