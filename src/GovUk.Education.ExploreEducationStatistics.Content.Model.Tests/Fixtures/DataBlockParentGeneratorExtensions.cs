#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataBlockGeneratorExtensions
{
    public static Generator<DataBlockParent> DefaultDataBlockParent(this DataFixture fixture)
        => fixture.Generator<DataBlockParent>().WithDefaults();

    public static Generator<DataBlockParent> WithDefaults(this Generator<DataBlockParent> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static Generator<DataBlockParent> WithVersions(
        this Generator<DataBlockParent> generator,
        IEnumerable<DataBlockVersion> versions)
        => generator.ForInstance(d => d.SetVersions(versions));

    public static Generator<DataBlockParent> WithLatestVersion(
        this Generator<DataBlockParent> generator,
        DataBlockVersion version)
        => generator.ForInstance(d => d.SetLatestVersion(version));

    public static Generator<DataBlockParent> WithLatestPublishedVersion(
        this Generator<DataBlockParent> generator,
        DataBlockVersion version)
        => generator.ForInstance(d => d.SetLatestPublishedVersion(version));

    public static Generator<DataBlockParent> WithLatestPublishedVersion(
        this Generator<DataBlockParent> generator,
        Func<DataBlockVersion> version)
        => generator.ForInstance(d => d.SetLatestPublishedVersion(version));

    public static InstanceSetters<DataBlockParent> SetDefaults(this InstanceSetters<DataBlockParent> setters)
        => setters
            .SetDefault(d => d.Id);

    public static InstanceSetters<DataBlockParent> SetVersions(
        this InstanceSetters<DataBlockParent> setters,
        IEnumerable<DataBlockVersion> versions) => setters
            .Set(parent => parent.Versions,
                (_, dataBlockParent, _) =>
                {
                    versions.ForEach(version =>
                    {
                        version.DataBlockParent = dataBlockParent;
                        version.DataBlockParentId = dataBlockParent.Id;
                    });

                    return versions;
                })
            .Set(
                parent => parent.LatestVersion,
                versions
                    .OrderBy(version => version.Version)
                    .Last)
            .Set(
                parent => parent.LatestPublishedVersion,
                versions
                    .Where(version => version.Published != null)
                    .OrderBy(version => version.Version)
                    .LastOrDefault);

    public static InstanceSetters<DataBlockParent> SetLatestVersion(
        this InstanceSetters<DataBlockParent> setters,
        DataBlockVersion version)
        => setters
            .Set(d => d.LatestVersion, version)
            .Set(parent => parent.Versions,
                (_, dataBlockParent, _) => dataBlockParent.Versions.Concat(ListOf(version)).ToList());

    public static InstanceSetters<DataBlockParent> SetLatestPublishedVersion(
        this InstanceSetters<DataBlockParent> setters,
        DataBlockVersion version)
        => SetLatestPublishedVersion(setters, () => version);

    public static InstanceSetters<DataBlockParent> SetLatestPublishedVersion(
        this InstanceSetters<DataBlockParent> setters,
        Func<DataBlockVersion> version)
        => setters
            .Set(d => d.LatestPublishedVersion, version)
            .Set(parent => parent.Versions,
                (_, dataBlockParent, _) => dataBlockParent.Versions.Concat(ListOf(version.Invoke())).ToList());
}
