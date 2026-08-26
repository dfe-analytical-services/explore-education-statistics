using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataBlockGeneratorExtensions
{
    public static Generator<DataBlock> DefaultDataBlock(this DataFixture fixture) =>
        fixture.Generator<DataBlock>().WithDefaults();

    public static Generator<DataBlock> WithDefaults(this Generator<DataBlock> generator) =>
        generator.ForInstance(dataBlock => dataBlock.SetDefaults());

    public static Generator<DataBlock> WithLatestDraftVersion(
        this Generator<DataBlock> generator,
        DataBlockVersion? version
    ) => generator.ForInstance(dataBlock => dataBlock.SetLatestDraftVersion(version));

    public static Generator<DataBlock> WithLatestDraftVersion(
        this Generator<DataBlock> generator,
        Func<DataBlockVersion> version
    ) => generator.ForInstance(dataBlock => dataBlock.SetLatestDraftVersion(version));

    public static Generator<DataBlock> WithLatestPublishedVersion(
        this Generator<DataBlock> generator,
        DataBlockVersion? version
    ) => generator.ForInstance(dataBlock => dataBlock.SetLatestPublishedVersion(version));

    public static Generator<DataBlock> WithLatestPublishedVersion(
        this Generator<DataBlock> generator,
        Func<DataBlockVersion?> version
    ) => generator.ForInstance(dataBlock => dataBlock.SetLatestPublishedVersion(version));

    public static InstanceSetters<DataBlock> SetDefaults(this InstanceSetters<DataBlock> setters) =>
        setters.SetDefault(dataBlock => dataBlock.Id);

    public static InstanceSetters<DataBlock> SetLatestDraftVersion(
        this InstanceSetters<DataBlock> setters,
        DataBlockVersion? version
    ) => setters.SetLatestDraftVersion(() => version);

    public static InstanceSetters<DataBlock> SetLatestDraftVersion(
        this InstanceSetters<DataBlock> setters,
        Func<DataBlockVersion?> version
    ) =>
        setters.Set(
            (_, dataBlock, _) =>
            {
                var dataBlockVersion = version.Invoke();
                dataBlock.LatestDraftVersion = dataBlockVersion;
                dataBlock.LatestDraftVersionId = dataBlockVersion?.Id;

                dataBlockVersion?.DataBlock = dataBlock;
            }
        );

    public static InstanceSetters<DataBlock> SetLatestPublishedVersion(
        this InstanceSetters<DataBlock> setters,
        DataBlockVersion? version
    ) => setters.SetLatestPublishedVersion(() => version);

    public static InstanceSetters<DataBlock> SetLatestPublishedVersion(
        this InstanceSetters<DataBlock> setters,
        Func<DataBlockVersion?> version
    ) =>
        setters.Set(
            (_, dataBlock, _) =>
            {
                var dataBlockVersion = version.Invoke();
                dataBlock.LatestPublishedVersion = dataBlockVersion;
                dataBlock.LatestPublishedVersionId = dataBlockVersion?.Id;

                dataBlock.LatestDraftVersion ??= dataBlockVersion;

                dataBlockVersion?.DataBlock = dataBlock;
            }
        );
}
