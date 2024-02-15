using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationSchoolOptionMetaGeneratorExtensions
{
    public static Generator<LocationSchoolOptionMeta> DefaultLocationSchoolOptionMeta(
        this DataFixture fixture)
        => fixture.Generator<LocationSchoolOptionMeta>().WithDefaults();

    public static Generator<LocationSchoolOptionMeta> WithDefaults(
        this Generator<LocationSchoolOptionMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationSchoolOptionMeta> WithUkprn(
        this Generator<LocationSchoolOptionMeta> generator,
        string ukprn)
        => generator.ForInstance(s => s.SetUrn(ukprn));

    public static Generator<LocationSchoolOptionMeta> WithLaEstab(
        this Generator<LocationSchoolOptionMeta> generator,
        string laEstab)
        => generator.ForInstance(s => s.SetLaEstab(laEstab));
    public static InstanceSetters<LocationSchoolOptionMeta> SetDefaults(
        this InstanceSetters<LocationSchoolOptionMeta> setters)
        => setters
            .SetDefault(m => m.Label)
            .SetDefault(m => m.Urn)
            .SetDefault(m => m.LaEstab);

    public static InstanceSetters<LocationSchoolOptionMeta> SetUrn(
        this InstanceSetters<LocationSchoolOptionMeta> setters,
        string ukprn)
        => setters.Set(m => m.Urn, ukprn);

    public static InstanceSetters<LocationSchoolOptionMeta> SetLaEstab(
        this InstanceSetters<LocationSchoolOptionMeta> setters,
        string laEstab)
        => setters.Set(m => m.LaEstab, laEstab);
}
