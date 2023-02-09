#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class FilterGeneratorExtensions
{
    public static Generator<Filter> DefaultFilter(this DataFixture fixture)
        => fixture.Generator<Filter>().WithDefaults();

    public static Generator<Filter> WithDefaults(this Generator<Filter> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<Filter> WithSubject(this Generator<Filter> generator, Subject subject)
        => generator.ForInstance(s => s.SetSubject(subject));

    public static Generator<Filter> WithFilterGroups(
        this Generator<Filter> generator,
        IEnumerable<FilterGroup> filterGroups)
        => generator.ForInstance(s => s.SetFilterGroups(filterGroups));

    // TODO this needs to apply to backwards-linking that s.SetFilterGroups(filterGroups) does
    public static Generator<Filter> WithFilterGroups(
        this Generator<Filter> generator,
        Func<SetterContext, IEnumerable<FilterGroup>> filterGroups)
        => generator.ForInstance(s => s
            .Set(f => f.FilterGroups, (_, _, context) => filterGroups.Invoke(context)));

    public static Generator<Filter> WithFootnotes(
        this Generator<Filter> generator,
        IEnumerable<Footnote> footnotes)
        => generator.ForInstance(s => s.SetFootnotes(footnotes));

    public static InstanceSetters<Filter> SetDefaults(this InstanceSetters<Filter> setters)
        => setters
            .SetDefault(f => f.Id)
            .SetDefault(f => f.Label)
            .SetDefault(f => f.Hint)
            .SetDefault(f => f.Name);

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
        => setters.Set(
            f => f.FilterGroups,
            (_, filter) =>
            {
                var list = filterGroups.ToList();

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
