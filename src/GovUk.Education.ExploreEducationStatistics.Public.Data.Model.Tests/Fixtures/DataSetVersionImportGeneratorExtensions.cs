using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class DataSetVersionImportGeneratorExtensions
{
    public static Generator<DataSetVersionImport> DefaultDataSetVersionImport(this DataFixture fixture)
        => fixture.Generator<DataSetVersionImport>().WithDefaults();

    public static Generator<DataSetVersionImport> WithDefaults(this Generator<DataSetVersionImport> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<DataSetVersionImport> WithDataSetVersion(
        this Generator<DataSetVersionImport> generator,
        DataSetVersion dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<DataSetVersionImport> WithDataSetVersionId(
        this Generator<DataSetVersionImport> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<DataSetVersionImport> WithStatus(this Generator<DataSetVersionImport> generator,
        DataSetVersionImportStatus status)
        => generator.ForInstance(s => s.SetStatus(status));

    public static InstanceSetters<DataSetVersionImport> SetDefaults(this InstanceSetters<DataSetVersionImport> setters)
        => setters
            .SetDefault(i => i.Id)
            .SetDefault(i => i.DataSetVersionId)
            .SetStatus(DataSetVersionImportStatus.Queued);

    public static InstanceSetters<DataSetVersionImport> SetDataSetVersion(
        this InstanceSetters<DataSetVersionImport> instanceSetter,
        DataSetVersion dataSetVersion)
        => instanceSetter
            .Set(i => i.DataSetVersion, dataSetVersion)
            .SetDataSetVersionId(dataSetVersion.Id);

    public static InstanceSetters<DataSetVersionImport> SetDataSetVersionId(
        this InstanceSetters<DataSetVersionImport> instanceSetter,
        Guid dataSetVersionId)
        => instanceSetter.Set(i => i.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<DataSetVersionImport> SetStatus(
        this InstanceSetters<DataSetVersionImport> instanceSetter,
        DataSetVersionImportStatus status)
        => instanceSetter.Set(i => i.Status, status);
}
