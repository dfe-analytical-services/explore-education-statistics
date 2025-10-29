using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class EmbedBlockGeneratorExtensions
{
    public static Generator<EmbedBlock> DefaultEmbedBlock(this DataFixture fixture) =>
        fixture.Generator<EmbedBlock>().WithDefaults();

    public static Generator<EmbedBlock> WithDefaults(this Generator<EmbedBlock> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<EmbedBlock> SetDefaults(this InstanceSetters<EmbedBlock> setters) =>
        setters
            .SetDefault(eb => eb.Id)
            .SetDefault(eb => eb.Title)
            .SetDefault(eb => eb.Url)
            .Set(eb => eb.Created, f => f.Date.Past());

    public static Generator<EmbedBlock> WithId(this Generator<EmbedBlock> generator, Guid id) =>
        generator.ForInstance(s => s.SetId(id));

    public static Generator<EmbedBlock> WithTitle(this Generator<EmbedBlock> generator, string title) =>
        generator.ForInstance(s => s.SetTitle(title));

    public static Generator<EmbedBlock> WithUrl(this Generator<EmbedBlock> generator, string url) =>
        generator.ForInstance(s => s.SetUrl(url));

    public static Generator<EmbedBlock> WithCreated(this Generator<EmbedBlock> generator, DateTime created) =>
        generator.ForInstance(s => s.SetCreated(created));

    public static Generator<EmbedBlock> WithUpdated(this Generator<EmbedBlock> generator, DateTime? updated) =>
        generator.ForInstance(s => s.SetUpdated(updated));

    public static InstanceSetters<EmbedBlock> SetId(this InstanceSetters<EmbedBlock> setters, Guid id) =>
        setters.Set(eb => eb.Id, id);

    public static InstanceSetters<EmbedBlock> SetTitle(this InstanceSetters<EmbedBlock> setters, string title) =>
        setters.Set(eb => eb.Title, title);

    public static InstanceSetters<EmbedBlock> SetUrl(this InstanceSetters<EmbedBlock> setters, string url) =>
        setters.Set(eb => eb.Url, url);

    public static InstanceSetters<EmbedBlock> SetCreated(this InstanceSetters<EmbedBlock> setters, DateTime created) =>
        setters.Set(eb => eb.Created, created);

    public static InstanceSetters<EmbedBlock> SetUpdated(this InstanceSetters<EmbedBlock> setters, DateTime? updated) =>
        setters.Set(eb => eb.Updated, updated);
}
