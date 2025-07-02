using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public class OptimisedTestSchemaInitialiser
{
    public string SchemaName { get; private set; }

    private readonly string _connectionString;

    public OptimisedTestSchemaInitialiser(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        SchemaName = "test_" + Guid.NewGuid().ToString("N");

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA \"{SchemaName}\";";
        await command.ExecuteNonQueryAsync();

        // Optionally run migrations or EnsureCreated
        var options = new DbContextOptionsBuilder<PublicDataDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        await using var context = new PublicDataDbContext(options, new SchemaNameProvider(SchemaName), true);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP SCHEMA \"{SchemaName}\" CASCADE;";
        await command.ExecuteNonQueryAsync();
    }
}
