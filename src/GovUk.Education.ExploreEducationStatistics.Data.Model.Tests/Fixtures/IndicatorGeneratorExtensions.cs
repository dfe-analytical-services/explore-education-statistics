#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class IndicatorGeneratorExtensions
{
    public static Generator<Indicator> DefaultIndicator(this DataFixture fixture)
        => fixture.Generator<Indicator>().WithDefaults();

    public static Generator<Indicator> WithDefaults(this Generator<Indicator> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<Indicator> WithIndicatorGroup(
        this Generator<Indicator> generator,
        IndicatorGroup indicatorGroup)
        => generator.ForInstance(s => s.SetIndicatorGroup(indicatorGroup));

    public static Generator<Indicator> WithFootnotes(
        this Generator<Indicator> generator,
        IEnumerable<Footnote> footnotes)
        => generator.ForInstance(s => s.SetFootnotes(footnotes));

    public static InstanceSetters<Indicator> SetDefaults(this InstanceSetters<Indicator> setters)
        => setters
            .SetDefault(i => i.Id)
            .SetDefault(i => i.Label)
            .SetDefault(i => i.Name)
            .Set(i => i.Name, (_, i) => i.Name.SnakeCase())
            .Set(i => i.Unit, Unit.Number)
            .Set(i => i.DecimalPlaces, 0);

    public static InstanceSetters<Indicator> SetIndicatorGroup(
        this InstanceSetters<Indicator> setters,
        IndicatorGroup indicatorGroup)
        => setters
            .Set(
                i => i.IndicatorGroup,
                (_, indicator) =>
                {
                    indicatorGroup.Indicators.Add(indicator);
                    return indicatorGroup;
                }
            )
            .Set(i => i.IndicatorGroupId, indicatorGroup.Id);

    public static InstanceSetters<Indicator> SetFootnotes(
        this InstanceSetters<Indicator> setters,
        IEnumerable<Footnote> footnotes)
        => setters.Set(
            obj => obj.Footnotes,
            (_, indicator) => footnotes
                .Select(
                    footnote =>
                    {
                        var indicatorFootnote = new IndicatorFootnote
                        {
                            Indicator = indicator,
                            IndicatorId = indicator.Id,
                            Footnote = footnote,
                            FootnoteId = footnote.Id,
                        };

                        footnote.Indicators.Add(indicatorFootnote);

                        return indicatorFootnote;
                    }
                )
                .ToList()
        );
}
