using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures.Parquet;

public static class ParquetIndicatorGeneratorExtensions
{
    public static Generator<ParquetIndicator> DefaultParquetIndicator(this DataFixture fixture) =>
        fixture.Generator<ParquetIndicator>().WithDefaults();

    public static Generator<ParquetIndicator> WithDefaults(this Generator<ParquetIndicator> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<ParquetIndicator> WithId(this Generator<ParquetIndicator> generator, string id) =>
        generator.ForInstance(s => s.SetId(id));

    public static Generator<ParquetIndicator> WithColumn(this Generator<ParquetIndicator> generator, string column) =>
        generator.ForInstance(s => s.SetId(column));

    public static Generator<ParquetIndicator> WithLabel(this Generator<ParquetIndicator> generator, string label) =>
        generator.ForInstance(s => s.SetLabel(label));

    public static Generator<ParquetIndicator> WithUnit(this Generator<ParquetIndicator> generator, string unit) =>
        generator.ForInstance(s => s.SetUnit(unit));

    public static Generator<ParquetIndicator> WithDecimalPlaces(
        this Generator<ParquetIndicator> generator,
        byte decimalPlaces
    ) => generator.ForInstance(s => s.SetDecimalPlaces(decimalPlaces));

    public static InstanceSetters<ParquetIndicator> SetDefaults(this InstanceSetters<ParquetIndicator> setters) =>
        setters.SetDefault(m => m.Id).SetDefault(m => m.Column).SetDefault(m => m.Label);

    public static InstanceSetters<ParquetIndicator> SetId(this InstanceSetters<ParquetIndicator> setters, string id) =>
        setters.Set(m => m.Id, id);

    public static InstanceSetters<ParquetIndicator> SetColumn(
        this InstanceSetters<ParquetIndicator> setters,
        string column
    ) => setters.Set(m => m.Column, column);

    public static InstanceSetters<ParquetIndicator> SetLabel(
        this InstanceSetters<ParquetIndicator> setters,
        string label
    ) => setters.Set(m => m.Label, label);

    public static InstanceSetters<ParquetIndicator> SetUnit(
        this InstanceSetters<ParquetIndicator> setters,
        string unit
    ) => setters.Set(m => m.Unit, unit);

    public static InstanceSetters<ParquetIndicator> SetDecimalPlaces(
        this InstanceSetters<ParquetIndicator> setters,
        byte decimalPlaces
    ) => setters.Set(m => m.DecimalPlaces, decimalPlaces);
}
