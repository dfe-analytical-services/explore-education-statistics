using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures.Parquet;

public static class ParquetTimePeriodGeneratorExtensions
{
    public static Generator<ParquetTimePeriod> DefaultParquetTimePeriod(this DataFixture fixture) =>
        fixture.Generator<ParquetTimePeriod>().WithDefaults();

    public static Generator<ParquetTimePeriod> WithDefaults(this Generator<ParquetTimePeriod> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<ParquetTimePeriod> WithId(this Generator<ParquetTimePeriod> generator, int id) =>
        generator.ForInstance(s => s.SetId(id));

    public static Generator<ParquetTimePeriod> WithPeriod(this Generator<ParquetTimePeriod> generator, string period) =>
        generator.ForInstance(s => s.SetPeriod(period));

    public static Generator<ParquetTimePeriod> WithIdentifier(
        this Generator<ParquetTimePeriod> generator,
        string identifier
    ) => generator.ForInstance(s => s.SetIdentifier(identifier));

    public static InstanceSetters<ParquetTimePeriod> SetDefaults(this InstanceSetters<ParquetTimePeriod> setters) =>
        setters.SetDefault(o => o.Id).SetDefault(o => o.Period).SetDefault(o => o.Identifier);

    public static InstanceSetters<ParquetTimePeriod> SetId(this InstanceSetters<ParquetTimePeriod> setters, int id) =>
        setters.Set(o => o.Id, id);

    public static InstanceSetters<ParquetTimePeriod> SetPeriod(
        this InstanceSetters<ParquetTimePeriod> setters,
        string period
    ) => setters.Set(o => o.Period, period);

    public static InstanceSetters<ParquetTimePeriod> SetIdentifier(
        this InstanceSetters<ParquetTimePeriod> setters,
        string identifier
    ) => setters.Set(o => o.Identifier, identifier);
}
