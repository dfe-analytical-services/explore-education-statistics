using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationProviderOptionMetaGeneratorExtensions
{
    public static Generator<LocationProviderOptionMeta> DefaultLocationProviderOptionMeta(
        this DataFixture fixture
    ) => fixture.Generator<LocationProviderOptionMeta>().WithDefaults();

    public static Generator<LocationProviderOptionMeta> WithDefaults(
        this Generator<LocationProviderOptionMeta> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationProviderOptionMeta> WithUkprn(
        this Generator<LocationProviderOptionMeta> generator,
        string ukprn
    ) => generator.ForInstance(s => s.SetUkprn(ukprn));

    public static InstanceSetters<LocationProviderOptionMeta> SetDefaults(
        this InstanceSetters<LocationProviderOptionMeta> setters
    ) => setters.SetBaseDefaults().SetDefaultCode(m => m.Ukprn);

    public static InstanceSetters<LocationProviderOptionMeta> SetUkprn(
        this InstanceSetters<LocationProviderOptionMeta> setters,
        string ukprn
    ) => setters.Set(m => m.Ukprn, ukprn);
}
