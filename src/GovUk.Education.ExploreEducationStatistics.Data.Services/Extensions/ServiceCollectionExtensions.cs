#nullable enable
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Writers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnalytics(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddAnalyticsCommon(configuration)
            .WhenEnabled.AddWriteStrategy<CaptureTableToolDownloadCallAnalyticsWriteStrategy>()
            .AddWriteStrategy<CapturePermaLinkTableDownloadCallAnalyticsWriteStrategy>()
            .Services;

    /// <summary>
    /// Registers <see cref="IStorageDataSetResolver" /> together with everything needed to read data sets from
    /// each supported storage, so that hosts need not know which collaborators a particular storage requires.
    /// </summary>
    public static IServiceCollection AddStorageDataSets(this IServiceCollection services) =>
        services
            .AddScoped<IStorageDataSetResolver, StorageDataSetResolver>()
            // StatsDB
            // @MarkFix rename these so it's clear they're for the StatisticsDB data sets?
            .AddTransient<StatisticsDbDataSetFactory>()
            .AddTransient<IObservationService, ObservationService>()
            .AddTransient<IMatchingObservationsQueryGenerator, MatchingObservationsQueryGenerator>()
            .AddTransient<IAllObservationsMatchedFilterItemsStrategy, AllObservationsMatchedFilterItemsStrategy>()
            .AddTransient<ISparseObservationsMatchedFilterItemsStrategy, SparseObservationsMatchedFilterItemsStrategy>()
            .AddTransient<IDenseObservationsMatchedFilterItemsStrategy, DenseObservationsMatchedFilterItemsStrategy>()
            .AddTransient<ISqlStatementsHelper, SqlStatementsHelper>()
            .AddTransient<IRawSqlExecutor, RawSqlExecutor>()
            .AddTransient<ITemporaryTableCreator, TemporaryTableCreator>();
}
