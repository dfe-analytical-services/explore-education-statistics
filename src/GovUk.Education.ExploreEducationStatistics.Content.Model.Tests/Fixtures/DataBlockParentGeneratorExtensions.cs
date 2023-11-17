#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataBlockGeneratorExtensions
{
    public static Generator<DataBlockParent> DefaultDataBlockParent(this DataFixture fixture)
        => fixture.Generator<DataBlockParent>().WithDefaults();

    public static Generator<DataBlockParent> WithDefaults(this Generator<DataBlockParent> generator)
        => generator.ForInstance(dataBlockParent => dataBlockParent.SetDefaults());

    public static Generator<DataBlockParent> WithLatestVersion(
        this Generator<DataBlockParent> generator,
        DataBlockVersion? version)
        => generator.ForInstance(dataBlockParent => dataBlockParent.SetLatestVersion(version));

    public static Generator<DataBlockParent> WithLatestVersion(
        this Generator<DataBlockParent> generator,
        Func<DataBlockVersion> version)
        => generator.ForInstance(dataBlockParent => dataBlockParent.SetLatestVersion(version));

    public static Generator<DataBlockParent> WithLatestPublishedVersion(
        this Generator<DataBlockParent> generator,
        DataBlockVersion? version)
        => generator.ForInstance(dataBlockParent => dataBlockParent.SetLatestPublishedVersion(version));

    public static Generator<DataBlockParent> WithLatestPublishedVersion(
        this Generator<DataBlockParent> generator,
        Func<DataBlockVersion?> version)
        => generator.ForInstance(dataBlockParent => dataBlockParent.SetLatestPublishedVersion(version));

    public static InstanceSetters<DataBlockParent> SetDefaults(this InstanceSetters<DataBlockParent> setters)
        => setters.SetDefault(dataBlockParent => dataBlockParent.Id);

    public static InstanceSetters<DataBlockParent> SetLatestVersion(
        this InstanceSetters<DataBlockParent> setters,
        DataBlockVersion? version)
        => setters.SetLatestVersion(() => version);

    public static InstanceSetters<DataBlockParent> SetLatestVersion(
        this InstanceSetters<DataBlockParent> setters,
        Func<DataBlockVersion?> version)
        => setters
            .Set((_, dataBlockParent, _) =>
            {
                var dataBlockVersion = version.Invoke();
                dataBlockParent.LatestVersion = dataBlockVersion;
                dataBlockParent.LatestVersionId = dataBlockVersion?.Id;

                if (dataBlockVersion != null)
                {
                    dataBlockVersion.DataBlockParent = dataBlockParent;
                }
            });

    public static InstanceSetters<DataBlockParent> SetLatestPublishedVersion(
        this InstanceSetters<DataBlockParent> setters,
        DataBlockVersion? version)
        => setters.SetLatestPublishedVersion(() => version);

    public static InstanceSetters<DataBlockParent> SetLatestPublishedVersion(
        this InstanceSetters<DataBlockParent> setters,
        Func<DataBlockVersion?> version)
        => setters
            .Set((_, dataBlockParent, _) =>
            {
                var dataBlockVersion = version.Invoke();
                dataBlockParent.LatestPublishedVersion = dataBlockVersion;
                dataBlockParent.LatestPublishedVersionId = dataBlockVersion?.Id;

                if (dataBlockParent.LatestVersion == null)
                {
                    dataBlockParent.LatestVersion = dataBlockVersion;
                }

                if (dataBlockVersion != null)
                {
                    dataBlockVersion.DataBlockParent = dataBlockParent;
                }
            });
}
