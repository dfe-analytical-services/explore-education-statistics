using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationCodedOptionMetaGeneratorExtensions
{
    public static Generator<LocationCodedOptionMeta> DefaultLocationCodedOptionMeta(
        this DataFixture fixture
    ) => fixture.Generator<LocationCodedOptionMeta>().WithDefaults();

    public static Generator<LocationCodedOptionMeta> WithDefaults(
        this Generator<LocationCodedOptionMeta> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationCodedOptionMeta> WithCode(
        this Generator<LocationCodedOptionMeta> generator,
        string code
    ) => generator.ForInstance(s => s.SetCode(code));

    public static InstanceSetters<LocationCodedOptionMeta> SetDefaults(
        this InstanceSetters<LocationCodedOptionMeta> setters
    ) => setters.SetBaseDefaults().SetDefaultCode(m => m.Code);

    public static InstanceSetters<LocationCodedOptionMeta> SetCode(
        this InstanceSetters<LocationCodedOptionMeta> setters,
        string code
    ) => setters.Set(m => m.Code, code);
}
