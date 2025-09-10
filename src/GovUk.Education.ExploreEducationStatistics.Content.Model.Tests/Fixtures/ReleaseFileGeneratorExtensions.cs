using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Semver;

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
            .SetDefault(rf => rf.Summary)
            .SetDefault(rf => rf.Published);

    public static Generator<ReleaseFile> WithId(
        this Generator<ReleaseFile> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<ReleaseFile> WithFile(
        this Generator<ReleaseFile> generator,
        File file)
        => generator.ForInstance(s => s.SetFile(file));

    public static Generator<ReleaseFile> WithFile(
        this Generator<ReleaseFile> generator,
        Func<File> file)
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

    public static Generator<ReleaseFile> WithFilterSequence(
        this Generator<ReleaseFile> generator,
        List<FilterSequenceEntry> sequence)
        => generator.ForInstance(s => s.SetFilterSequence(sequence));

    public static Generator<ReleaseFile> WithIndicatorSequence(
        this Generator<ReleaseFile> generator,
        List<IndicatorGroupSequenceEntry> sequence)
        => generator.ForInstance(s => s.SetIndicatorSequence(sequence));

    public static Generator<ReleaseFile> WithPublicApiDataSetId(
        this Generator<ReleaseFile> generator,
        Guid publicApiDataSetId)
        => generator.ForInstance(s => s.SetPublicApiDataSetId(publicApiDataSetId));

    public static Generator<ReleaseFile> WithPublicApiDataSetVersion(
        this Generator<ReleaseFile> generator,
        SemVersion version)
        => generator.ForInstance(s => s.SetPublicApiDataSetVersion(version));

    public static Generator<ReleaseFile> WithPublicApiDataSetVersion(
        this Generator<ReleaseFile> generator,
        int major,
        int minor,
        int patch = 0)
        => generator.ForInstance(s => s.SetPublicApiDataSetVersion(major, minor, patch));

    public static InstanceSetters<ReleaseFile> SetId(
        this InstanceSetters<ReleaseFile> setters,
        Guid id)
        => setters.Set(rf => rf.Id, id);

    public static InstanceSetters<ReleaseFile> SetFile(
        this InstanceSetters<ReleaseFile> setters,
        File file)
        => setters.SetFile(() => file);

    public static InstanceSetters<ReleaseFile> SetFile(
        this InstanceSetters<ReleaseFile> setters,
        Func<File> file)
        => setters
            .Set(rf => rf.File, file)
            .Set(rf => rf.FileId, (_, rf) => rf.File.Id);

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

    public static InstanceSetters<ReleaseFile> SetFilterSequence(
        this InstanceSetters<ReleaseFile> setters,
        List<FilterSequenceEntry> sequence)
        => setters.Set(rf => rf.FilterSequence, sequence);

    public static InstanceSetters<ReleaseFile> SetIndicatorSequence(
        this InstanceSetters<ReleaseFile> setters,
        List<IndicatorGroupSequenceEntry> sequence)
        => setters.Set(rf => rf.IndicatorSequence, sequence);

    public static InstanceSetters<ReleaseFile> SetPublicApiDataSetId(
        this InstanceSetters<ReleaseFile> setters,
        Guid publicApiDataSetId)
        => setters.Set(rf => rf.PublicApiDataSetId, publicApiDataSetId);

    public static InstanceSetters<ReleaseFile> SetPublicApiDataSetVersion(
        this InstanceSetters<ReleaseFile> setters,
        int major,
        int minor,
        int patch = 0)
        => setters.Set(
            rf => rf.PublicApiDataSetVersion,
            new SemVersion(major: major, minor: minor, patch: patch));

    public static InstanceSetters<ReleaseFile> SetPublicApiDataSetVersion(
        this InstanceSetters<ReleaseFile> setters,
        SemVersion version)
        => setters.Set(rf => rf.PublicApiDataSetVersion, version);
}
