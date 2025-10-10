using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class EmbedBlockLinkGeneratorExtensions
{
    public static Generator<EmbedBlockLink> DefaultEmbedBlockLink(this DataFixture fixture) =>
        fixture.Generator<EmbedBlockLink>().WithDefaults();

    public static Generator<EmbedBlockLink> WithDefaults(this Generator<EmbedBlockLink> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<EmbedBlockLink> SetDefaults(this InstanceSetters<EmbedBlockLink> setters) =>
        setters
            .SetDefault(ebl => ebl.Id)
            .SetDefault(ebl => ebl.Order, offset: 1)
            .Set(ebl => ebl.Created, f => f.Date.Past());

    public static Generator<EmbedBlockLink> WithId(this Generator<EmbedBlockLink> generator, Guid id) =>
        generator.ForInstance(s => s.SetId(id));

    public static Generator<EmbedBlockLink> WithComments(
        this Generator<EmbedBlockLink> generator,
        List<Comment> comments
    ) => generator.ForInstance(s => s.SetComments(comments));

    public static Generator<EmbedBlockLink> WithEmbedBlock(
        this Generator<EmbedBlockLink> generator,
        EmbedBlock embedBlock
    ) => generator.ForInstance(s => s.SetEmbedBlock(embedBlock));

    public static Generator<EmbedBlockLink> WithEmbedBlockId(
        this Generator<EmbedBlockLink> generator,
        Guid embedBlockId
    ) => generator.ForInstance(s => s.SetEmbedBlockId(embedBlockId));

    public static Generator<EmbedBlockLink> WithOrder(this Generator<EmbedBlockLink> generator, int order) =>
        generator.ForInstance(s => s.SetOrder(order));

    public static Generator<EmbedBlockLink> WithCreated(this Generator<EmbedBlockLink> generator, DateTime created) =>
        generator.ForInstance(s => s.SetCreated(created));

    public static Generator<EmbedBlockLink> WithUpdated(this Generator<EmbedBlockLink> generator, DateTime? updated) =>
        generator.ForInstance(s => s.SetUpdated(updated));

    public static InstanceSetters<EmbedBlockLink> SetId(this InstanceSetters<EmbedBlockLink> setters, Guid id) =>
        setters.Set(ebl => ebl.Id, id);

    public static InstanceSetters<EmbedBlockLink> SetComments(
        this InstanceSetters<EmbedBlockLink> setters,
        List<Comment> comments
    ) => setters.Set(ebl => ebl.Comments, comments);

    public static InstanceSetters<EmbedBlockLink> SetEmbedBlock(
        this InstanceSetters<EmbedBlockLink> setters,
        EmbedBlock embedBlock
    ) => setters.Set(ebl => ebl.EmbedBlock, embedBlock).SetEmbedBlockId(embedBlock.Id);

    public static InstanceSetters<EmbedBlockLink> SetEmbedBlockId(
        this InstanceSetters<EmbedBlockLink> setters,
        Guid embedBlockId
    ) => setters.Set(ebl => ebl.EmbedBlockId, embedBlockId);

    public static InstanceSetters<EmbedBlockLink> SetOrder(this InstanceSetters<EmbedBlockLink> setters, int order) =>
        setters.Set(ebl => ebl.Order, order);

    public static InstanceSetters<EmbedBlockLink> SetCreated(
        this InstanceSetters<EmbedBlockLink> setters,
        DateTime created
    ) => setters.Set(ebl => ebl.Created, created);

    public static InstanceSetters<EmbedBlockLink> SetUpdated(
        this InstanceSetters<EmbedBlockLink> setters,
        DateTime? updated
    ) => setters.Set(ebl => ebl.Updated, updated);
}
