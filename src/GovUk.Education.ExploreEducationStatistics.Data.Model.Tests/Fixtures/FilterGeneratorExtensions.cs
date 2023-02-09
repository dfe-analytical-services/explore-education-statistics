#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class FilterGeneratorExtensions
{
    public static Generator<Filter> DefaultFilter(this DataFixture fixture)
        => fixture.Generator<Filter>().WithDefaults();

    public static Generator<Filter> DefaultFilter(
        this DataFixture fixture,
        int filterGroupCount,
        int filterItemCount)
        => fixture
            .DefaultFilter()
            .WithFilterGroups(_ => fixture.DefaultFilterGroup(filterItemCount: filterItemCount)
                .Generate(filterGroupCount));

    public static Generator<Filter> WithDefaults(this Generator<Filter> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<Filter> WithSubject(this Generator<Filter> generator, Subject subject)
        => generator.ForInstance(s => s.SetSubject(subject));

    public static Generator<Filter> WithFilterGroups(
        this Generator<Filter> generator,
        IEnumerable<FilterGroup> filterGroups)
        => generator.ForInstance(s => s.SetFilterGroups(filterGroups));

    public static Generator<Filter> WithFilterGroups(
        this Generator<Filter> generator,
        Func<SetterContext, IEnumerable<FilterGroup>> filterGroups)
        => generator.ForInstance(s => s.SetFilterGroups(filterGroups.Invoke));

    public static Generator<Filter> WithFootnotes(
        this Generator<Filter> generator,
        IEnumerable<Footnote> footnotes)
        => generator.ForInstance(s => s.SetFootnotes(footnotes));

    public static InstanceSetters<Filter> SetDefaults(this InstanceSetters<Filter> setters)
        => setters
            .SetDefault(f => f.Id)
            .SetDefault(f => f.Label)
            .SetDefault(f => f.Hint)
            .SetDefault(f => f.Name)
            .Set(f => f.Name, (_, f) => f.Name.SnakeCase());

    public static InstanceSetters<Filter> SetSubject(
        this InstanceSetters<Filter> setters,
        Subject subject)
        => setters
            .Set(
                f => f.Subject,
                (_, filter) =>
                {
                    subject.Filters.Add(filter);
                    return subject;
                }
            )
            .Set(f => f.SubjectId, subject.Id);

    public static InstanceSetters<Filter> SetFilterGroups(
        this InstanceSetters<Filter> setters,
        IEnumerable<FilterGroup> filterGroups) 
        => setters.SetFilterGroups(_ => filterGroups);
    
    private static InstanceSetters<Filter> SetFilterGroups(
        this InstanceSetters<Filter> setters,
        Func<SetterContext, IEnumerable<FilterGroup>> filterGroups)
        => setters.Set(
            f => f.FilterGroups,
            (_, filter, context) =>
            {
                var list = filterGroups.Invoke(context).ToList();

                list.ForEach(filterGroup => filterGroup.Filter = filter);

                return list;
            }
        );

    public static InstanceSetters<Filter> SetFootnotes(
        this InstanceSetters<Filter> setters,
        IEnumerable<Footnote> footnotes)
        => setters.Set(
            f => f.Footnotes,
            (_, filter) => footnotes
                .Select(
                    footnote =>
                    {
                        var filterFootnote = new FilterFootnote
                        {
                            Filter = filter,
                            FilterId = filter.Id,
                            Footnote = footnote,
                            FootnoteId = footnote.Id,
                        };

                        footnote.Filters.Add(filterFootnote);

                        return filterFootnote;
                    }
                )
                .ToList()
        );
}
