using Bogus;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class DataSetVersionGeneratorExtensions
{
    public static Generator<DataSetVersion> DefaultDataSetVersion(this DataFixture fixture) =>
        fixture.Generator<DataSetVersion>().WithDefaults();

    public static Generator<DataSetVersion> DefaultDataSetVersion(
        this DataFixture fixture,
        int filters,
        int indicators,
        int locations,
        int timePeriods,
        int maxFilterOptions = 10,
        int maxLocationOptions = 10
    ) =>
        fixture
            .DefaultDataSetVersion()
            .WithFilterMetas(faker =>
                fixture
                    .DefaultFilterMeta(options: faker.Random.Int(1, maxFilterOptions))
                    .Generate(filters)
            )
            .WithIndicatorMetas(() => fixture.DefaultIndicatorMeta().Generate(indicators))
            .WithLocationMetas(faker =>
                fixture
                    .DefaultLocationMeta(options: faker.Random.Int(1, maxLocationOptions))
                    .Generate(locations)
            )
            .WithGeographicLevelMeta()
            .WithTimePeriodMetas(() => fixture.DefaultTimePeriodMeta().Generate(timePeriods))
            .WithMetaSummary();

    public static Generator<DataSetVersion> WithDefaults(
        this Generator<DataSetVersion> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<DataSetVersion> WithId(
        this Generator<DataSetVersion> generator,
        Guid id
    ) => generator.ForInstance(s => s.SetId(id));

    public static Generator<DataSetVersion> WithDataSet(
        this Generator<DataSetVersion> generator,
        DataSet dataSet
    ) => generator.ForInstance(s => s.SetDataSet(dataSet));

    public static Generator<DataSetVersion> WithDataSetId(
        this Generator<DataSetVersion> generator,
        Guid dataSetId
    ) => generator.ForInstance(s => s.SetDataSetId(dataSetId));

    public static Generator<DataSetVersion> WithRelease(
        this Generator<DataSetVersion> generator,
        Release release
    ) => generator.ForInstance(s => s.SetRelease(release));

    public static Generator<DataSetVersion> WithRelease(
        this Generator<DataSetVersion> generator,
        Func<Release> release
    ) => generator.ForInstance(s => s.SetRelease(release));

    public static Generator<DataSetVersion> WithVersionNumber(
        this Generator<DataSetVersion> generator,
        int major,
        int minor,
        int patch = 0
    ) => generator.ForInstance(s => s.SetVersionNumber(major, minor, patch));

    public static Generator<DataSetVersion> WithPublished(
        this Generator<DataSetVersion> generator,
        DateTimeOffset published
    ) => generator.ForInstance(s => s.SetPublished(published));

    public static Generator<DataSetVersion> WithWithdrawn(
        this Generator<DataSetVersion> generator,
        DateTimeOffset withdrawn
    ) => generator.ForInstance(s => s.SetWithdrawn(withdrawn));

    public static Generator<DataSetVersion> WithStatus(
        this Generator<DataSetVersion> generator,
        DataSetVersionStatus status
    ) => generator.ForInstance(s => s.SetStatus(status));

    public static Generator<DataSetVersion> WithStatusPublished(
        this Generator<DataSetVersion> generator
    ) => generator.ForInstance(s => s.SetStatusPublished());

    public static Generator<DataSetVersion> WithStatusDraft(
        this Generator<DataSetVersion> generator
    ) => generator.ForInstance(s => s.SetStatusDraft());

    public static Generator<DataSetVersion> WithStatusProcessing(
        this Generator<DataSetVersion> generator
    ) => generator.ForInstance(s => s.SetStatusProcessing());

    public static Generator<DataSetVersion> WithStatusFailed(
        this Generator<DataSetVersion> generator
    ) => generator.ForInstance(s => s.SetStatusFailed());

    public static Generator<DataSetVersion> WithStatusMapping(
        this Generator<DataSetVersion> generator
    ) => generator.ForInstance(s => s.SetStatusMapping());

    public static Generator<DataSetVersion> WithStatusWithdrawn(
        this Generator<DataSetVersion> generator
    ) => generator.ForInstance(s => s.SetStatusWithdrawn());

    public static Generator<DataSetVersion> WithTotalResults(
        this Generator<DataSetVersion> generator,
        long totalResults
    ) => generator.ForInstance(s => s.SetTotalResults(totalResults));

    public static Generator<DataSetVersion> WithMetaSummary(
        this Generator<DataSetVersion> generator,
        DataSetVersionMetaSummary? metaSummary
    ) => generator.ForInstance(s => s.SetMetaSummary(metaSummary));

    public static Generator<DataSetVersion> WithMetaSummary(
        this Generator<DataSetVersion> generator
    ) => generator.ForInstance(s => s.SetMetaSummary());

    public static Generator<DataSetVersion> WithGeographicLevelMeta(
        this Generator<DataSetVersion> generator,
        GeographicLevelMeta? meta
    ) => generator.ForInstance(s => s.SetGeographicLevelMeta(meta));

    public static Generator<DataSetVersion> WithGeographicLevelMeta(
        this Generator<DataSetVersion> generator,
        Func<GeographicLevelMeta> meta
    ) => generator.ForInstance(s => s.SetGeographicLevelMeta(meta));

    public static Generator<DataSetVersion> WithGeographicLevelMeta(
        this Generator<DataSetVersion> generator
    ) => generator.ForInstance(s => s.SetGeographicLevelMeta());

    public static Generator<DataSetVersion> WithImports(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<DataSetVersionImport>> imports
    ) => generator.ForInstance(s => s.SetImports(imports));

    public static Generator<DataSetVersion> WithLocationMetas(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<LocationMeta>> metas
    ) => generator.ForInstance(s => s.SetLocationMetas(metas));

    public static Generator<DataSetVersion> WithLocationMetas(
        this Generator<DataSetVersion> generator,
        Func<Faker, IEnumerable<LocationMeta>> metas
    ) => generator.ForInstance(s => s.SetLocationMetas(metas));

    public static Generator<DataSetVersion> WithFilterMetas(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<FilterMeta>> metas
    ) => generator.ForInstance(s => s.SetFilterMetas(metas));

    public static Generator<DataSetVersion> WithFilterMetas(
        this Generator<DataSetVersion> generator,
        Func<Faker, IEnumerable<FilterMeta>> metas
    ) => generator.ForInstance(s => s.SetFilterMetas(metas));

    public static Generator<DataSetVersion> WithIndicatorMetas(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<IndicatorMeta>> metas
    ) => generator.ForInstance(s => s.SetIndicatorMetas(metas));

    public static Generator<DataSetVersion> WithTimePeriodMetas(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<TimePeriodMeta>> metas
    ) => generator.ForInstance(s => s.SetTimePeriodMetas(metas));

    public static Generator<DataSetVersion> WithFilterMetaChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<FilterMetaChange> metaChanges
    ) => generator.ForInstance(s => s.SetFilterMetaChanges(metaChanges));

    public static Generator<DataSetVersion> WithFilterOptionMetaChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<FilterOptionMetaChange> metaChanges
    ) => generator.ForInstance(s => s.SetFilterOptionMetaChanges(metaChanges));

    public static Generator<DataSetVersion> WithGeographicLevelMetaChanges(
        this Generator<DataSetVersion> generator,
        GeographicLevelMetaChange metaChange
    ) => generator.ForInstance(s => s.SetGeographicLevelMetaChange(metaChange));

    public static Generator<DataSetVersion> WithIndicatorMetaChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<IndicatorMetaChange> metaChanges
    ) => generator.ForInstance(s => s.SetIndicatorMetaChanges(metaChanges));

    public static Generator<DataSetVersion> WithLocationMetaChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<LocationMetaChange> metaChanges
    ) => generator.ForInstance(s => s.SetLocationMetaChanges(metaChanges));

    public static Generator<DataSetVersion> WithLocationOptionMetaChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<LocationOptionMetaChange> metaChanges
    ) => generator.ForInstance(s => s.SetLocationOptionMetaChanges(metaChanges));

    public static Generator<DataSetVersion> WithTimePeriodMetaChanges(
        this Generator<DataSetVersion> generator,
        IEnumerable<TimePeriodMetaChange> metaChanges
    ) => generator.ForInstance(s => s.SetTimePeriodMetaChanges(metaChanges));

    public static Generator<DataSetVersion> WithPreviewTokens(
        this Generator<DataSetVersion> generator,
        IEnumerable<PreviewToken> previewTokens
    ) => generator.WithPreviewTokens(() => previewTokens);

    public static Generator<DataSetVersion> WithPreviewTokens(
        this Generator<DataSetVersion> generator,
        Func<IEnumerable<PreviewToken>> previewTokens
    ) => generator.ForInstance(s => s.SetPreviewTokens(previewTokens));

    public static Generator<DataSetVersion> WithNotes(
        this Generator<DataSetVersion> generator,
        string notes
    ) => generator.ForInstance(s => s.SetNotes(notes));

    public static InstanceSetters<DataSetVersion> SetDefaults(
        this InstanceSetters<DataSetVersion> setters
    ) =>
        setters
            .SetDefault(dsv => dsv.Id)
            .SetDefault(dsv => dsv.DataSetId)
            .Set(
                dsv => dsv.Release,
                (_, _, context) => context.Fixture.Generator<Release>().WithDefaults().Generate()
            )
            .SetDefault(dsv => dsv.Notes)
            .Set(dsv => dsv.VersionMajor, 1)
            .Set(dsv => dsv.VersionMinor, (_, _, context) => context.Index)
            .Set(dsv => dsv.VersionPatch, 0)
            .Set(dsv => dsv.TotalResults, f => f.Random.Long(min: 10000, max: 10_000_000))
            .Set(dsv => dsv.Status, DataSetVersionStatus.Draft);

    public static InstanceSetters<DataSetVersion> SetId(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Guid id
    ) => instanceSetter.Set(dsv => dsv.Id, id);

    public static InstanceSetters<DataSetVersion> SetDataSet(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DataSet dataSet
    ) => instanceSetter.Set(dsv => dsv.DataSet, dataSet).Set(dsv => dsv.DataSetId, dataSet.Id);

    public static InstanceSetters<DataSetVersion> SetDataSetId(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Guid dataSetId
    ) => instanceSetter.Set(dsv => dsv.DataSetId, dataSetId);

    public static InstanceSetters<DataSetVersion> SetRelease(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Release release
    ) => instanceSetter.Set(dsv => dsv.Release, release);

    public static InstanceSetters<DataSetVersion> SetRelease(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<Release> release
    ) => instanceSetter.Set(dsv => dsv.Release, release);

    public static InstanceSetters<DataSetVersion> SetVersionNumber(
        this InstanceSetters<DataSetVersion> instanceSetter,
        int major,
        int minor,
        int patch = 0
    ) =>
        instanceSetter
            .Set(dsv => dsv.VersionMajor, major)
            .Set(dsv => dsv.VersionMinor, minor)
            .Set(dsv => dsv.VersionPatch, patch);

    public static InstanceSetters<DataSetVersion> SetStatus(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DataSetVersionStatus status
    ) => instanceSetter.Set(dsv => dsv.Status, status);

    public static InstanceSetters<DataSetVersion> SetStatusPublished(
        this InstanceSetters<DataSetVersion> instanceSetter
    ) =>
        instanceSetter
            .SetStatus(DataSetVersionStatus.Published)
            .SetPublished(DateTimeOffset.UtcNow.AddDays(-1))
            .SetWithdrawn(null);

    public static InstanceSetters<DataSetVersion> SetStatusProcessing(
        this InstanceSetters<DataSetVersion> instanceSetter
    ) =>
        instanceSetter
            .SetStatus(DataSetVersionStatus.Processing)
            .SetWithdrawn(null)
            .SetPublished(null);

    public static InstanceSetters<DataSetVersion> SetStatusFailed(
        this InstanceSetters<DataSetVersion> instanceSetter
    ) =>
        instanceSetter.SetStatus(DataSetVersionStatus.Failed).SetWithdrawn(null).SetPublished(null);

    public static InstanceSetters<DataSetVersion> SetStatusMapping(
        this InstanceSetters<DataSetVersion> instanceSetter
    ) =>
        instanceSetter
            .SetStatus(DataSetVersionStatus.Mapping)
            .SetWithdrawn(null)
            .SetPublished(null);

    public static InstanceSetters<DataSetVersion> SetStatusDraft(
        this InstanceSetters<DataSetVersion> instanceSetter
    ) => instanceSetter.SetStatus(DataSetVersionStatus.Draft).SetWithdrawn(null).SetPublished(null);

    public static InstanceSetters<DataSetVersion> SetStatusWithdrawn(
        this InstanceSetters<DataSetVersion> instanceSetter
    ) =>
        instanceSetter
            .SetStatus(DataSetVersionStatus.Withdrawn)
            .SetWithdrawn(DateTimeOffset.UtcNow.AddDays(-1))
            .Set((_, dsv) => dsv.Published ??= DateTimeOffset.UtcNow.AddDays(-2));

    public static InstanceSetters<DataSetVersion> SetPublished(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DateTimeOffset? published
    ) => instanceSetter.Set(dsv => dsv.Published, published);

    public static InstanceSetters<DataSetVersion> SetWithdrawn(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DateTimeOffset? withdrawn
    ) => instanceSetter.Set(dsv => dsv.Withdrawn, withdrawn);

    public static InstanceSetters<DataSetVersion> SetTotalResults(
        this InstanceSetters<DataSetVersion> instanceSetter,
        long totalResults
    ) => instanceSetter.Set(dsv => dsv.TotalResults, totalResults);

    public static InstanceSetters<DataSetVersion> SetMetaSummary(
        this InstanceSetters<DataSetVersion> instanceSetter,
        DataSetVersionMetaSummary? metaSummary
    ) => instanceSetter.Set(dsv => dsv.MetaSummary, metaSummary);

    public static InstanceSetters<DataSetVersion> SetMetaSummary(
        this InstanceSetters<DataSetVersion> instanceSetter
    ) =>
        instanceSetter.Set(
            dsv => dsv.MetaSummary,
            (_, dsv) => DataSetVersionMetaSummary.Create(dsv)
        );

    public static InstanceSetters<DataSetVersion> SetGeographicLevelMeta(
        this InstanceSetters<DataSetVersion> instanceSetter,
        GeographicLevelMeta? meta
    ) => instanceSetter.SetGeographicLevelMeta(() => meta);

    public static InstanceSetters<DataSetVersion> SetGeographicLevelMeta(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<GeographicLevelMeta?> meta
    ) => instanceSetter.Set(dsv => dsv.GeographicLevelMeta, meta);

    public static InstanceSetters<DataSetVersion> SetGeographicLevelMeta(
        this InstanceSetters<DataSetVersion> instanceSetter
    ) =>
        instanceSetter.Set(
            dsv => dsv.GeographicLevelMeta,
            (_, dsv) =>
                new GeographicLevelMeta
                {
                    DataSetVersionId = dsv.Id,
                    Levels = dsv.LocationMetas.Select(m => m.Level).ToList(),
                }
        );

    public static InstanceSetters<DataSetVersion> SetImports(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<IEnumerable<DataSetVersionImport>> imports
    ) => instanceSetter.SetImports(_ => imports());

    public static InstanceSetters<DataSetVersion> SetImports(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<Faker, IEnumerable<DataSetVersionImport>> imports
    ) =>
        instanceSetter.Set(
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
        Func<IEnumerable<LocationMeta>> metas
    ) => instanceSetter.SetLocationMetas(_ => metas());

    public static InstanceSetters<DataSetVersion> SetLocationMetas(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<Faker, IEnumerable<LocationMeta>> metas
    ) =>
        instanceSetter.Set(
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
        Func<IEnumerable<FilterMeta>> metas
    ) => instanceSetter.SetFilterMetas(_ => metas());

    public static InstanceSetters<DataSetVersion> SetFilterMetas(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<Faker, IEnumerable<FilterMeta>> metas
    ) =>
        instanceSetter.Set(
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
        Func<IEnumerable<IndicatorMeta>> metas
    ) =>
        instanceSetter.Set(
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
        Func<IEnumerable<TimePeriodMeta>> metas
    ) =>
        instanceSetter.Set(
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

    public static InstanceSetters<DataSetVersion> SetFilterMetaChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<FilterMetaChange> filterChanges
    ) =>
        instanceSetter.Set(
            dsv => dsv.FilterMetaChanges,
            (_, dsv) =>
                filterChanges
                    .Select(change =>
                    {
                        change.DataSetVersion = dsv;
                        change.DataSetVersionId = dsv.Id;
                        return change;
                    })
                    .ToList()
        );

    public static InstanceSetters<DataSetVersion> SetFilterOptionMetaChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<FilterOptionMetaChange> filterOptionChanges
    ) =>
        instanceSetter.Set(
            dsv => dsv.FilterOptionMetaChanges,
            (_, dsv) =>
                filterOptionChanges
                    .Select(change =>
                    {
                        change.DataSetVersion = dsv;
                        change.DataSetVersionId = dsv.Id;
                        return change;
                    })
                    .ToList()
        );

    public static InstanceSetters<DataSetVersion> SetGeographicLevelMetaChange(
        this InstanceSetters<DataSetVersion> instanceSetter,
        GeographicLevelMetaChange change
    ) =>
        instanceSetter.Set(
            dsv => dsv.GeographicLevelMetaChange,
            (_, dsv) =>
            {
                change.DataSetVersion = dsv;
                change.DataSetVersionId = dsv.Id;
                return change;
            }
        );

    public static InstanceSetters<DataSetVersion> SetIndicatorMetaChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<IndicatorMetaChange> indicatorChanges
    ) =>
        instanceSetter.Set(
            dsv => dsv.IndicatorMetaChanges,
            (_, dsv) =>
                indicatorChanges
                    .Select(change =>
                    {
                        change.DataSetVersion = dsv;
                        change.DataSetVersionId = dsv.Id;
                        return change;
                    })
                    .ToList()
        );

    public static InstanceSetters<DataSetVersion> SetLocationMetaChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<LocationMetaChange> locationChanges
    ) =>
        instanceSetter.Set(
            dsv => dsv.LocationMetaChanges,
            (_, dsv) =>
                locationChanges
                    .Select(change =>
                    {
                        change.DataSetVersion = dsv;
                        change.DataSetVersionId = dsv.Id;
                        return change;
                    })
                    .ToList()
        );

    public static InstanceSetters<DataSetVersion> SetLocationOptionMetaChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<LocationOptionMetaChange> locationOptionChanges
    ) =>
        instanceSetter.Set(
            dsv => dsv.LocationOptionMetaChanges,
            (_, dsv) =>
                locationOptionChanges
                    .Select(change =>
                    {
                        change.DataSetVersion = dsv;
                        change.DataSetVersionId = dsv.Id;
                        return change;
                    })
                    .ToList()
        );

    public static InstanceSetters<DataSetVersion> SetTimePeriodMetaChanges(
        this InstanceSetters<DataSetVersion> instanceSetter,
        IEnumerable<TimePeriodMetaChange> timePeriodChanges
    ) =>
        instanceSetter.Set(
            dsv => dsv.TimePeriodMetaChanges,
            (_, dsv) =>
                timePeriodChanges
                    .Select(change =>
                    {
                        change.DataSetVersion = dsv;
                        change.DataSetVersionId = dsv.Id;
                        return change;
                    })
                    .ToList()
        );

    public static InstanceSetters<DataSetVersion> SetPreviewTokens(
        this InstanceSetters<DataSetVersion> instanceSetter,
        Func<IEnumerable<PreviewToken>> previewTokens
    ) =>
        instanceSetter.Set(
            (_, dsv) =>
            {
                dsv.PreviewTokens = previewTokens().ToList();

                foreach (var previewToken in dsv.PreviewTokens)
                {
                    previewToken.DataSetVersion = dsv;
                    previewToken.DataSetVersionId = dsv.Id;
                }
            }
        );

    public static InstanceSetters<DataSetVersion> SetNotes(
        this InstanceSetters<DataSetVersion> instanceSetter,
        string notes
    ) => instanceSetter.Set(dsv => dsv.Notes, notes);
}
