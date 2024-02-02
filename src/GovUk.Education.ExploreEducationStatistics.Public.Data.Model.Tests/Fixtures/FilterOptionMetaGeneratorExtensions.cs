using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterOptionMetaGeneratorExtensions
{
    public static Generator<FilterOptionMeta> DefaultFilterOptionMeta(this DataFixture fixture)
        => fixture.Generator<FilterOptionMeta>().WithDefaults();

    public static Generator<FilterOptionMeta> WithDefaults(this Generator<FilterOptionMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterOptionMeta> WithPublicId(this Generator<FilterOptionMeta> generator, string publicId)
        => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<FilterOptionMeta> WithPrivateId(this Generator<FilterOptionMeta> generator, int privateId)
        => generator.ForInstance(s => s.SetPrivateId(privateId));

    public static Generator<FilterOptionMeta> WithLabel(this Generator<FilterOptionMeta> generator, string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<FilterOptionMeta> WithIsAggregate(
        this Generator<FilterOptionMeta> generator,
        bool? isAggregate)
        => generator.ForInstance(s => s.SetIsAggregate(isAggregate));

    public static InstanceSetters<FilterOptionMeta> SetDefaults(this InstanceSetters<FilterOptionMeta> setters)
        => setters
            .SetDefault(m => m.PublicId)
            .SetDefault(m => m.PrivateId)
            .SetDefault(m => m.Label)
            .Set(m => m.IsAggregate, false);

    public static InstanceSetters<FilterOptionMeta> SetPublicId(
        this InstanceSetters<FilterOptionMeta> setters,
        string publicId)
        => setters.Set(m => m.PublicId, publicId);

    public static InstanceSetters<FilterOptionMeta> SetPrivateId(
        this InstanceSetters<FilterOptionMeta> setters,
        int privateId)
        => setters.Set(m => m.PrivateId, privateId);

    public static InstanceSetters<FilterOptionMeta> SetLabel(
        this InstanceSetters<FilterOptionMeta> setters,
        string label)
        => setters.Set(m => m.Label, label);

    public static InstanceSetters<FilterOptionMeta> SetIsAggregate(
        this InstanceSetters<FilterOptionMeta> setters,
        bool? isAggregate)
        => setters.Set(m => m.IsAggregate, isAggregate);
}
