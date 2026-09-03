using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class KeyStatisticDataBlockGeneratorExtensions
{
    public static Generator<KeyStatisticDataBlock> DefaultKeyStatisticDataBlock(this DataFixture fixture) =>
        fixture.Generator<KeyStatisticDataBlock>().WithDefaults();

    public static Generator<KeyStatisticDataBlock> WithDefaults(this Generator<KeyStatisticDataBlock> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<KeyStatisticDataBlock> SetDefaults(
        this InstanceSetters<KeyStatisticDataBlock> setters
    ) =>
        setters
            .SetDefault(db => db.Id)
            .SetDefault(db => db.GuidanceText)
            .SetDefault(db => db.GuidanceTitle)
            .SetDefault(db => db.Order)
            .SetDefault(db => db.Trend)
            .Set(db => db.Created, f => f.Date.Past());

    public static Generator<KeyStatisticDataBlock> WithId(this Generator<KeyStatisticDataBlock> generator, Guid id) =>
        generator.ForInstance(s => s.SetId(id));

    public static Generator<KeyStatisticDataBlock> WithDataBlock(
        this Generator<KeyStatisticDataBlock> generator,
        DataBlock dataBlock
    ) => generator.ForInstance(s => s.SetDataBlock(dataBlock));

    public static Generator<KeyStatisticDataBlock> WithDataBlockId(
        this Generator<KeyStatisticDataBlock> generator,
        Guid dataBlockId
    ) => generator.ForInstance(s => s.SetDataBlockId(dataBlockId));

    public static Generator<KeyStatisticDataBlock> WithDataBlockParent(
        this Generator<KeyStatisticDataBlock> generator,
        DataBlockParent dataBlockParent
    ) => generator.ForInstance(s => s.SetDataBlockParent(dataBlockParent));

    public static Generator<KeyStatisticDataBlock> WithDataBlockParentId(
        this Generator<KeyStatisticDataBlock> generator,
        Guid dataBlockParentId
    ) => generator.ForInstance(s => s.SetDataBlockParentId(dataBlockParentId));

    public static Generator<KeyStatisticDataBlock> WithGuidanceText(
        this Generator<KeyStatisticDataBlock> generator,
        string? guidanceText
    ) => generator.ForInstance(s => s.SetGuidanceText(guidanceText));

    public static Generator<KeyStatisticDataBlock> WithGuidanceTitle(
        this Generator<KeyStatisticDataBlock> generator,
        string? guidanceTitle
    ) => generator.ForInstance(s => s.SetGuidanceTitle(guidanceTitle));

    public static Generator<KeyStatisticDataBlock> WithOrder(
        this Generator<KeyStatisticDataBlock> generator,
        int order
    ) => generator.ForInstance(s => s.SetOrder(order));

    public static Generator<KeyStatisticDataBlock> WithTrend(
        this Generator<KeyStatisticDataBlock> generator,
        string? trend
    ) => generator.ForInstance(s => s.SetTrend(trend));

    public static Generator<KeyStatisticDataBlock> WithCreated(
        this Generator<KeyStatisticDataBlock> generator,
        DateTime created
    ) => generator.ForInstance(s => s.SetCreated(created));

    public static Generator<KeyStatisticDataBlock> WithCreatedById(
        this Generator<KeyStatisticDataBlock> generator,
        Guid createdById
    ) => generator.ForInstance(s => s.SetCreatedById(createdById));

    public static Generator<KeyStatisticDataBlock> WithUpdated(
        this Generator<KeyStatisticDataBlock> generator,
        DateTime? updated
    ) => generator.ForInstance(s => s.SetUpdated(updated));

    public static Generator<KeyStatisticDataBlock> WithUpdatedById(
        this Generator<KeyStatisticDataBlock> generator,
        Guid? updatedById
    ) => generator.ForInstance(s => s.SetUpdatedById(updatedById));

    public static InstanceSetters<KeyStatisticDataBlock> SetId(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        Guid id
    ) => setters.Set(db => db.Id, id);

    public static InstanceSetters<KeyStatisticDataBlock> SetDataBlock(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        DataBlock dataBlock
    ) => setters.Set(db => db.DataBlock, dataBlock).SetDataBlockId(dataBlock.Id);

    public static InstanceSetters<KeyStatisticDataBlock> SetDataBlockId(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        Guid dataBlockId
    ) => setters.Set(db => db.DataBlockId, dataBlockId);

    public static InstanceSetters<KeyStatisticDataBlock> SetDataBlockParent(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        DataBlockParent dataBlockParent
    ) => setters.Set(db => db.DataBlockParent, dataBlockParent).SetDataBlockParentId(dataBlockParent.Id);

    public static InstanceSetters<KeyStatisticDataBlock> SetDataBlockParentId(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        Guid dataBlockParentId
    ) => setters.Set(db => db.DataBlockParentId, dataBlockParentId);

    public static InstanceSetters<KeyStatisticDataBlock> SetGuidanceText(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        string? guidanceText
    ) => setters.Set(db => db.GuidanceText, guidanceText);

    public static InstanceSetters<KeyStatisticDataBlock> SetGuidanceTitle(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        string? guidanceTitle
    ) => setters.Set(db => db.GuidanceTitle, guidanceTitle);

    public static InstanceSetters<KeyStatisticDataBlock> SetOrder(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        int order
    ) => setters.Set(db => db.Order, order);

    public static InstanceSetters<KeyStatisticDataBlock> SetTrend(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        string? trend
    ) => setters.Set(db => db.Trend, trend);

    public static InstanceSetters<KeyStatisticDataBlock> SetCreated(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        DateTime created
    ) => setters.Set(db => db.Created, created);

    public static InstanceSetters<KeyStatisticDataBlock> SetCreatedById(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        Guid createdById
    ) => setters.Set(db => db.CreatedById, createdById);

    public static InstanceSetters<KeyStatisticDataBlock> SetUpdated(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        DateTime? updated
    ) => setters.Set(db => db.Updated, updated);

    public static InstanceSetters<KeyStatisticDataBlock> SetUpdatedById(
        this InstanceSetters<KeyStatisticDataBlock> setters,
        Guid? updatedById
    ) => setters.Set(db => db.UpdatedById, updatedById);
}
