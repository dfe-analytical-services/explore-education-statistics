#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class TopicGeneratorExtensions
{
    public static Generator<Topic> DefaultTopic(this DataFixture fixture)
        => fixture.Generator<Topic>().WithDefaults();

    public static Generator<Topic> WithDefaults(this Generator<Topic> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<Topic> SetDefaults(this InstanceSetters<Topic> setters)
        => setters
            .SetDefault(t => t.Id)
            .SetDefault(t => t.Slug)
            .SetDefault(t => t.Title);

    public static Generator<Topic> WithPublications(
        this Generator<Topic> generator,
        IEnumerable<Publication> publications)
        => generator.ForInstance(s => s.SetPublications(publications));

    public static Generator<Topic> WithPublications(
        this Generator<Topic> generator,
        Func<SetterContext, IEnumerable<Publication>> publications)
        => generator.ForInstance(s => s.SetPublications(publications.Invoke));

    public static Generator<Topic> WithTheme(
        this Generator<Topic> generator,
        Theme theme)
        => generator.ForInstance(s => s.SetTheme(theme));

    public static Generator<Topic> WithThemeId(
        this Generator<Topic> generator,
        Guid themeId)
        => generator.ForInstance(s => s.SetThemeId(themeId));

    public static Generator<Topic> WithThemes(this Generator<Topic> generator,
        IEnumerable<Theme> themes)
    {
        themes.ForEach((theme, index) =>
            generator.ForIndex(index, s => s.SetTheme(theme)));

        return generator;
    }

    public static Generator<Topic> WithSlug(
        this Generator<Topic> generator,
        string slug)
        => generator.ForInstance(s => s.SetSlug(slug));

    public static Generator<Topic> WithTitle(
        this Generator<Topic> generator,
        string title)
        => generator.ForInstance(s => s.SetTitle(title));

    public static InstanceSetters<Topic> SetPublications(
        this InstanceSetters<Topic> setters,
        IEnumerable<Publication> publications)
        => setters.SetPublications(_ => publications);

    public static InstanceSetters<Topic> SetPublications(
        this InstanceSetters<Topic> setters,
        Func<SetterContext, IEnumerable<Publication>> publications)
        => setters.Set(
            t => t.Publications,
            (_, topic, context) =>
            {
                var list = publications.Invoke(context).ToList();

                list.ForEach(publication => publication.Topic = topic);

                return list;
            }
        );

    public static InstanceSetters<Topic> SetTheme(
        this InstanceSetters<Topic> setters,
        Theme theme)
        => setters.Set(t => t.Theme, theme)
            .SetThemeId(theme.Id);

    public static InstanceSetters<Topic> SetThemeId(
        this InstanceSetters<Topic> setters,
        Guid themeId)
        => setters.Set(t => t.ThemeId, themeId);

    public static InstanceSetters<Topic> SetSlug(
        this InstanceSetters<Topic> setters,
        string slug)
        => setters.Set(t => t.Slug, slug);

    public static InstanceSetters<Topic> SetTitle(
        this InstanceSetters<Topic> setters,
        string title)
        => setters.Set(t => t.Title, title);
}
