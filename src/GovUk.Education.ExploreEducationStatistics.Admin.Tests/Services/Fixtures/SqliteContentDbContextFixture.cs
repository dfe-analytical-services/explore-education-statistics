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

    /// <param name="enforceForeignKeys">
    /// Foreign keys are enforced by default, so that tests surface the referential integrity failures that a
    /// real database would raise.
    /// <para>
    /// Only pass <c>false</c> where a test's data does not yet include the related entities that the model
    /// requires, and prefer completing the entity graph over opting out. Opting out means the test cannot
    /// catch a whole class of bug that only appears against a real database.
    /// </para>
    /// </param>
    public SqliteContentDbContextFixture(bool enforceForeignKeys = true)
    {
        _connection = new SqliteConnection($"DataSource=:memory:;Foreign Keys={enforceForeignKeys}");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
        CreateTablesExcludedFromMigrations(context);
    }

    /// <summary>
    /// <c>AspNetRoles</c> belongs to <c>UsersAndRolesDbContext</c>, and <see cref="ContentDbContext"/> maps it
    /// only so that it can reference it, deliberately excluding it from its own migrations. <c>EnsureCreated</c>
    /// honours that exclusion and does not create the table, but the <c>Users</c> table still carries a foreign
    /// key to it, which SQLite enforces - so without this, no test could insert a User.
    /// </summary>
    private static void CreateTablesExcludedFromMigrations(ContentDbContext context) =>
        context.Database.ExecuteSqlRaw(
            """
            CREATE TABLE IF NOT EXISTS "AspNetRoles" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
                "Name" TEXT NULL,
                "NormalizedName" TEXT NULL,
                "ConcurrencyStamp" TEXT NULL
            );
            """
        );

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
