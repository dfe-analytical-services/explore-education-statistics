#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IStorageDataSetResolver" /> together with everything needed to read data sets from
    /// each supported storage, so that hosts need not know which collaborators a particular storage requires.
    /// Parquet storage reads its base path from the <see cref="DataFilesOptions" /> configuration section.
    /// </summary>
    public static IServiceCollection AddStorageDataSets(
        this IServiceCollection services,
        IConfiguration configuration
    ) =>
        services
            .AddScoped<IStorageDataSetResolver, StorageDataSetResolver>()
            // StatsDB
            .AddTransient<StatisticsDbDataSetFactory>()
            .AddTransient<IObservationService, ObservationService>()
            .AddTransient<IMatchingObservationsQueryGenerator, MatchingObservationsQueryGenerator>()
            .AddTransient<IAllObservationsMatchedFilterItemsStrategy, AllObservationsMatchedFilterItemsStrategy>()
            .AddTransient<ISparseObservationsMatchedFilterItemsStrategy, SparseObservationsMatchedFilterItemsStrategy>()
            .AddTransient<IDenseObservationsMatchedFilterItemsStrategy, DenseObservationsMatchedFilterItemsStrategy>()
            .AddTransient<ISqlStatementsHelper, SqlStatementsHelper>()
            .AddTransient<IRawSqlExecutor, RawSqlExecutor>()
            .AddTransient<ITemporaryTableCreator, TemporaryTableCreator>()
            // Parquet
            .Configure<DataFilesOptions>(configuration.GetSection(DataFilesOptions.Section))
            .AddSingleton<IParquetDataSetPathResolver, ParquetDataSetPathResolver>()
            .AddTransient<ParquetV1DataSetFactory>();

    /// <summary>
    /// Registers <see cref="IStorageDataSetWriterResolver" /> together with everything needed to write data sets to
    /// each supported storage when importing them. Hosts must also register <see cref="AddStorageDataSets" />,
    /// <see cref="IDatabaseHelper" />, <see cref="IGuidGenerator" /> and the StatisticsDbContext.
    /// </summary>
    public static IServiceCollection AddStorageDataSetWriters(this IServiceCollection services) =>
        services
            .AddScoped<IStorageDataSetWriterResolver, StorageDataSetWriterResolver>()
            // StatsDB
            .AddTransient<StatisticsDbDataSetWriterFactory>()
            .AddTransient<ImporterLocationService>()
            .AddSingleton<IImporterLocationCache, ImporterLocationCache>()
            .AddTransient<IObservationBatchImporter, StoredProcedureObservationBatchImporter>()
            // Parquet
            .AddTransient<IParquetDataSetWriter, ParquetDataSetWriter>()
            .AddTransient<ParquetV1DataSetWriterFactory>();
}
