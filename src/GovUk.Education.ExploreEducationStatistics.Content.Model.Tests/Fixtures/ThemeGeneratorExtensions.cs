#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ThemeGeneratorExtensions
{
    public static Generator<Theme> DefaultTheme(this DataFixture fixture)
        => fixture.Generator<Theme>().WithDefaults();

    public static Generator<Theme> WithDefaults(this Generator<Theme> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<Theme> SetDefaults(this InstanceSetters<Theme> setters)
        => setters
            .SetDefault(t => t.Id)
            .SetDefault(t => t.Slug)
            .SetDefault(t => t.Summary)
            .SetDefault(t => t.Title);

    public static Generator<Theme> WithSlug(
        this Generator<Theme> generator,
        string slug)
        => generator.ForInstance(s => s.SetSlug(slug));

    public static Generator<Theme> WithSummary(
        this Generator<Theme> generator,
        string summary)
        => generator.ForInstance(s => s.SetSummary(summary));

    public static Generator<Theme> WithTitle(
        this Generator<Theme> generator,
        string title)
        => generator.ForInstance(s => s.SetTitle(title));

    public static Generator<Theme> WithTopics(
        this Generator<Theme> generator,
        IEnumerable<Topic> topics)
        => generator.ForInstance(s => s.SetTopics(topics));

    public static Generator<Theme> WithTopics(
        this Generator<Theme> generator,
        Func<SetterContext, IEnumerable<Topic>> topics)
        => generator.ForInstance(s => s.SetTopics(topics.Invoke));

    public static InstanceSetters<Theme> SetTopics(
        this InstanceSetters<Theme> setters,
        IEnumerable<Topic> topics)
        => setters.SetTopics(_ => topics);

    private static InstanceSetters<Theme> SetTopics(
        this InstanceSetters<Theme> setters,
        Func<SetterContext, IEnumerable<Topic>> topics)
        => setters.Set(
            t => t.Topics,
            (_, theme, context) =>
            {
                var list = topics.Invoke(context).ToList();

                list.ForEach(topic => topic.Theme = theme);

                return list;
            }
        );

    public static InstanceSetters<Theme> SetSlug(
        this InstanceSetters<Theme> setters,
        string slug)
        => setters.Set(t => t.Slug, slug);

    public static InstanceSetters<Theme> SetSummary(
        this InstanceSetters<Theme> setters,
        string summary)
        => setters.Set(t => t.Summary, summary);

    public static InstanceSetters<Theme> SetTitle(
        this InstanceSetters<Theme> setters,
        string title)
        => setters.Set(t => t.Title, title);
}
