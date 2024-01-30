using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class DataSetGeneratorExtensions
{
    public static Generator<DataSet> DefaultDataSet(this DataFixture fixture)
        => fixture.Generator<DataSet>().WithDefaults();

    public static Generator<DataSet> WithDefaults(this Generator<DataSet> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<DataSet> WithPublicationId(this Generator<DataSet> generator, Guid publicationId)
        => generator.ForInstance(s => s.SetPublicationId(publicationId));

    public static Generator<DataSet> WithTitle(this Generator<DataSet> generator, string title)
        => generator.ForInstance(s => s.SetTitle(title));

    public static Generator<DataSet> WithSummary(this Generator<DataSet> generator, string summary)
        => generator.ForInstance(s => s.SetSummary(summary));

    public static Generator<DataSet> WithStatus(this Generator<DataSet> generator, DataSetStatus status)
        => generator.ForInstance(s => s.SetStatus(status));

    public static Generator<DataSet> WithStatusPublished(this Generator<DataSet> generator)
        => generator.ForInstance(s => s.SetStatusPublished());

    public static Generator<DataSet> WithStatusStaged(this Generator<DataSet> generator)
        => generator.ForInstance(s => s.SetStatusStaged());

    public static Generator<DataSet> WithStatusUnpublished(this Generator<DataSet> generator)
        => generator.ForInstance(s => s.SetStatusUnpublished());

    public static Generator<DataSet> WithSupersedingDataSet(this Generator<DataSet> generator, DataSet? dataSet)
        => generator.ForInstance(s => s.SetSupersedingDataSet(dataSet));

    public static Generator<DataSet> WithLatestVersion(
        this Generator<DataSet> generator,
        DataSetVersion? dataSetVersion)
        => generator.ForInstance(s => s.SetLatestVersion(dataSetVersion));

    public static Generator<DataSet> WithVersions(
        this Generator<DataSet> generator,
        IEnumerable<DataSetVersion> versions)
        => generator.ForInstance(s => s.SetVersions(versions));

    public static InstanceSetters<DataSet> SetDefaults(this InstanceSetters<DataSet> setters)
        => setters
            .SetDefault(ds => ds.Id)
            .SetDefault(ds => ds.Title)
            .SetDefault(ds => ds.Summary)
            .SetDefault(ds => ds.PublicationId)
            .Set(ds => ds.Status, DataSetStatus.Staged)
            .Set(ds => ds.Created, ds => ds.Date.PastOffset())
            .Set(
                ds => ds.Updated,
                (f, ds) => f.Date.SoonOffset(14, ds.Created)
            );

    public static InstanceSetters<DataSet> SetTitle(
        this InstanceSetters<DataSet> instanceSetter,
        string title)
        => instanceSetter.Set(ds => ds.Title, title);

    public static InstanceSetters<DataSet> SetSummary(
        this InstanceSetters<DataSet> instanceSetter,
        string summary)
        => instanceSetter.Set(ds => ds.Summary, summary);

    public static InstanceSetters<DataSet> SetPublicationId(
        this InstanceSetters<DataSet> instanceSetter,
        Guid publicationId)
        => instanceSetter.Set(ds => ds.PublicationId, publicationId);

    public static InstanceSetters<DataSet> SetStatus(
        this InstanceSetters<DataSet> instanceSetter,
        DataSetStatus status)
        => instanceSetter.Set(ds => ds.Status, status);

    public static InstanceSetters<DataSet> SetStatusPublished(this InstanceSetters<DataSet> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetStatus.Published)
            .SetPublished(DateTimeOffset.UtcNow)
            .SetUnpublished(null);

    public static InstanceSetters<DataSet> SetStatusStaged(this InstanceSetters<DataSet> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetStatus.Staged)
            .SetUnpublished(null)
            .SetPublished(null);

    public static InstanceSetters<DataSet> SetStatusUnpublished(this InstanceSetters<DataSet> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetStatus.Published)
            .SetUnpublished(DateTimeOffset.UtcNow)
            .SetPublished(null);

    public static InstanceSetters<DataSet> SetPublished(
        this InstanceSetters<DataSet> instanceSetter,
        DateTimeOffset? published)
        => instanceSetter.Set(ds => ds.Published, published);

    public static InstanceSetters<DataSet> SetUnpublished(
        this InstanceSetters<DataSet> instanceSetter,
        DateTimeOffset? unpublished)
        => instanceSetter.Set(ds => ds.Unpublished, unpublished);

    public static InstanceSetters<DataSet> SetSupersedingDataSet(
        this InstanceSetters<DataSet> instanceSetter,
        DataSet? dataSet)
        => instanceSetter
            .Set(ds => ds.SupersedingDataSet, dataSet)
            .Set(ds => ds.SupersedingDataSetId, dataSet?.Id);

    public static InstanceSetters<DataSet> SetLatestVersion(
        this InstanceSetters<DataSet> instanceSetter,
        DataSetVersion? dataSetVersion)
        => instanceSetter
            .Set(
                (_, ds) =>
                {
                    if (dataSetVersion is not null)
                    {
                        dataSetVersion.DataSet = ds;
                        dataSetVersion.DataSetId = ds.Id;

                        if (!ds.Versions.Contains(dataSetVersion))
                        {
                            ds.Versions.Add(dataSetVersion);
                        }
                    }

                    ds.LatestVersion = dataSetVersion;
                    ds.LatestVersionId = dataSetVersion?.Id;
                }
            );

    public static InstanceSetters<DataSet> SetVersions(
        this InstanceSetters<DataSet> instanceSetter,
        IEnumerable<DataSetVersion> dataSetVersions)
        => instanceSetter
            .Set(
                (_, ds) =>
                {
                    ds.Versions = dataSetVersions
                        .Select(
                            version =>
                            {
                                version.DataSet = ds;
                                version.DataSetId = ds.Id;

                                return version;
                            }
                        )
                        .ToList();

                    ds.LatestVersion = ds.Versions
                        .Where(version => version.Status == DataSetVersionStatus.Published)
                        .OrderBy(version => version.VersionMajor)
                        .ThenBy(version => version.VersionMinor)
                        .LastOrDefault();
                }
            );
}
