using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class DataSetVersionGeneratorExtensions
{
    public static Generator<DataSetVersion> DefaultDataSetVersion(this DataFixture fixture)
        => fixture.Generator<DataSetVersion>().WithDefaults();

    public static Generator<DataSetVersion> DefaultDataSetVersion(
        this DataFixture fixture,
        int filters,
        int indicators,
        int locations,
        int timePeriods,
        int maxFilterOptions = 10,
        int maxLocationOptions = 10)
        => fixture
            .Generator<DataSetVersion>()
            .WithDefaults()
            .WithMeta(
                fixture.DefaultDataSetMeta(
                    filters: filters,
                    indicators: indicators,
                    locations: locations,
                    timePeriods: timePeriods,
                    maxFilterOptions: maxFilterOptions,
                    maxLocationOptions: maxLocationOptions
                )
            );

    public static Generator<DataSetVersion> WithDefaults(this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<DataSetVersion> WithDataSet(
        this Generator<DataSetVersion> generator,
        DataSet dataSet)
        => generator.ForInstance(s => s.SetDataSet(dataSet));

    public static Generator<DataSetVersion> WithDataSetId(
        this Generator<DataSetVersion> generator,
        Guid dataSetId)
        => generator.ForInstance(s => s.SetDataSetId(dataSetId));

    public static Generator<DataSetVersion> WithCsvFileId(
        this Generator<DataSetVersion> generator,
        Guid csvFileId)
        => generator.ForInstance(s => s.SetCsvFileId(csvFileId));

    public static Generator<DataSetVersion> WithParquetFilename(
        this Generator<DataSetVersion> generator,
        string parquetFilename)
        => generator.ForInstance(s => s.SetParquetFilename(parquetFilename));

    public static Generator<DataSetVersion> WithVersionNumber(
        this Generator<DataSetVersion> generator,
        int major,
        int minor)
        => generator.ForInstance(s => s.SetVersionNumber(major, minor));

    public static Generator<DataSetVersion> WithStatus(
        this Generator<DataSetVersion> generator,
        DataSetVersionStatus status)
        => generator.ForInstance(s => s.SetStatus(status));

    public static Generator<DataSetVersion> WithStatusPublished(this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetStatusPublished());

    public static Generator<DataSetVersion> WithStatusStaged(this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetStatusStaged());

    public static Generator<DataSetVersion> WithStatusUnpublished(this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetStatusUnpublished());

    public static Generator<DataSetVersion> WithMetaSummary(
        this Generator<DataSetVersion> generator,
        DataSetVersionMetaSummary metaSummary)
        => generator.ForInstance(s => s.SetMetaSummary(metaSummary));

    public static Generator<DataSetVersion> WithMeta(
        this Generator<DataSetVersion> generator,
        DataSetMeta meta)
        => generator.ForInstance(s => s.SetMeta(meta));

    public static Generator<DataSetVersion> WithTotalResults(
        this Generator<DataSetVersion> generator,
        long totalResults)
        => generator.ForInstance(s => s.SetTotalResults(totalResults));

    public static Generator<DataSetVersion> WithFilterChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<ChangeSetFilters> filterChanges)
        => generator.ForInstance(s => s.SetFilterChanges(filterChanges));

    public static Generator<DataSetVersion> WithFilterOptionChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<ChangeSetFilterOptions> filterOptionChanges)
        => generator.ForInstance(s => s.SetFilterOptionChanges(filterOptionChanges));

    public static Generator<DataSetVersion> WithIndicatorChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<ChangeSetIndicators> indicatorChanges)
        => generator.ForInstance(s => s.SetIndicatorChanges(indicatorChanges));

    public static Generator<DataSetVersion> WithLocationChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<ChangeSetLocations> locationChanges)
        => generator.ForInstance(s => s.SetLocationChanges(locationChanges));

    public static Generator<DataSetVersion> WithTimePeriodChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<ChangeSetTimePeriods> timePeriodChanges)
        => generator.ForInstance(s => s.SetTimePeriodChanges(timePeriodChanges));

    public static InstanceSetters<DataSetVersion> SetDefaults(this InstanceSetters<DataSetVersion> setters)
        => setters
            .SetDefault(dsv => dsv.Id)
            .SetDefault(dsv => dsv.DataSetId)
            .SetDefault(dsv => dsv.CsvFileId)
            .SetDefault(dsv => dsv.ParquetFilename)
            .SetDefault(dsv => dsv.Notes)
            .Set(dsv => dsv.VersionMajor, 1)
            .Set(dsv => dsv.VersionMinor, (_, _, context) => context.Index)
            .Set(dsv => dsv.TotalResults, f => f.Random.Long(min: 10000, max: 10_000_000))
            .Set(dsv => dsv.Status, DataSetVersionStatus.Staged)
            .Set(dsv => dsv.Created, dsv => dsv.Date.PastOffset())
            .Set(
                dsv => dsv.Updated,
                (f, dataSet) => f.Date.SoonOffset(14, dataSet.Created)
            );

    public static InstanceSetters<DataSetVersion> SetDataSet(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DataSet dataSet)
        => instanceSetter
            .Set(dsv => dsv.DataSet, dataSet)
            .Set(dsv => dsv.DataSetId, dataSet.Id);

    public static InstanceSetters<DataSetVersion> SetDataSetId(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Guid dataSetId)
        => instanceSetter
            .Set(dsv => dsv.DataSetId, dataSetId);

    public static InstanceSetters<DataSetVersion> SetCsvFileId(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Guid csvFileId)
        => instanceSetter.Set(dsv => dsv.CsvFileId, csvFileId);

    public static InstanceSetters<DataSetVersion> SetParquetFilename(
        this InstanceSetters<DataSetVersion> instanceSetter,
        string parquetFilename)
        => instanceSetter.Set(dsv => dsv.ParquetFilename, parquetFilename);

    public static InstanceSetters<DataSetVersion> SetVersionNumber(
        this InstanceSetters<DataSetVersion> instanceSetter,
        int major,
        int minor)
        => instanceSetter
            .Set(dsv => dsv.VersionMajor, major)
            .Set(dsv => dsv.VersionMinor, minor);

    public static InstanceSetters<DataSetVersion> SetStatus(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DataSetVersionStatus status)
        => instanceSetter.Set(dsv => dsv.Status, status);

    public static InstanceSetters<DataSetVersion> SetStatusPublished(
        this InstanceSetters<DataSetVersion> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetVersionStatus.Published)
            .SetPublished(DateTimeOffset.UtcNow)
            .SetUnpublished(null);

    public static InstanceSetters<DataSetVersion> SetStatusStaged(this InstanceSetters<DataSetVersion> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetVersionStatus.Staged)
            .SetUnpublished(null)
            .SetPublished(null);

    public static InstanceSetters<DataSetVersion> SetStatusUnpublished(
        this InstanceSetters<DataSetVersion> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetVersionStatus.Published)
            .SetUnpublished(DateTimeOffset.UtcNow)
            .SetPublished(null);

    public static InstanceSetters<DataSetVersion> SetPublished(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DateTimeOffset? published)
        => instanceSetter.Set(dsv => dsv.Published, published);

    public static InstanceSetters<DataSetVersion> SetUnpublished(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DateTimeOffset? unpublished)
        => instanceSetter.Set(dsv => dsv.Unpublished, unpublished);

    public static InstanceSetters<DataSetVersion> SetMetaSummary(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DataSetVersionMetaSummary metaSummary)
        => instanceSetter.Set(dsv => dsv.MetaSummary, metaSummary);

    public static InstanceSetters<DataSetVersion> SetMeta(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DataSetMeta meta)
        => instanceSetter.Set(
            (_, dsv) =>
            {
                dsv.Meta = meta;
                dsv.MetaSummary = meta.ToSummary();

                meta.DataSetVersion = dsv;
                meta.DataSetVersionId = dsv.Id;
            }
        );

    public static InstanceSetters<DataSetVersion> SetTotalResults(
        this InstanceSetters<DataSetVersion> instanceSetter,
        long totalResults)
        => instanceSetter.Set(dsv => dsv.TotalResults, totalResults);

    public static InstanceSetters<DataSetVersion> SetFilterChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<ChangeSetFilters> filterChanges)
        => instanceSetter.Set(
            dsv => dsv.FilterChanges,
            (_, dsv) => filterChanges.Select(
                    changeSet =>
                    {
                        changeSet.DataSetVersionId = dsv.Id;
                        changeSet.DataSetVersion = dsv;
                        return changeSet;
                    }
                )
                .ToList()
        );

    public static InstanceSetters<DataSetVersion> SetFilterOptionChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<ChangeSetFilterOptions> filterOptionChanges)
        => instanceSetter.Set(
            dsv => dsv.FilterOptionChanges,
            (_, dsv) => filterOptionChanges.Select(
                    changeSet =>
                    {
                        changeSet.DataSetVersionId = dsv.Id;
                        changeSet.DataSetVersion = dsv;
                        return changeSet;
                    }
                )
                .ToList()
        );

    public static InstanceSetters<DataSetVersion> SetIndicatorChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<ChangeSetIndicators> indicatorChanges)
        => instanceSetter.Set(
            dsv => dsv.IndicatorChanges,
            (_, dsv) => indicatorChanges.Select(
                    changeSet =>
                    {
                        changeSet.DataSetVersionId = dsv.Id;
                        changeSet.DataSetVersion = dsv;
                        return changeSet;
                    }
                )
                .ToList()
        );

    public static InstanceSetters<DataSetVersion> SetLocationChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<ChangeSetLocations> locationChanges)
        => instanceSetter.Set(
            dsv => dsv.LocationChanges,
            (_, dsv) => locationChanges.Select(
                    changeSet =>
                    {
                        changeSet.DataSetVersionId = dsv.Id;
                        changeSet.DataSetVersion = dsv;
                        return changeSet;
                    }
                )
                .ToList()
        );

    public static InstanceSetters<DataSetVersion> SetTimePeriodChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<ChangeSetTimePeriods> timePeriodChanges)
        => instanceSetter.Set(
            dsv => dsv.TimePeriodChanges,
            (_, dsv) => timePeriodChanges.Select(
                    changeSet =>
                    {
                        changeSet.DataSetVersionId = dsv.Id;
                        changeSet.DataSetVersion = dsv;
                        return changeSet;
                    }
                )
                .ToList()
        );
}
