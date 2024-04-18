using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseFileGeneratorExtensions
{
    public static Generator<ReleaseFile> DefaultReleaseFile(this DataFixture fixture)
        => fixture.Generator<ReleaseFile>().WithDefaults();

    public static Generator<ReleaseFile> WithDefaults(this Generator<ReleaseFile> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<ReleaseFile> SetDefaults(this InstanceSetters<ReleaseFile> setters)
        => setters
            .SetDefault(rf => rf.Id)
            .SetDefault(rf => rf.Name)
            .SetDefault(rf => rf.Order)
            .SetDefault(rf => rf.Summary);

    public static Generator<ReleaseFile> WithFile(
        this Generator<ReleaseFile> generator,
        File file)
        => generator.ForInstance(s => s.SetFile(file));

    public static Generator<ReleaseFile> WithFileId(
        this Generator<ReleaseFile> generator,
        Guid fileId)
        => generator.ForInstance(s => s.SetFileId(fileId));

    public static Generator<ReleaseFile> WithFiles(this Generator<ReleaseFile> generator,
        IEnumerable<File> files)
    {
        files.ForEach((file, index) =>
            generator.ForIndex(index, s => s.SetFile(file)));

        return generator;
    }

    public static Generator<ReleaseFile> WithName(
        this Generator<ReleaseFile> generator,
        string name)
        => generator.ForInstance(s => s.SetName(name));

    public static Generator<ReleaseFile> WithOrder(
        this Generator<ReleaseFile> generator,
        int order)
        => generator.ForInstance(s => s.SetOrder(order));

    public static Generator<ReleaseFile> WithReleaseVersion(
        this Generator<ReleaseFile> generator,
        ReleaseVersion releaseVersion)
        => generator.ForInstance(s => s.SetReleaseVersion(releaseVersion));

    public static Generator<ReleaseFile> WithReleaseVersions(this Generator<ReleaseFile> generator,
        IEnumerable<ReleaseVersion> releaseVersions)
    {
        releaseVersions.ForEach((releaseVersion, index) =>
            generator.ForIndex(index, s => s.SetReleaseVersion(releaseVersion)));

        return generator;
    }

    public static Generator<ReleaseFile> WithReleaseVersionId(
        this Generator<ReleaseFile> generator,
        Guid releaseVersionId)
        => generator.ForInstance(s => s.SetReleaseVersionId(releaseVersionId));

    public static Generator<ReleaseFile> WithSummary(
        this Generator<ReleaseFile> generator,
        string summary)
        => generator.ForInstance(s => s.SetSummary(summary));

    public static InstanceSetters<ReleaseFile> SetFile(
        this InstanceSetters<ReleaseFile> setters,
        File file)
        => setters
            .Set(rf => rf.File, file)
            .SetFileId(file.Id);

    public static InstanceSetters<ReleaseFile> SetFileId(
        this InstanceSetters<ReleaseFile> setters,
        Guid fileId)
        => setters.Set(rf => rf.FileId, fileId);

    public static InstanceSetters<ReleaseFile> SetName(
        this InstanceSetters<ReleaseFile> setters,
        string name)
        => setters.Set(rf => rf.Name, name);

    public static InstanceSetters<ReleaseFile> SetOrder(
        this InstanceSetters<ReleaseFile> setters,
        int order)
        => setters.Set(rf => rf.Order, order);

    public static InstanceSetters<ReleaseFile> SetReleaseVersion(
        this InstanceSetters<ReleaseFile> setters,
        ReleaseVersion releaseVersion)
        => setters.Set(rf => rf.ReleaseVersion, releaseVersion)
            .SetReleaseVersionId(releaseVersion.Id);

    public static InstanceSetters<ReleaseFile> SetReleaseVersionId(
        this InstanceSetters<ReleaseFile> setters,
        Guid releaseVersionId)
        => setters.Set(rf => rf.ReleaseVersionId, releaseVersionId);

    public static InstanceSetters<ReleaseFile> SetSummary(
        this InstanceSetters<ReleaseFile> setters,
        string summary)
        => setters.Set(rf => rf.Summary, summary);
}
