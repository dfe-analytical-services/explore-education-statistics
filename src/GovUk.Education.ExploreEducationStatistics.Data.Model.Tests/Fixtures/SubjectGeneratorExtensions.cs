#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class SubjectGeneratorExtensions
{
    public static Generator<Subject> DefaultSubject(this DataFixture fixture)
        => fixture.Generator<Subject>().WithDefaults();

    public static Generator<Subject> WithDefaults(this Generator<Subject> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<Subject> WithFilters(this Generator<Subject> generator, IEnumerable<Filter> filters)
        => generator.ForInstance(s => s.SetFilters(filters));
    
    public static Generator<Subject> WithFilters(this Generator<Subject> generator, Func<SetterContext, IEnumerable<Filter>> filters)
        => generator.ForInstance(s => s.SetFilters(filters));

    public static Generator<Subject> WithIndicatorGroups(
        this Generator<Subject> generator,
        IEnumerable<IndicatorGroup> indicatorGroups)
        => generator.ForInstance(s => s.SetIndicatorGroups(indicatorGroups));

    public static Generator<Subject> WithIndicatorGroups(
        this Generator<Subject> generator,
        Func<SetterContext, IEnumerable<IndicatorGroup>> indicatorGroups)
        => generator.ForInstance(s => s.SetIndicatorGroups(indicatorGroups));
    
    public static Generator<Subject> WithObservations(
        this Generator<Subject> generator,
        IEnumerable<Observation> observations)
        => generator.ForInstance(s => s.SetObservations(observations));
    
    public static Generator<Subject> WithFootnotes(
        this Generator<Subject> generator,
        IEnumerable<Footnote> footnotes)
        => generator.ForInstance(s => s.SetFootnotes(footnotes));
    
    public static InstanceSetters<Subject> SetDefaults(this InstanceSetters<Subject> setters)
        => setters.SetDefault(s => s.Id);

    public static InstanceSetters<Subject> SetFilters(
        this InstanceSetters<Subject> setters,
        IEnumerable<Filter> filters)
        => setters.SetFilters(_ => filters);

    public static InstanceSetters<Subject> SetFilters(
        this InstanceSetters<Subject> setters,
        Func<SetterContext, IEnumerable<Filter>> filters)
        => setters.Set(
            s => s.Filters,
            (_, subject, context) =>
            {
                var list = filters.Invoke(context).ToList();

                list.ForEach(filter => filter.Subject = subject);

                return list;
            }
        );

    public static InstanceSetters<Subject> SetIndicatorGroups(
        this InstanceSetters<Subject> setters,
        IEnumerable<IndicatorGroup> indicatorGroups)
        => setters.SetIndicatorGroups(_ => indicatorGroups);

    public static InstanceSetters<Subject> SetIndicatorGroups(
        this InstanceSetters<Subject> setters,
        Func<SetterContext, IEnumerable<IndicatorGroup>> indicatorGroups)
        => setters.Set(
            s => s.IndicatorGroups,
            (_, subject, context) =>
            {
                var list = indicatorGroups.Invoke(context).ToList();

                list.ForEach(indicatorGroup => indicatorGroup.Subject = subject);

                return list;
            }
        );

    public static InstanceSetters<Subject> SetObservations(
        this InstanceSetters<Subject> setters,
        IEnumerable<Observation> observations)
        => setters.Set(
            s => s.Observations,
            (_, subject) =>
            {
                var list = observations.ToList();

                list.ForEach(observation => observation.Subject = subject);

                return list;
            }
        );

    public static InstanceSetters<Subject> SetFootnotes(
        this InstanceSetters<Subject> setters,
        IEnumerable<Footnote> footnotes)
        => setters.Set(
            s => s.Footnotes,
            (_, subject) => footnotes
                .Select(
                    footnote =>
                    {
                        var subjectFootnote = new SubjectFootnote
                        {
                            Subject = subject,
                            SubjectId = subject.Id,
                            Footnote = footnote,
                            FootnoteId = footnote.Id,
                        };

                        footnote.Subjects.Add(subjectFootnote);

                        return subjectFootnote;
                    }
                )
                .ToList()
        );
}
