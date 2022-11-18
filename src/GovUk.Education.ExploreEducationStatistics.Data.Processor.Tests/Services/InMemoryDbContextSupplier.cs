#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

/// <summary>
/// Component for supplying services with DbContexts when testing with InMemory DbContexts.
/// </summary>
public class InMemoryDbContextSupplier : IDbContextSupplier
{
    private readonly string? _contentDbContextId;
    private readonly string? _statisticsDbContextId;

    public InMemoryDbContextSupplier(
        string? contentDbContextId = null, 
        string? statisticsDbContextId = null)
    {
        _contentDbContextId = contentDbContextId;
        _statisticsDbContextId = statisticsDbContextId;
    }

    public TDbContext CreateDbContext<TDbContext>() where TDbContext : DbContext
    {
        return (typeof(TDbContext).Name switch
        {
            nameof(ContentDbContext) => CreateContentDbContext() as TDbContext,
            nameof(StatisticsDbContext) => CreateStatisticsDbContext() as TDbContext,
            _ => throw new ArgumentOutOfRangeException("Unable to provide DbContext of type " + 
                                                       typeof(TDbContext).Name)
        })!;
    }

    public TDbContext CreateDbContextDelegate<TDbContext>() where TDbContext : DbContext
    {
        return (typeof(TDbContext).Name switch
        {
            nameof(ContentDbContext) => CreateContentDbContext() as TDbContext,
            nameof(StatisticsDbContext) => CreateStatisticsDbContext() as TDbContext,
            _ => throw new ArgumentOutOfRangeException("Unable to provide DbContext delegate of type " +
                                                       typeof(TDbContext).Name)
        })!;
    }

    private StatisticsDbContext CreateStatisticsDbContext()
    {
        return StatisticsDbUtils.InMemoryStatisticsDbContext(_statisticsDbContextId);
    }

    private ContentDbContext CreateContentDbContext()
    {
        return ContentDbUtils.InMemoryContentDbContext(_contentDbContextId);
    }
}