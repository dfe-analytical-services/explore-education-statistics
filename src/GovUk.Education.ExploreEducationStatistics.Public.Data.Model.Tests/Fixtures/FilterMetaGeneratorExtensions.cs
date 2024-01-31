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

    public static Generator<FilterMeta> WithIdentifier(this Generator<FilterMeta> generator, string identifier)
        => generator.ForInstance(s => s.SetIdentifier(identifier));

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

    public static InstanceSetters<FilterMeta> SetDefaults(this InstanceSetters<FilterMeta> setters)
        => setters
            .SetDefault(m => m.Identifier)
            .SetDefault(m => m.Label)
            .SetDefault(m => m.Hint);

    public static InstanceSetters<FilterMeta> SetIdentifier(this InstanceSetters<FilterMeta> setters, string identifier)
        => setters.Set(m => m.Identifier, identifier);

    public static InstanceSetters<FilterMeta> SetLabel(this InstanceSetters<FilterMeta> setters, string label)
        => setters.Set(m => m.Label, label);

    public static InstanceSetters<FilterMeta> SetHint(this InstanceSetters<FilterMeta> setters, string hint)
        => setters.Set(m => m.Hint, hint);

    public static InstanceSetters<FilterMeta> SetOptions(
        this InstanceSetters<FilterMeta> setters,
        IEnumerable<FilterOptionMeta> options)
        => setters.Set(m => m.Options, options.ToList());

    public static InstanceSetters<FilterMeta> SetOptions(
        this InstanceSetters<FilterMeta> setters,
        Func<IEnumerable<FilterOptionMeta>> options)
        => setters.Set(m => m.Options, _ => options().ToList());
}
