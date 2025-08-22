using System;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class LinkGeneratorExtensions
{
    public static Generator<Link> DefaultLink(this DataFixture fixture)
        => fixture.Generator<Link>().WithDefaults();

    public static Generator<Link> WithDefaults(this Generator<Link> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<Link> SetDefaults(this InstanceSetters<Link> setters)
        => setters
            .SetDefault(l => l.Id)
            .SetDefault(l => l.Description)
            .SetDefault(l => l.Url);

    public static Generator<Link> WithId(
        this Generator<Link> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<Link> WithDescription(
        this Generator<Link> generator,
        string description)
        => generator.ForInstance(s => s.SetDescription(description));

    public static Generator<Link> WithUrl(
        this Generator<Link> generator,
        string url)
        => generator.ForInstance(s => s.SetUrl(url));

    public static InstanceSetters<Link> SetId(
        this InstanceSetters<Link> setters,
        Guid id)
        => setters.Set(l => l.Id, id);

    public static InstanceSetters<Link> SetDescription(
        this InstanceSetters<Link> setters,
        string description)
        => setters.Set(l => l.Description, description);

    public static InstanceSetters<Link> SetUrl(
        this InstanceSetters<Link> setters,
        string url)
        => setters.Set(l => l.Url, url);
}
