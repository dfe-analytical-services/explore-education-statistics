using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationCodedOptionMetaGeneratorExtensions
{
    public static Generator<LocationOptionMeta> DefaultLocationCodedOptionMeta(this DataFixture fixture)
        => fixture.Generator<LocationOptionMeta>().WithDefaults();

    public static Generator<LocationOptionMeta> WithDefaults(this Generator<LocationOptionMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationOptionMeta> WithCode(
        this Generator<LocationOptionMeta> generator,
        string code)
        => generator.ForInstance(s => s.SetCode(code));

    public static InstanceSetters<LocationOptionMeta> SetDefaults(
        this InstanceSetters<LocationOptionMeta> setters)
        => setters
            .SetDefault(m => m.PublicId)
            .SetDefault(m => m.PrivateId)
            .SetDefault(m => m.Label)
            .SetDefault(m => m.Code);

    public static InstanceSetters<LocationOptionMeta> SetCode(
        this InstanceSetters<LocationOptionMeta> setters,
        string code)
        => setters.Set(m => m.Code, code);
}
