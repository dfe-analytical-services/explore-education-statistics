using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures.Parquet;

public static class ParquetFilterOptionGeneratorExtensions
{
    public static Generator<ParquetFilterOption> DefaultParquetFilterOption(this DataFixture fixture)
        => fixture.Generator<ParquetFilterOption>().WithDefaults();

    public static Generator<ParquetFilterOption> WithDefaults(
        this Generator<ParquetFilterOption> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ParquetFilterOption> WithId(
        this Generator<ParquetFilterOption> generator,
        int id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<ParquetFilterOption> WithPublicId(
        this Generator<ParquetFilterOption> generator,
        string publicId)
        => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<ParquetFilterOption> WithLabel(
        this Generator<ParquetFilterOption> generator,
        string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<ParquetFilterOption> WithColumnName(
        this Generator<ParquetFilterOption> generator,
        string columnName)
        => generator.ForInstance(s => s.SetColumnName(columnName));

    public static InstanceSetters<ParquetFilterOption> SetDefaults(
        this InstanceSetters<ParquetFilterOption> setters)
        => setters
            .SetDefault(o => o.Id)
            .SetDefault(o => o.Label)
            .SetDefault(o => o.ColumnName)
            .SetDefault(o => o.PublicId);

    public static InstanceSetters<ParquetFilterOption> SetId(
        this InstanceSetters<ParquetFilterOption> setters,
        int id)
        => setters.Set(o => o.Id, id);

    public static InstanceSetters<ParquetFilterOption> SetPublicId(
        this InstanceSetters<ParquetFilterOption> setters,
        string publicId)
        => setters.Set(o => o.PublicId, publicId);

    public static InstanceSetters<ParquetFilterOption> SetLabel(
        this InstanceSetters<ParquetFilterOption> setters,
        string label)
        => setters.Set(o => o.Label, label);

    public static InstanceSetters<ParquetFilterOption> SetColumnName(
        this InstanceSetters<ParquetFilterOption> setters,
        string columnName)
        => setters.Set(o => o.ColumnName, columnName);
}
