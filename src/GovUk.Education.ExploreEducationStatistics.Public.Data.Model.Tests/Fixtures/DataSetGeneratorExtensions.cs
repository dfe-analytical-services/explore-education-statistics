using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class DataSetGeneratorExtensions
{
    public static Generator<DataSet> DefaultDataSet(this DataFixture fixture) =>
        fixture.Generator<DataSet>().WithDefaults();

    public static Generator<DataSet> WithDefaults(this Generator<DataSet> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<DataSet> WithPublicationId(
        this Generator<DataSet> generator,
        Guid publicationId
    ) => generator.ForInstance(s => s.SetPublicationId(publicationId));

    public static Generator<DataSet> WithId(this Generator<DataSet> generator, Guid id) =>
        generator.ForInstance(s => s.SetId(id));

    public static Generator<DataSet> WithTitle(this Generator<DataSet> generator, string title) =>
        generator.ForInstance(s => s.SetTitle(title));

    public static Generator<DataSet> WithSummary(
        this Generator<DataSet> generator,
        string summary
    ) => generator.ForInstance(s => s.SetSummary(summary));

    public static Generator<DataSet> WithStatus(
        this Generator<DataSet> generator,
        DataSetStatus status
    ) => generator.ForInstance(s => s.SetStatus(status));

    public static Generator<DataSet> WithStatusPublished(this Generator<DataSet> generator) =>
        generator.ForInstance(s => s.SetStatusPublished());

    public static Generator<DataSet> WithStatusDraft(this Generator<DataSet> generator) =>
        generator.ForInstance(s => s.SetStatusDraft());

    public static Generator<DataSet> WithStatusUnpublished(this Generator<DataSet> generator) =>
        generator.ForInstance(s => s.SetStatusWithdrawn());

    public static Generator<DataSet> WithPublished(
        this Generator<DataSet> generator,
        DateTimeOffset? published
    ) => generator.ForInstance(s => s.SetPublished(published));

    public static Generator<DataSet> WithWithdrawn(
        this Generator<DataSet> generator,
        DateTimeOffset? withdrawn
    ) => generator.ForInstance(s => s.SetWithdrawn(withdrawn));

    public static Generator<DataSet> WithSupersedingDataSet(
        this Generator<DataSet> generator,
        DataSet? dataSet
    ) => generator.ForInstance(s => s.SetSupersedingDataSet(dataSet));

    public static Generator<DataSet> WithLatestDraftVersion(
        this Generator<DataSet> generator,
        DataSetVersion? dataSetVersion
    ) => generator.ForInstance(s => s.SetLatestDraftVersion(dataSetVersion));

    public static Generator<DataSet> WithLatestDraftVersion(
        this Generator<DataSet> generator,
        Func<DataSetVersion?> dataSetVersion
    ) => generator.ForInstance(s => s.SetLatestDraftVersion(dataSetVersion));

    public static Generator<DataSet> WithLatestLiveVersion(
        this Generator<DataSet> generator,
        DataSetVersion? dataSetVersion
    ) => generator.ForInstance(s => s.SetLatestLiveVersion(dataSetVersion));

    public static Generator<DataSet> WithLatestLiveVersion(
        this Generator<DataSet> generator,
        Func<DataSetVersion?> dataSetVersion
    ) => generator.ForInstance(s => s.SetLatestLiveVersion(dataSetVersion));

    public static Generator<DataSet> WithVersions(
        this Generator<DataSet> generator,
        IEnumerable<DataSetVersion> versions
    ) => generator.ForInstance(s => s.SetVersions(versions));

    public static Generator<DataSet> WithVersions(
        this Generator<DataSet> generator,
        Func<IEnumerable<DataSetVersion>> versions
    ) => generator.ForInstance(s => s.SetVersions(versions));

    public static InstanceSetters<DataSet> SetDefaults(this InstanceSetters<DataSet> setters) =>
        setters
            .SetDefault(ds => ds.Id)
            .SetDefault(ds => ds.Title)
            .SetDefault(ds => ds.Summary)
            .SetDefault(ds => ds.PublicationId)
            .Set(ds => ds.Status, DataSetStatus.Draft);

    public static InstanceSetters<DataSet> SetId(
        this InstanceSetters<DataSet> instanceSetter,
        Guid id
    ) => instanceSetter.Set(ds => ds.Id, id);

    public static InstanceSetters<DataSet> SetTitle(
        this InstanceSetters<DataSet> instanceSetter,
        string title
    ) => instanceSetter.Set(ds => ds.Title, title);

    public static InstanceSetters<DataSet> SetSummary(
        this InstanceSetters<DataSet> instanceSetter,
        string summary
    ) => instanceSetter.Set(ds => ds.Summary, summary);

    public static InstanceSetters<DataSet> SetPublicationId(
        this InstanceSetters<DataSet> instanceSetter,
        Guid publicationId
    ) => instanceSetter.Set(ds => ds.PublicationId, publicationId);

    public static InstanceSetters<DataSet> SetStatus(
        this InstanceSetters<DataSet> instanceSetter,
        DataSetStatus status
    ) => instanceSetter.Set(ds => ds.Status, status);

    public static InstanceSetters<DataSet> SetStatusPublished(
        this InstanceSetters<DataSet> instanceSetter
    ) =>
        instanceSetter
            .SetStatus(DataSetStatus.Published)
            .SetPublished(DateTimeOffset.UtcNow.AddDays(-1))
            .SetWithdrawn(null);

    public static InstanceSetters<DataSet> SetStatusDraft(
        this InstanceSetters<DataSet> instanceSetter
    ) => instanceSetter.SetStatus(DataSetStatus.Draft).SetWithdrawn(null).SetPublished(null);

    public static InstanceSetters<DataSet> SetStatusWithdrawn(
        this InstanceSetters<DataSet> instanceSetter
    ) =>
        instanceSetter
            .SetStatus(DataSetStatus.Withdrawn)
            .SetWithdrawn(DateTimeOffset.UtcNow.AddDays(-1))
            .Set((_, dsv) => dsv.Published ??= DateTimeOffset.UtcNow.AddDays(-2));

    public static InstanceSetters<DataSet> SetPublished(
        this InstanceSetters<DataSet> instanceSetter,
        DateTimeOffset? published
    ) => instanceSetter.Set(ds => ds.Published, published);

    public static InstanceSetters<DataSet> SetWithdrawn(
        this InstanceSetters<DataSet> instanceSetter,
        DateTimeOffset? withdrawn
    ) => instanceSetter.Set(ds => ds.Withdrawn, withdrawn);

    public static InstanceSetters<DataSet> SetSupersedingDataSet(
        this InstanceSetters<DataSet> instanceSetter,
        DataSet? dataSet
    ) =>
        instanceSetter
            .Set(ds => ds.SupersedingDataSet, dataSet)
            .Set(ds => ds.SupersedingDataSetId, dataSet?.Id);

    public static InstanceSetters<DataSet> SetLatestLiveVersion(
        this InstanceSetters<DataSet> instanceSetter,
        DataSetVersion? dataSetVersion
    ) => instanceSetter.SetLatestLiveVersion(() => dataSetVersion);

    public static InstanceSetters<DataSet> SetLatestLiveVersion(
        this InstanceSetters<DataSet> instanceSetter,
        Func<DataSetVersion?> dataSetVersion
    ) =>
        instanceSetter.Set(
            (_, ds) =>
            {
                var dsv = dataSetVersion();

                if (dsv is not null)
                {
                    dsv.DataSet = ds;
                    dsv.DataSetId = ds.Id;

                    if (!ds.Versions.Contains(dsv))
                    {
                        ds.Versions.Add(dsv);
                    }
                }

                ds.LatestLiveVersion = dsv;
                ds.LatestLiveVersionId = dsv?.Id;
            }
        );

    public static InstanceSetters<DataSet> SetLatestDraftVersion(
        this InstanceSetters<DataSet> instanceSetter,
        DataSetVersion? dataSetVersion
    ) => instanceSetter.SetLatestDraftVersion(() => dataSetVersion);

    public static InstanceSetters<DataSet> SetLatestDraftVersion(
        this InstanceSetters<DataSet> instanceSetter,
        Func<DataSetVersion?> dataSetVersion
    ) =>
        instanceSetter.Set(
            (_, ds) =>
            {
                var dsv = dataSetVersion();

                if (dsv is not null)
                {
                    dsv.DataSet = ds;
                    dsv.DataSetId = ds.Id;

                    if (!ds.Versions.Contains(dsv))
                    {
                        ds.Versions.Add(dsv);
                    }
                }

                ds.LatestDraftVersion = dsv;
                ds.LatestDraftVersionId = dsv?.Id;
            }
        );

    public static InstanceSetters<DataSet> SetVersions(
        this InstanceSetters<DataSet> instanceSetter,
        IEnumerable<DataSetVersion> dataSetVersions
    ) => instanceSetter.SetVersions(() => dataSetVersions);

    public static InstanceSetters<DataSet> SetVersions(
        this InstanceSetters<DataSet> instanceSetter,
        Func<IEnumerable<DataSetVersion>> dataSetVersions
    ) =>
        instanceSetter.Set(
            (_, ds) =>
            {
                var dsvs = dataSetVersions();

                ds.Versions = dsvs.Select(version =>
                    {
                        version.DataSet = ds;
                        version.DataSetId = ds.Id;

                        return version;
                    })
                    .ToList();

                ds.LatestLiveVersion = ds
                    .Versions.Where(version => version.Status == DataSetVersionStatus.Published)
                    .OrderBy(version => version.VersionMajor)
                    .ThenBy(version => version.VersionMinor)
                    .LastOrDefault();
            }
        );
}
