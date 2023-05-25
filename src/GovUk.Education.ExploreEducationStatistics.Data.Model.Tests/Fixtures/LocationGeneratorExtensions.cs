#nullable enable
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures.LocationHierarchyPresets;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class LocationGeneratorExtensions
{
    public static Generator<Location> DefaultLocation(this DataFixture fixture)
        => fixture.Generator<Location>().WithDefaults();

    public static Generator<Location> WithDefaults(this Generator<Location> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<Location> WithGeographicLevel(this Generator<Location> generator, GeographicLevel level)
        => generator.ForInstance(s => s.SetGeographicLevel(level));

    public static Generator<Location> WithPresetRegion(this Generator<Location> generator)
        => generator.ForInstance(s => s.SetPresetRegion());

    public static Generator<Location> WithPresetLocalAuthority(this Generator<Location> generator, Region region)
        => generator.ForInstance(s => s.SetPresetLocalAuthority(region));

    public static Generator<Location> WithPresetRegionAndLocalAuthority(this Generator<Location> generator)
        => generator.ForInstance(s => s.SetPresetRegionAndLocalAuthority());

    public static InstanceSetters<Location> SetDefaults(this InstanceSetters<Location> setters)
        => setters
            .SetDefault(l => l.Id)
            .Set(l => l.Country, CountryPresets.England);

    public static InstanceSetters<Location> SetGeographicLevel(
        this InstanceSetters<Location> setters,
        GeographicLevel level)
        => setters.Set(l => l.GeographicLevel, level);

    public static InstanceSetters<Location> SetPresetRegion(
        this InstanceSetters<Location> setters)
        => setters.Set(
            l => l.Region,
            f => f.PickRandom(RegionLocalAuthorities.Value.Keys.ToList())
        );

    public static InstanceSetters<Location> SetPresetLocalAuthority(
        this InstanceSetters<Location> setters,
        Region region)
        => setters.Set(
                l => l.LocalAuthority,
                f => f.PickRandom(RegionLocalAuthorities.Value[region])
        );

    public static InstanceSetters<Location> SetPresetRegionAndLocalAuthority(
        this InstanceSetters<Location> setters)
        => setters
            .SetPresetRegion()
            .Set(
                l => l.LocalAuthority,
                (faker, location) => faker.PickRandom(RegionLocalAuthorities.Value[location.Region!])
            );
}
