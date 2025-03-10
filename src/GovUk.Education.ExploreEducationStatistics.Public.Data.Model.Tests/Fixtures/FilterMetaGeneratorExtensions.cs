using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterMetaGeneratorExtensions
{
    public static Generator<FilterMeta> DefaultFilterMeta(this DataFixture fixture)
        => fixture.Generator<FilterMeta>().WithDefaults();

    public static Generator<FilterMeta> DefaultFilterMeta(this DataFixture fixture, int options)
        => fixture.Generator<FilterMeta>()
            .WithDefaults()
            .WithOptions(() => fixture.DefaultFilterOptionMeta().GenerateList(options));

    public static Generator<FilterMeta> WithDefaults(this Generator<FilterMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterMeta> WithDataSetVersion(this Generator<FilterMeta> generator,
        DataSetVersion dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<FilterMeta> WithDataSetVersionId(this Generator<FilterMeta> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<FilterMeta> WithPublicId(this Generator<FilterMeta> generator, string identifier)
        => generator.ForInstance(s => s.SetPublicId(identifier));

    public static Generator<FilterMeta> WithColumn(this Generator<FilterMeta> generator, string column)
        => generator.ForInstance(s => s.SetColumn(column));

    public static Generator<FilterMeta> WithLabel(this Generator<FilterMeta> generator, string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<FilterMeta> WithHint(this Generator<FilterMeta> generator, string hint)
        => generator.ForInstance(s => s.SetHint(hint));

    public static Generator<FilterMeta> WithDefaultOptionId(
        this Generator<FilterMeta> generator,
        int? defaultOptionId)
        => generator.ForInstance(s => s.SetDefaultOptionId(defaultOptionId));

    public static Generator<FilterMeta> WithDefaultOption(
        this Generator<FilterMeta> generator,
        FilterOptionMeta? defaultOption)
        => generator.ForInstance(s => s.SetDefaultOption(defaultOption));

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
            .Set(m => m.PublicId, f => f.Random.AlphaNumeric(10))
            .SetDefault(m => m.Column)
            .SetDefault(m => m.Label)
            .SetDefault(m => m.Hint);

    public static InstanceSetters<FilterMeta> SetDataSetVersion(
        this InstanceSetters<FilterMeta> setters,
        DataSetVersion dataSetVersion)
        => setters
            .Set(m => m.DataSetVersion, dataSetVersion)
            .SetDataSetVersionId(dataSetVersion.Id);

    public static InstanceSetters<FilterMeta> SetDataSetVersionId(
        this InstanceSetters<FilterMeta> setters,
        Guid dataSetVersionId)
        => setters.Set(m => m.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<FilterMeta> SetPublicId(this InstanceSetters<FilterMeta> setters, string publicId)
        => setters.Set(m => m.PublicId, publicId);

    public static InstanceSetters<FilterMeta> SetColumn(this InstanceSetters<FilterMeta> setters, string column)
        => setters.Set(m => m.Column, column);

    public static InstanceSetters<FilterMeta> SetLabel(this InstanceSetters<FilterMeta> setters, string label)
        => setters.Set(m => m.Label, label);

    public static InstanceSetters<FilterMeta> SetHint(this InstanceSetters<FilterMeta> setters, string hint)
        => setters.Set(m => m.Hint, hint);

    public static InstanceSetters<FilterMeta> SetDefaultOptionId(
        this InstanceSetters<FilterMeta> setters,
        int? defaultOptionId)
        => setters.Set(m => m.DefaultOptionId, defaultOptionId);

    public static InstanceSetters<FilterMeta> SetDefaultOption(
        this InstanceSetters<FilterMeta> setters,
        FilterOptionMeta? defaultOption)
        => setters
            .Set(m => m.DefaultOption, defaultOption)
            .Set(m => m.DefaultOptionId, defaultOption?.Id);

    public static InstanceSetters<FilterMeta> SetOptions(
        this InstanceSetters<FilterMeta> setters,
        IEnumerable<FilterOptionMeta> options)
        => setters.SetOptions(() => options);

    public static InstanceSetters<FilterMeta> SetOptions(
        this InstanceSetters<FilterMeta> setters,
        Func<IEnumerable<FilterOptionMeta>> options)
        => setters
            .Set((_, m, context) =>
            {
                m.Options = options().ToList();
                m.OptionLinks = m.Options
                    .Select(o => context.Fixture
                        .DefaultFilterOptionMetaLink()
                        .WithOption(o)
                        .WithMeta(m)
                        .Generate())
                    .ToList();
            });

    public static InstanceSetters<FilterMeta> SetOptionLinks(
        this InstanceSetters<FilterMeta> setters,
        Func<IEnumerable<FilterOptionMetaLink>> links)
        => setters.Set(m => m.OptionLinks, _ => links().ToList());
}
