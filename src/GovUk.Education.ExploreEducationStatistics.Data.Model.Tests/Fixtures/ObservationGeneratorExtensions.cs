#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class ObservationGeneratorExtensions
{
    public static Generator<Observation> DefaultObservation(this DataFixture fixture)
        => fixture.Generator<Observation>().WithDefaults();

    public static Generator<Observation> WithDefaults(this Generator<Observation> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<Observation> WithSubject(this Generator<Observation> generator, Subject subject)
        => generator.ForInstance(s => s.SetSubject(subject));

    public static Generator<Observation> WithLocation(this Generator<Observation> generator, Location location)
        => generator.ForInstance(s => s.SetLocation(location));

    public static Generator<Observation> WithFilterItems(
        this Generator<Observation> generator,
        IEnumerable<FilterItem> filterItems)
        => generator.ForInstance(s => s.SetFilterItems(filterItems));

    public static Generator<Observation> WithMeasures(
        this Generator<Observation> generator,
        IEnumerable<Indicator> measures)
        => generator.ForInstance(s => s.SetMeasures(measures));

    public static InstanceSetters<Observation> SetDefaults(this InstanceSetters<Observation> setters)
        => setters
            .SetDefault(o => o.Id)
            .Set(o => o.TimeIdentifier, TimeIdentifier.AcademicYear)
            .Set(o => o.Year, f => f.Random.Int(2016, 2022))
            .Set(o => o.CsvRow, f => f.IndexFaker + 1);

    public static InstanceSetters<Observation> SetSubject(
        this InstanceSetters<Observation> setters,
        Subject subject)
        => setters
            .Set(
                o => o.Subject,
                (_, observation) =>
                {
                    subject.Observations.Add(observation);
                    return subject;
                }
            )
            .Set(o => o.SubjectId, subject.Id);

    public static InstanceSetters<Observation> SetLocation(
        this InstanceSetters<Observation> setters,
        Location location)
        => setters
            .Set(o => o.Location, location)
            .Set(o => o.LocationId, location.Id);

    public static InstanceSetters<Observation> SetFilterItems(
        this InstanceSetters<Observation> setters,
        params FilterItem[] filterItems)
        => setters.SetFilterItems(filterItems.ToList());

    public static InstanceSetters<Observation> SetFilterItems(
        this InstanceSetters<Observation> setters,
        IEnumerable<FilterItem> filterItems)
        => setters.Set(
            o => o.FilterItems,
            (_, observation) => filterItems
                .Select(
                    filterItem => new ObservationFilterItem
                    {
                        FilterItem = filterItem,
                        FilterItemId = filterItem.Id,
                        Filter = filterItem.FilterGroup?.Filter,
                        FilterId = filterItem.FilterGroup?.FilterId,
                        Observation = observation,
                        ObservationId = observation.Id,
                    }
                )
                .ToList()
        );

    public static InstanceSetters<Observation> SetMeasures(
        this InstanceSetters<Observation> setters,
        IEnumerable<Indicator> indicators)
        => setters.Set(
            o => o.Measures,
            f => indicators.ToDictionary(i => i.Id, _ => f.Random.Short().ToString())
        );

    public static InstanceSetters<Observation> SetTimePeriod(
        this InstanceSetters<Observation> setters,
        int year,
        TimeIdentifier identifier)
        => setters
            .Set(o => o.Year, year)
            .Set(o => o.TimeIdentifier, identifier);
}
