using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class OrganisationGeneratorExtensions
{
    public static Generator<Organisation> DefaultOrganisation(this DataFixture fixture)
        => fixture.Generator<Organisation>().WithDefaults();

    public static Generator<Organisation> WithDefaults(this Generator<Organisation> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<Organisation> SetDefaults(this InstanceSetters<Organisation> setters)
        => setters
            .SetDefault(t => t.Id)
            .SetDefault(t => t.Title)
            .SetDefault(t => t.Url)
            .Set(p => p.Created, f => f.Date.Past());

    public static Generator<Organisation> WithId(
        this Generator<Organisation> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<Organisation> WithTitle(
        this Generator<Organisation> generator,
        string title)
        => generator.ForInstance(s => s.SetTitle(title));

    public static Generator<Organisation> WithUrl(
        this Generator<Organisation> generator,
        string url)
        => generator.ForInstance(s => s.SetUrl(url));

    public static Generator<Organisation> WithCreated(
        this Generator<Organisation> generator,
        DateTimeOffset created) => generator.ForInstance(s => s.SetCreated(created));

    public static Generator<Organisation> WithUpdated(
        this Generator<Organisation> generator,
        DateTimeOffset? updated) => generator.ForInstance(s => s.SetUpdated(updated));

    public static InstanceSetters<Organisation> SetId(
        this InstanceSetters<Organisation> setters,
        Guid id)
        => setters.Set(o => o.Id, id);

    public static InstanceSetters<Organisation> SetTitle(
        this InstanceSetters<Organisation> setters,
        string title)
        => setters.Set(o => o.Title, title);

    public static InstanceSetters<Organisation> SetUrl(
        this InstanceSetters<Organisation> setters,
        string url)
        => setters.Set(o => o.Url, url);

    public static InstanceSetters<Organisation> SetCreated(
        this InstanceSetters<Organisation> setters,
        DateTimeOffset created)
        => setters.Set(o => o.Created, created);

    public static InstanceSetters<Organisation> SetUpdated(
        this InstanceSetters<Organisation> setters,
        DateTimeOffset? updated)
        => setters.Set(o => o.Updated, updated);
}
