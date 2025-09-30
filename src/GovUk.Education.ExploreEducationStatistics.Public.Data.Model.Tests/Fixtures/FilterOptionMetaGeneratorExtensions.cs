using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterOptionMetaGeneratorExtensions
{
    public static Generator<FilterOptionMeta> DefaultFilterOptionMeta(this DataFixture fixture) =>
        fixture.Generator<FilterOptionMeta>().WithDefaults();

    public static Generator<FilterOptionMeta> WithDefaults(
        this Generator<FilterOptionMeta> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterOptionMeta> WithLabel(
        this Generator<FilterOptionMeta> generator,
        string label
    ) => generator.ForInstance(s => s.SetLabel(label));

    public static InstanceSetters<FilterOptionMeta> SetDefaults(
        this InstanceSetters<FilterOptionMeta> setters
    ) => setters.SetDefault(m => m.Label);

    public static InstanceSetters<FilterOptionMeta> SetLabel(
        this InstanceSetters<FilterOptionMeta> setters,
        string label
    ) => setters.Set(m => m.Label, label);
}
