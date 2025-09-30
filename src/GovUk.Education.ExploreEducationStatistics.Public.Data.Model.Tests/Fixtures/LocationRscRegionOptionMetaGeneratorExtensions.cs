using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationRscRegionOptionMetaGeneratorExtensions
{
    public static Generator<LocationRscRegionOptionMeta> DefaultLocationRscRegionOptionMeta(
        this DataFixture fixture
    ) => fixture.Generator<LocationRscRegionOptionMeta>().WithDefaults();

    public static Generator<LocationRscRegionOptionMeta> WithDefaults(
        this Generator<LocationRscRegionOptionMeta> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<LocationRscRegionOptionMeta> SetDefaults(
        this InstanceSetters<LocationRscRegionOptionMeta> setters
    ) => setters.SetBaseDefaults();
}
