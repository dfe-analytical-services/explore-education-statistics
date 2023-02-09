#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class FootnoteGeneratorExtensions
{
    public static Generator<Footnote> DefaultFootnote(this DataFixture fixture)
        => fixture.Generator<Footnote>().WithDefaults();

    public static Generator<Footnote> WithDefaults(this Generator<Footnote> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<Footnote> SetDefaults(this InstanceSetters<Footnote> setters)
        => setters
            .SetDefault(f => f.Id)
            .SetDefault(f => f.Content)
            .Set(f => f.Order, f => f.IndexFaker)
            .Set(f => f.Created, f => f.Date.Past())
            .Set(
                f => f.Updated,
                (f, footnote) => f.Date.Soon(14, footnote.Created)
            );
    
    public static InstanceSetters<Footnote> SetSubjects(this InstanceSetters<Footnote> instanceSetter, Func<SetterContext, IEnumerable<Subject>> subjects)
    {
        instanceSetter.Set(footnote => footnote.Subjects, (_, footnote, context) =>
        {
            return subjects
                .Invoke(context)
                .Select(subject => new SubjectFootnote
                {
                    Footnote = footnote,
                    Subject = subject
                })
                .ToList();
        });
    
        return instanceSetter;
    }
    
    public static InstanceSetters<Footnote> SetFilters(this InstanceSetters<Footnote> instanceSetter, Func<SetterContext, IEnumerable<Filter>> filters)
    {
        instanceSetter.Set(footnote => footnote.Filters, (_, footnote, context) =>
        {
            return filters
                .Invoke(context)
                .Select(filter => new FilterFootnote
                {
                    Footnote = footnote,
                    Filter = filter
                })
                .ToList();
        });
    
        return instanceSetter;
    }
    
    public static InstanceSetters<Footnote> SetFilterGroups(this InstanceSetters<Footnote> instanceSetter, Func<SetterContext, IEnumerable<FilterGroup>> filterGroups)
    {
        instanceSetter.Set(footnote => footnote.FilterGroups, (_, footnote, context) =>
        {
            return filterGroups
                .Invoke(context)
                .Select(filterGroup => new FilterGroupFootnote
                {
                    Footnote = footnote,
                    FilterGroup = filterGroup
                })
                .ToList();
        });
    
        return instanceSetter;
    }
    
    public static InstanceSetters<Footnote> SetFilterItems(this InstanceSetters<Footnote> instanceSetter, Func<SetterContext, IEnumerable<FilterItem>> filterItems)
    {
        instanceSetter.Set(footnote => footnote.FilterItems, (_, footnote, context) =>
        {
            return filterItems
                .Invoke(context)
                .Select(filterItem => new FilterItemFootnote
                {
                    Footnote = footnote,
                    FilterItem = filterItem
                })
                .ToList();
        });
    
        return instanceSetter;
    }
    
    public static InstanceSetters<Footnote> SetIndicators(this InstanceSetters<Footnote> instanceSetter, Func<SetterContext, IEnumerable<Indicator>> indicators)
    {
        instanceSetter.Set(footnote => footnote.Indicators, (_, footnote, context) =>
        {
            return indicators
                .Invoke(context)
                .Select(indicator => new IndicatorFootnote
                {
                    Footnote = footnote,
                    Indicator = indicator
                })
                .ToList();
        });
    
        return instanceSetter;
    }
}
