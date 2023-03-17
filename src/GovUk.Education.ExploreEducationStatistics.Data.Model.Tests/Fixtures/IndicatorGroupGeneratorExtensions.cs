#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class IndicatorGroupGeneratorExtensions
{
    public static Generator<IndicatorGroup> DefaultIndicatorGroup(this DataFixture fixture)
        => fixture.Generator<IndicatorGroup>().WithDefaults();

    public static Generator<IndicatorGroup> DefaultIndicatorGroup(this DataFixture fixture, int indicatorCount)
        => fixture.Generator<IndicatorGroup>()
            .WithDefaults()
            .WithIndicators(fixture.DefaultIndicator().Generate(indicatorCount));

    public static Generator<IndicatorGroup> WithDefaults(this Generator<IndicatorGroup> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<IndicatorGroup> WithSubject(this Generator<IndicatorGroup> generator, Subject subject)
        => generator.ForInstance(s => s.SetSubject(subject));

    public static Generator<IndicatorGroup> WithIndicators(
        this Generator<IndicatorGroup> generator,
        IEnumerable<Indicator> indicators)
        => generator.ForInstance(s => s.SetIndicators(indicators));
    
    public static InstanceSetters<IndicatorGroup> SetDefaults(this InstanceSetters<IndicatorGroup> setters)
        => setters
            .SetDefault(ig => ig.Id)
            .SetDefault(ig => ig.Label);

    public static InstanceSetters<IndicatorGroup> SetSubject(
        this InstanceSetters<IndicatorGroup> setters,
        Subject subject)
        => setters
            .Set(
                ig => ig.Subject,
                (_, indicatorGroup) =>
                {
                    subject.IndicatorGroups.Add(indicatorGroup);
                    return subject;
                }
            )
            .Set(ig => ig.SubjectId, subject.Id);

    public static InstanceSetters<IndicatorGroup> SetIndicators(
        this InstanceSetters<IndicatorGroup> setters,
        IEnumerable<Indicator> indicators)
        => setters.Set(
            ig => ig.Indicators,
            (_, indicatorGroup) =>
            {
                var list = indicators.ToList();

                list.ForEach(indicator => indicator.IndicatorGroup = indicatorGroup);

                return list;
            }
        );
}
