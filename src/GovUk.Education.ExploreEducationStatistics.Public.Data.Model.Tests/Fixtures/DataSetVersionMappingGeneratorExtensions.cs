using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class DataSetVersionMappingGeneratorExtensions
{
    public static Generator<DataSetVersionMapping> DefaultDataSetVersionMapping(this DataFixture fixture)
        => fixture.Generator<DataSetVersionMapping>().WithDefaults();

    public static Generator<DataSetVersionMapping> WithDefaults(this Generator<DataSetVersionMapping> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<DataSetVersionMapping> WithSourceDataSetVersion(
        this Generator<DataSetVersionMapping> generator,
        DataSetVersion dataSetVersion)
        => generator.ForInstance(s => s.SetSourceDataSetVersion(dataSetVersion));

    public static Generator<DataSetVersionMapping> WithSourceDataSetVersionId(
        this Generator<DataSetVersionMapping> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetSourceDataSetVersionId(dataSetVersionId));

    public static Generator<DataSetVersionMapping> WithTargetDataSetVersion(
        this Generator<DataSetVersionMapping> generator,
        DataSetVersion dataSetVersion)
        => generator.ForInstance(s => s.SetTargetDataSetVersion(dataSetVersion));

    public static Generator<DataSetVersionMapping> WithTargetDataSetVersionId(
        this Generator<DataSetVersionMapping> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetTargetDataSetVersionId(dataSetVersionId));

    public static Generator<DataSetVersionMapping> WithLocationMappingPlan(
        this Generator<DataSetVersionMapping> generator,
        LocationMappingPlan locationMappingPlan)
        => generator.ForInstance(s => s.SetLocationMappingPlan(locationMappingPlan));

    public static Generator<DataSetVersionMapping> WithLocationMappingsComplete(
        this Generator<DataSetVersionMapping> generator,
        bool locationMappingsComplete)
        => generator.ForInstance(s => s.SetLocationMappingsComplete(locationMappingsComplete));

    public static Generator<DataSetVersionMapping> WithFilterMappingPlan(
        this Generator<DataSetVersionMapping> generator,
        FilterMappingPlan filterMappingPlan)
        => generator.ForInstance(s => s.SetFilterMappingPlan(filterMappingPlan));

    public static Generator<DataSetVersionMapping> WithFilterMappingsComplete(
        this Generator<DataSetVersionMapping> generator,
        bool filterMappingsComplete)
        => generator.ForInstance(s => s.SetFilterMappingsComplete(filterMappingsComplete));

    public static InstanceSetters<DataSetVersionMapping> SetDefaults(
        this InstanceSetters<DataSetVersionMapping> setters)
        => setters
            .SetDefault(mapping => mapping.Id)
            .SetDefault(mapping => mapping.SourceDataSetVersionId)
            .SetDefault(mapping => mapping.TargetDataSetVersionId)
            .SetLocationMappingPlan(new LocationMappingPlan())
            .SetFilterMappingPlan(new FilterMappingPlan());

    public static InstanceSetters<DataSetVersionMapping> SetSourceDataSetVersion(
        this InstanceSetters<DataSetVersionMapping> instanceSetter,
        DataSetVersion dataSetVersion)
        => instanceSetter
            .Set(mapping => mapping.SourceDataSetVersion, dataSetVersion)
            .SetSourceDataSetVersionId(dataSetVersion.Id);

    public static InstanceSetters<DataSetVersionMapping> SetSourceDataSetVersionId(
        this InstanceSetters<DataSetVersionMapping> instanceSetter,
        Guid dataSetVersionId)
        => instanceSetter.Set(mapping => mapping.SourceDataSetVersionId, dataSetVersionId);

    public static InstanceSetters<DataSetVersionMapping> SetTargetDataSetVersion(
        this InstanceSetters<DataSetVersionMapping> instanceSetter,
        DataSetVersion dataSetVersion)
        => instanceSetter
            .Set(mapping => mapping.TargetDataSetVersion, dataSetVersion)
            .SetTargetDataSetVersionId(dataSetVersion.Id);

    public static InstanceSetters<DataSetVersionMapping> SetTargetDataSetVersionId(
        this InstanceSetters<DataSetVersionMapping> instanceSetter,
        Guid dataSetVersionId)
        => instanceSetter.Set(mapping => mapping.TargetDataSetVersionId, dataSetVersionId);

    public static InstanceSetters<DataSetVersionMapping> SetLocationMappingPlan(
        this InstanceSetters<DataSetVersionMapping> instanceSetter,
        LocationMappingPlan locationMappingPlan)
        => instanceSetter
            .Set(mapping => mapping.LocationMappingPlan, locationMappingPlan);

    public static InstanceSetters<DataSetVersionMapping> SetLocationMappingsComplete(
        this InstanceSetters<DataSetVersionMapping> instanceSetter,
        bool locationMappingsComplete)
        => instanceSetter.Set(mapping => mapping.LocationMappingsComplete, locationMappingsComplete);

    public static InstanceSetters<DataSetVersionMapping> SetFilterMappingPlan(
        this InstanceSetters<DataSetVersionMapping> instanceSetter,
        FilterMappingPlan filterMappingPlan)
        => instanceSetter
            .Set(mapping => mapping.FilterMappingPlan, filterMappingPlan);

    public static InstanceSetters<DataSetVersionMapping> SetFilterMappingsComplete(
        this InstanceSetters<DataSetVersionMapping> instanceSetter,
        bool filterMappingsComplete)
        => instanceSetter.Set(mapping => mapping.FilterMappingsComplete, filterMappingsComplete);
}
