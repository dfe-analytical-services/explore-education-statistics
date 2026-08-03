#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Fixtures;

/// <summary>
/// Backs <see cref="ContentDbContext"/> instances with a shared, open in-memory SQLite
/// connection.
/// <para>
/// Unlike the EF Core in-memory provider, SQLite is a real relational provider that supports
/// transactions, execution strategies and commit/rollback. This lets us exercise the genuine
/// transactional code path in services which the in-memory provider cannot run.
/// </para>
/// <para>
/// The database exists only while the connection is open, so the fixture keeps a single
/// connection open for its lifetime and must be disposed at the end of the test.
/// </para>
/// </summary>
public sealed class SqliteContentDbContextFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ContentDbContext> _options;

    public SqliteContentDbContextFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False"); // Disable foreign keys until all tests have been updated to include related entities
        _connection.Open();

        _options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    /// <summary>
    /// Creates a new <see cref="ContentDbContext"/> bound to the shared connection. Use separate
    /// contexts for the seed, act and assert phases just as the in-memory tests use separate
    /// context ids, so that assertions read persisted rather than tracked state.
    /// </summary>
    public ContentDbContext CreateContext(bool updateTimestamps = true) =>
        new SqliteContentDbContext(_options, updateTimestamps);

    public void Dispose() => _connection.Dispose();

    /// <summary>
    /// Applies the SQLite specific model tweaks that the production <see cref="ContentDbContext"/> has no reason
    /// to know about. SQL Server column types such as <c>nvarchar(max)</c> are declared in the model but cannot be
    /// parsed by SQLite, which only accepts a numeric length, so they are rewritten to <c>TEXT</c>.
    /// <para>
    /// This runs after <c>base.OnModelCreating</c>, so it also catches column types applied by the
    /// <c>IEntityTypeConfiguration</c> classes picked up via <c>ApplyConfigurationsFromAssembly</c>.
    /// </para>
    /// </summary>
    private sealed class SqliteContentDbContext(DbContextOptions<ContentDbContext> options, bool updateTimestamps)
        : ContentDbContext(options, updateTimestamps)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var properties = modelBuilder.Model.GetEntityTypes().SelectMany(entityType => entityType.GetProperties());

            foreach (var property in properties.Where(property => property.GetColumnType() == "nvarchar(max)"))
            {
                property.SetColumnType("TEXT");
            }
        }
    }
}
