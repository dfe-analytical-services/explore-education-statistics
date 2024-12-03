namespace GovUk.Education.ExploreEducationStatistics.Common.Database;

/// <summary>
/// Custom migrations that run on deployment / startup.
/// These differ in use from EF database migrations in that they are not limited
/// to running against a single database / DbContext. These can be simple
/// standalone implementations or can use dependency injection if registered
/// with a DI container.
/// </summary>
public interface ICustomMigration
{
    void Apply();
}
