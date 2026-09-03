using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class FeaturedTableGeneratorExtensions
{
    public static Generator<FeaturedTable> DefaultFeaturedTable(this DataFixture fixture) =>
        fixture.Generator<FeaturedTable>().WithDefaults();

    public static Generator<FeaturedTable> WithDefaults(this Generator<FeaturedTable> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<FeaturedTable> WithOrder(this Generator<FeaturedTable> generator, int order) =>
        generator.ForInstance(s => s.SetOrder(order));

    public static Generator<FeaturedTable> WithReleaseVersion(
        this Generator<FeaturedTable> generator,
        ReleaseVersion releaseVersion
    ) => generator.ForInstance(s => s.SetReleaseVersion(releaseVersion));

    public static Generator<FeaturedTable> WithDataBlock(
        this Generator<FeaturedTable> generator,
        DataBlock dataBlock
    ) => generator.ForInstance(s => s.SetDataBlock(dataBlock));

    public static Generator<FeaturedTable> WithDataBlockParent(
        this Generator<FeaturedTable> generator,
        DataBlockParent dataBlockParent
    ) => generator.ForInstance(s => s.SetDataBlockParent(dataBlockParent));

    public static Generator<FeaturedTable> WithCreated(
        this Generator<FeaturedTable> generator,
        DateTime created,
        Guid? createdById = null
    ) => generator.ForInstance(s => s.SetCreated(created, createdById));

    public static Generator<FeaturedTable> WithUpdated(
        this Generator<FeaturedTable> generator,
        DateTime updated,
        Guid? updatedById = null
    ) => generator.ForInstance(s => s.SetUpdated(updated, updatedById));

    public static InstanceSetters<FeaturedTable> SetDefaults(this InstanceSetters<FeaturedTable> setters) =>
        setters
            .SetDefault(featuredTable => featuredTable.Id)
            .SetDefault(featuredTable => featuredTable.Name)
            .SetDefault(featuredTable => featuredTable.Description)
            .SetDefault(featuredTable => featuredTable.DataBlockId)
            .SetDefault(featuredTable => featuredTable.DataBlockParentId)
            .SetDefault(featuredTable => featuredTable.Order, offset: 1)
            .Set(featuredTable => featuredTable.Created, DateTime.UtcNow.AddDays(-1))
            .SetDefault(featuredTable => featuredTable.CreatedById);

    public static InstanceSetters<FeaturedTable> SetOrder(this InstanceSetters<FeaturedTable> setters, int order) =>
        setters.Set(s => s.Order, order);

    public static InstanceSetters<FeaturedTable> SetReleaseVersion(
        this InstanceSetters<FeaturedTable> setters,
        ReleaseVersion releaseVersion
    ) => setters.Set(s => s.ReleaseVersion, releaseVersion).Set(s => s.ReleaseVersionId, releaseVersion.Id);

    public static InstanceSetters<FeaturedTable> SetDataBlock(
        this InstanceSetters<FeaturedTable> setters,
        DataBlock dataBlock
    ) => setters.Set(s => s.DataBlock, dataBlock).Set(s => s.DataBlockId, dataBlock.Id);

    public static InstanceSetters<FeaturedTable> SetDataBlockParent(
        this InstanceSetters<FeaturedTable> setters,
        DataBlockParent dataBlockParent
    ) => setters.Set(s => s.DataBlockParent, dataBlockParent).Set(s => s.DataBlockParentId, dataBlockParent.Id);

    public static InstanceSetters<FeaturedTable> SetUpdated(
        this InstanceSetters<FeaturedTable> setters,
        DateTime updated,
        Guid? updatedById = null
    ) =>
        setters
            .Set(s => s.Updated, updated)
            .Set(s => s.UpdatedById, (_, featuredTable, _) => updatedById ?? featuredTable.UpdatedById);

    public static InstanceSetters<FeaturedTable> SetCreated(
        this InstanceSetters<FeaturedTable> setters,
        DateTime created,
        Guid? createdById = null
    ) =>
        setters
            .Set(s => s.Created, created)
            .Set(s => s.CreatedById, (_, featuredTable, _) => createdById ?? featuredTable.CreatedById);
}
