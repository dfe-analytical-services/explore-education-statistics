#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class FilterItemGeneratorExtensions
{
    public static Generator<FilterItem> DefaultFilterItem(this DataFixture fixture)
        => fixture.Generator<FilterItem>().WithDefaults();

    public static Generator<FilterItem> WithDefaults(this Generator<FilterItem> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterItem> WithFilterGroup(this Generator<FilterItem> generator, FilterGroup filterGroup)
        => generator.ForInstance(s => s.SetFilterGroup(filterGroup));

    public static Generator<FilterItem> WithFootnotes(
        this Generator<FilterItem> generator,
        IEnumerable<Footnote> footnotes)
        => generator.ForInstance(s => s.SetFootnotes(footnotes));

    public static InstanceSetters<FilterItem> SetDefaults(this InstanceSetters<FilterItem> setters)
        => setters
            .SetDefault(fi => fi.Id)
            .SetDefault(fi => fi.Label);

    public static InstanceSetters<FilterItem> SetFilterGroup(
        this InstanceSetters<FilterItem> setters,
        FilterGroup filterGroup)
        => setters
            .Set(
                fi => fi.FilterGroup,
                (_, filterItem) =>
                {
                    filterGroup.FilterItems.Add(filterItem);
                    return filterGroup;
                }
            )
            .Set(fi => fi.FilterGroupId, filterGroup.Id);

    public static InstanceSetters<FilterItem> SetFootnotes(
        this InstanceSetters<FilterItem> setters,
        IEnumerable<Footnote> footnotes)
        => setters.Set(
            fi => fi.Footnotes,
            (_, filterItem) => footnotes
                .Select(
                    footnote =>
                    {
                        var filterItemFootnote = new FilterItemFootnote
                        {
                            FilterItem = filterItem,
                            FilterItemId = filterItem.Id,
                            Footnote = footnote,
                            FootnoteId = footnote.Id,
                        };

                        footnote.FilterItems.Add(filterItemFootnote);

                        return filterItemFootnote;
                    }
                )
                .ToList()
        );
}
