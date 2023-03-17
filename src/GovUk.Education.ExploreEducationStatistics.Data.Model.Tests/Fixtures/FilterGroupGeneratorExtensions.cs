#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class FilterGroupGeneratorExtensions
{
    public static Generator<FilterGroup> DefaultFilterGroup(this DataFixture fixture)
        => fixture.Generator<FilterGroup>().WithDefaults();

    public static Generator<FilterGroup> DefaultFilterGroup(this DataFixture fixture, int filterItemCount)
        => fixture.Generator<FilterGroup>()
            .WithDefaults()
            .WithFilterItems(fixture.DefaultFilterItem()
                .Generate(filterItemCount));

    public static Generator<FilterGroup> WithDefaults(this Generator<FilterGroup> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterGroup> WithFilter(this Generator<FilterGroup> generator, Filter filter)
        => generator.ForInstance(s => s.SetFilter(filter));

    public static Generator<FilterGroup> WithFilterItems(
        this Generator<FilterGroup> generator,
        IEnumerable<FilterItem> filterItems)
        => generator.ForInstance(s => s.SetFilterItems(filterItems));

    public static Generator<FilterGroup> WithFilterItems(
        this Generator<FilterGroup> generator,
        Func<SetterContext, IEnumerable<FilterItem>> filterItems)
        => generator.ForInstance(s => s.SetFilterItems(filterItems));

    public static Generator<FilterGroup> WithFootnotes(
        this Generator<FilterGroup> generator,
        IEnumerable<Footnote> footnotes)
        => generator.ForInstance(s => s.SetFootnotes(footnotes));

    public static InstanceSetters<FilterGroup> SetDefaults(this InstanceSetters<FilterGroup> setters)
        => setters
            .SetDefault(fg => fg.Id)
            .SetDefault(fg => fg.Label);

    public static InstanceSetters<FilterGroup> SetFilter(
        this InstanceSetters<FilterGroup> setters,
        Filter filter)
        => setters
            .Set(
                fg => fg.Filter,
                (_, group) =>
                {
                    filter.FilterGroups.Add(group);
                    return filter;
                }
            )
            .Set(fg => fg.FilterId, filter.Id);

    public static InstanceSetters<FilterGroup> SetFilterItems(
        this InstanceSetters<FilterGroup> setters,
        IEnumerable<FilterItem> filterItems)
        => setters.SetFilterItems(_ => filterItems);
    
    public static InstanceSetters<FilterGroup> SetFilterItems(
        this InstanceSetters<FilterGroup> setters,
        Func<SetterContext, IEnumerable<FilterItem>> filterItems)
        => setters.Set(
            fg => fg.FilterItems,
            (_, filterGroup, context) =>
            {
                var list = filterItems.Invoke(context).ToList();

                list.ForEach(filterItem => filterItem.FilterGroup = filterGroup);

                return list;
            }
        );

    public static InstanceSetters<FilterGroup> SetFootnotes(
        this InstanceSetters<FilterGroup> setters,
        IEnumerable<Footnote> footnotes)
        => setters.Set(
            fg => fg.Footnotes,
            (_, filterGroup) => footnotes
                .Select(
                    footnote =>
                    {
                        var filterGroupFootnote = new FilterGroupFootnote
                        {
                            FilterGroup = filterGroup,
                            FilterGroupId = filterGroup.Id,
                            Footnote = footnote,
                            FootnoteId = footnote.Id,
                        };

                        footnote.FilterGroups.Add(filterGroupFootnote);

                        return filterGroupFootnote;
                    }
                )
                .ToList()
        );
}
