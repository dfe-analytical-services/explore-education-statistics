using Bogus;
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
            .DefaultDataSetVersion()
            .WithFilterMetas(faker => fixture
                .DefaultFilterMeta(options: faker.Random.Int(1, maxFilterOptions))
                .Generate(filters))
            .WithIndicatorMetas(() => fixture.DefaultIndicatorMeta().Generate(indicators))
            .WithLocationMetas(faker => fixture
                .DefaultLocationMeta(options: faker.Random.Int(1, maxLocationOptions))
                .Generate(locations))
            .WithGeographicLevelMeta()
            .WithTimePeriodMetas(() => fixture.DefaultTimePeriodMeta().Generate(timePeriods))
            .WithMetaSummary();

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

    public static Generator<DataSetVersion> WithVersionNumber(
        this Generator<DataSetVersion> generator,
        int major,
        int minor)
        => generator.ForInstance(s => s.SetVersionNumber(major, minor));

    public static Generator<DataSetVersion> WithPublished(
        this Generator<DataSetVersion> generator,
        DateTimeOffset published)
        => generator.ForInstance(s => s.SetPublished(published));

    public static Generator<DataSetVersion> WithWithdrawn(
        this Generator<DataSetVersion> generator,
        DateTimeOffset withdrawn)
        => generator.ForInstance(s => s.SetWithdrawn(withdrawn));

    public static Generator<DataSetVersion> WithStatus(
        this Generator<DataSetVersion> generator,
        DataSetVersionStatus status)
        => generator.ForInstance(s => s.SetStatus(status));

    public static Generator<DataSetVersion> WithStatusPublished(this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetStatusPublished());

    public static Generator<DataSetVersion> WithStatusDraft(this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetStatusDraft());

    public static Generator<DataSetVersion> WithStatusProcessing(this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetStatusProcessing());

    public static Generator<DataSetVersion> WithStatusFailed(this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetStatusFailed());

    public static Generator<DataSetVersion> WithStatusMapping(this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetStatusMapping());

    public static Generator<DataSetVersion> WithStatusWithdrawn(this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetStatusWithdrawn());

    public static Generator<DataSetVersion> WithTotalResults(
        this Generator<DataSetVersion> generator,
        long totalResults)
        => generator.ForInstance(s => s.SetTotalResults(totalResults));

    public static Generator<DataSetVersion> WithMetaSummary(
        this Generator<DataSetVersion> generator,
        DataSetVersionMetaSummary metaSummary)
        => generator.ForInstance(s => s.SetMetaSummary(metaSummary));

    public static Generator<DataSetVersion> WithMetaSummary(
        this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetMetaSummary());

    public static Generator<DataSetVersion> WithGeographicLevelMeta(
        this Generator<DataSetVersion> generator,
        GeographicLevelMeta meta)
        => generator.ForInstance(s => s.SetGeographicLevelMeta(meta));

    public static Generator<DataSetVersion> WithGeographicLevelMeta(
        this Generator<DataSetVersion> generator,
        Func<GeographicLevelMeta> meta)
        => generator.ForInstance(s => s.SetGeographicLevelMeta(meta));

    public static Generator<DataSetVersion> WithGeographicLevelMeta(
        this Generator<DataSetVersion> generator)
        => generator.ForInstance(s => s.SetGeographicLevelMeta());

    public static Generator<DataSetVersion> WithImports(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<DataSetVersionImport>> imports)
        => generator.ForInstance(s => s.SetImports(imports));

    public static Generator<DataSetVersion> WithLocationMetas(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<LocationMeta>> metas)
        => generator.ForInstance(s => s.SetLocationMetas(metas));

    public static Generator<DataSetVersion> WithLocationMetas(
        this Generator<DataSetVersion> generator,
        Func<Faker, IEnumerable<LocationMeta>> metas)
        => generator.ForInstance(s => s.SetLocationMetas(metas));

    public static Generator<DataSetVersion> WithFilterMetas(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<FilterMeta>> metas)
        => generator.ForInstance(s => s.SetFilterMetas(metas));

    public static Generator<DataSetVersion> WithFilterMetas(
        this Generator<DataSetVersion> generator,
        Func<Faker, IEnumerable<FilterMeta>> metas)
        => generator.ForInstance(s => s.SetFilterMetas(metas));

    public static Generator<DataSetVersion> WithIndicatorMetas(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<IndicatorMeta>> metas)
        => generator.ForInstance(s => s.SetIndicatorMetas(metas));

    public static Generator<DataSetVersion> WithTimePeriodMetas(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<TimePeriodMeta>> metas)
        => generator.ForInstance(s => s.SetTimePeriodMetas(metas));

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
            .SetDefault(dsv => dsv.Notes)
            .Set(dsv => dsv.VersionMajor, 1)
            .Set(dsv => dsv.VersionMinor, (_, _, context) => context.Index)
            .Set(dsv => dsv.TotalResults, f => f.Random.Long(min: 10000, max: 10_000_000))
            .Set(dsv => dsv.Status, DataSetVersionStatus.Draft);

    public static InstanceSetters<DataSetVersion> SetDataSet(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DataSet dataSet)
        => instanceSetter
            .Set(dsv => dsv.DataSet, dataSet)
            .Set(dsv => dsv.DataSetId, dataSet.Id);

    public static InstanceSetters<DataSetVersion> SetDataSetId(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Guid dataSetId)
        => instanceSetter.Set(dsv => dsv.DataSetId, dataSetId);

    public static InstanceSetters<DataSetVersion> SetCsvFileId(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Guid csvFileId)
        => instanceSetter.Set(dsv => dsv.CsvFileId, csvFileId);

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
            .SetWithdrawn(null);


    public static InstanceSetters<DataSetVersion> SetStatusProcessing(
        this InstanceSetters<DataSetVersion> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetVersionStatus.Processing)
            .SetWithdrawn(null)
            .SetPublished(null);

    public static InstanceSetters<DataSetVersion> SetStatusFailed(
        this InstanceSetters<DataSetVersion> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetVersionStatus.Failed)
            .SetWithdrawn(null)
            .SetPublished(null);

    public static InstanceSetters<DataSetVersion> SetStatusMapping(
        this InstanceSetters<DataSetVersion> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetVersionStatus.Mapping)
            .SetWithdrawn(null)
            .SetPublished(null);

    public static InstanceSetters<DataSetVersion> SetStatusDraft(this InstanceSetters<DataSetVersion> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetVersionStatus.Draft)
            .SetWithdrawn(null)
            .SetPublished(null);

    public static InstanceSetters<DataSetVersion> SetStatusWithdrawn(
        this InstanceSetters<DataSetVersion> instanceSetter)
        => instanceSetter
            .SetStatus(DataSetVersionStatus.Withdrawn)
            .SetWithdrawn(DateTimeOffset.UtcNow)
            .Set((_, dsv) => dsv.Published ??= DateTimeOffset.UtcNow.AddDays(-1));

    public static InstanceSetters<DataSetVersion> SetPublished(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DateTimeOffset? published)
        => instanceSetter.Set(dsv => dsv.Published, published);

    public static InstanceSetters<DataSetVersion> SetWithdrawn(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DateTimeOffset? withdrawn)
        => instanceSetter.Set(dsv => dsv.Withdrawn, withdrawn);

    public static InstanceSetters<DataSetVersion> SetTotalResults(
        this InstanceSetters<DataSetVersion> instanceSetter,
        long totalResults)
        => instanceSetter.Set(dsv => dsv.TotalResults, totalResults);

    public static InstanceSetters<DataSetVersion> SetMetaSummary(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DataSetVersionMetaSummary metaSummary)
        => instanceSetter.Set(dsv => dsv.MetaSummary, metaSummary);

    public static InstanceSetters<DataSetVersion> SetMetaSummary(
        this InstanceSetters<DataSetVersion> instanceSetter)
        => instanceSetter.Set(
            dsv => dsv.MetaSummary,
            (_, dsv) => DataSetVersionMetaSummary.Create(dsv));

    public static InstanceSetters<DataSetVersion> SetGeographicLevelMeta(
        this InstanceSetters<DataSetVersion> instanceSetter,
        GeographicLevelMeta meta)
        => instanceSetter.SetGeographicLevelMeta(() => meta);

    public static InstanceSetters<DataSetVersion> SetGeographicLevelMeta(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<GeographicLevelMeta> meta)
        => instanceSetter.Set(dsv => dsv.GeographicLevelMeta, meta);

    public static InstanceSetters<DataSetVersion> SetGeographicLevelMeta(
        this InstanceSetters<DataSetVersion> instanceSetter)
        => instanceSetter.Set(
            dsv => dsv.GeographicLevelMeta,
            (_, dsv) => new GeographicLevelMeta
            {
                DataSetVersionId = dsv.Id,
                Levels = dsv.LocationMetas.Select(m => m.Level).ToList()
            }
        );

    public static InstanceSetters<DataSetVersion> SetImports(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<IEnumerable<DataSetVersionImport>> imports)
        => instanceSetter.SetImports(_ => imports());

    public static InstanceSetters<DataSetVersion> SetImports(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<Faker, IEnumerable<DataSetVersionImport>> imports)
        => instanceSetter.Set(
            (faker, dsv) =>
            {
                dsv.Imports = imports(faker).ToList();

                foreach (var import in dsv.Imports)
                {
                    import.DataSetVersion = dsv;
                    import.DataSetVersionId = dsv.Id;
                }
            }
        );

    public static InstanceSetters<DataSetVersion> SetLocationMetas(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<IEnumerable<LocationMeta>> metas)
        => instanceSetter.SetLocationMetas(_ => metas());

    public static InstanceSetters<DataSetVersion> SetLocationMetas(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<Faker, IEnumerable<LocationMeta>> metas)
        => instanceSetter.Set(
            (faker, dsv) =>
            {
                dsv.LocationMetas = metas(faker).ToList();

                foreach (var meta in dsv.LocationMetas)
                {
                    meta.DataSetVersion = dsv;
                    meta.DataSetVersionId = dsv.Id;
                }
            }
        );

    public static InstanceSetters<DataSetVersion> SetFilterMetas(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<IEnumerable<FilterMeta>> metas)
        => instanceSetter.SetFilterMetas(_ => metas());

    public static InstanceSetters<DataSetVersion> SetFilterMetas(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<Faker, IEnumerable<FilterMeta>> metas)
        => instanceSetter.Set(
            (faker, dsv) =>
            {
                dsv.FilterMetas = metas(faker).ToList();

                foreach (var meta in dsv.FilterMetas)
                {
                    meta.DataSetVersion = dsv;
                    meta.DataSetVersionId = dsv.Id;
                }
            }
        );

    public static InstanceSetters<DataSetVersion> SetIndicatorMetas(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<IEnumerable<IndicatorMeta>> metas)
        => instanceSetter.Set(
            (_, dsv) =>
            {
                dsv.IndicatorMetas = metas().ToList();

                foreach (var meta in dsv.IndicatorMetas)
                {
                    meta.DataSetVersion = dsv;
                    meta.DataSetVersionId = dsv.Id;
                }
            }
        );

    public static InstanceSetters<DataSetVersion> SetTimePeriodMetas(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<IEnumerable<TimePeriodMeta>> metas)
        => instanceSetter.Set(
            (_, dsv) =>
            {
                dsv.TimePeriodMetas = metas().ToList();

                foreach (var meta in dsv.TimePeriodMetas)
                {
                    meta.DataSetVersion = dsv;
                    meta.DataSetVersionId = dsv.Id;
                }
            }
        );

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
