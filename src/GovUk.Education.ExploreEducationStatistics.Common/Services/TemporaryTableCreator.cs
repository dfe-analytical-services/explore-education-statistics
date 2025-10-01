using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Thinktecture;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class TemporaryTableCreator : ITemporaryTableCreator
{
    private readonly Regex _safeTempTableNames = new("^#[a-zA-Z0-9]+[_0-9]*$", RegexOptions.Compiled);

    public async Task<ITempTableReference> CreateTemporaryTable<TEntity, TDbContext>(TDbContext context,
        CancellationToken cancellationToken)
        where TEntity : class
        where TDbContext : DbContext
    {
        var options = new TempTableCreationOptions
        {
            TableNameProvider = new DefaultTempTableNameProvider(),
            PrimaryKeyCreation = IPrimaryKeyPropertiesProvider.None
        };
        
        var tempTable = await context.CreateTempTableAsync<TEntity>(
            options,
            cancellationToken);

        ValidateTempTableName(tempTable.Name);

        return tempTable;
    }

    public async Task<ITempTableReference> CreateAnonymousTemporaryTable<TEntity, TDbContext>(
        TDbContext context,
        CancellationToken cancellationToken)
        where TEntity : class
        where TDbContext : DbContext
    {
        var options = new TempTableCreationOptions
        {
            TableNameProvider = new DefaultTempTableNameProvider(),
            PrimaryKeyCreation = IPrimaryKeyPropertiesProvider.None
        };

        var tempTable = await context.CreateTempTableAsync<TEntity>(
            options,
            cancellationToken);

        ValidateTempTableName(tempTable.Name);

        return tempTable;
    }

    public async Task<ITempTableQuery<TEntity>> CreateAnonymousTemporaryTableAndPopulate<TEntity, TDbContext>(
        TDbContext context,
        IEnumerable<TEntity> values,
        CancellationToken cancellationToken)
        where TEntity : class
        where TDbContext : DbContext
    {
        var tempTable = await context.BulkInsertIntoTempTableAsync(values, cancellationToken: cancellationToken);

        ValidateTempTableName(tempTable.Name);

        return tempTable;
    }

    private void ValidateTempTableName(string tempTableName)
    {
        if (!_safeTempTableNames.IsMatch(tempTableName))
        {
            throw new ArgumentException($"{tempTableName} is not a valid temporary table name");
        }
    }
}
