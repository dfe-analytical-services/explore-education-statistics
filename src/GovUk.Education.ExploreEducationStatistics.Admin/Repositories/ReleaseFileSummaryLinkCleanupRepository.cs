#nullable enable
using System.Data;
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Repositories;

public interface IReleaseFileSummaryLinkCleanupRepository
{
    Task<SummaryHtmlBackup> GetOriginalSummaries(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);
    Task<bool> TryApply(Guid id, string current, string cleaned, CancellationToken cancellationToken);
}

public record SummaryHtmlBackup(bool Exists, IReadOnlyDictionary<Guid, string?> Summaries);

/// <summary>
/// Reads the immutable HTML migration backup and atomically backs up and replaces an exact current value.
/// No EF model or migration is needed for these temporary BAU backup tables.
/// </summary>
public class ReleaseFileSummaryLinkCleanupRepository(ContentDbContext context)
    : IReleaseFileSummaryLinkCleanupRepository
{
    public async Task<SummaryHtmlBackup> GetOriginalSummaries(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken
    )
    {
        var exists = await context
            .Database.SqlQueryRaw<int>(
                "SELECT CASE WHEN OBJECT_ID('dbo.ReleaseFiles_Ees7367_SummaryBackup', 'U') IS NULL THEN 0 ELSE 1 END AS Value"
            )
            .SingleAsync(cancellationToken);
        if (exists == 0 || ids.Count == 0)
        {
            return new SummaryHtmlBackup(exists == 1, new Dictionary<Guid, string?>());
        }

        var rows = await context
            .Database.SqlQueryRaw<BackupRow>(
                """
                SELECT Id, Summary FROM dbo.ReleaseFiles_Ees7367_SummaryBackup
                WHERE Id IN (SELECT CONVERT(uniqueidentifier, [value]) FROM OPENJSON(@ids))
                """,
                new SqlParameter("@ids", SqlDbType.NVarChar, -1) { Value = JsonSerializer.Serialize(ids) }
            )
            .ToListAsync(cancellationToken);
        return new SummaryHtmlBackup(true, rows.ToDictionary(row => row.Id, row => row.Summary));
    }

    public async Task<bool> TryApply(Guid id, string current, string cleaned, CancellationToken cancellationToken)
    {
        var applied = new SqlParameter("@applied", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        await context.Database.ExecuteSqlRawAsync(
            ApplySql,
            new object[]
            {
                new SqlParameter("@id", id),
                new SqlParameter("@current", SqlDbType.NVarChar, -1) { Value = current },
                new SqlParameter("@cleaned", SqlDbType.NVarChar, -1) { Value = cleaned },
                applied,
            },
            cancellationToken
        );
        return (bool)applied.Value;
    }

    // SQL Server string equality ignores trailing spaces, even with a binary collation.
    // Comparing the UTF-16 bytes checks case, accents and whitespace exactly, like StringComparison.Ordinal.
    private const string ApplySql = """
        SET XACT_ABORT ON;
        SET QUOTED_IDENTIFIER ON;
        SET @applied = 0;
        BEGIN TRY
            BEGIN TRANSACTION;
            -- Serialize table initialization and cleanup writes across endpoint requests.
            DECLARE @lockResult int;
            EXEC @lockResult = sys.sp_getapplock
                @Resource = 'Ees7367ReleaseFileSummaryLinkCleanup',
                @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
            IF @lockResult < 0 THROW 50000, 'Could not acquire summary cleanup lock.', 1;

            IF OBJECT_ID('dbo.ReleaseFiles_Ees7367_LinkCleanupBackup', 'U') IS NULL
                CREATE TABLE dbo.ReleaseFiles_Ees7367_LinkCleanupBackup (
                    Id uniqueidentifier NOT NULL PRIMARY KEY,
                    Summary nvarchar(max) NOT NULL,
                    CleanedSummary nvarchar(max) NOT NULL,
                    BackedUpAt datetime2 NOT NULL
                );

            IF EXISTS (
                SELECT 1 FROM dbo.ReleaseFiles rf WITH (UPDLOCK, HOLDLOCK)
                INNER JOIN dbo.ReleaseVersions rv WITH (HOLDLOCK) ON rv.Id = rf.ReleaseVersionId
                WHERE rf.Id = @id AND rv.SoftDeleted = 0
                    AND DATALENGTH(rf.Summary) = DATALENGTH(@current)
                    AND CONVERT(varbinary(max), rf.Summary) = CONVERT(varbinary(max), @current)
            ) AND NOT EXISTS (SELECT 1 FROM dbo.ReleaseFiles_Ees7367_LinkCleanupBackup WHERE Id = @id)
            BEGIN
                INSERT dbo.ReleaseFiles_Ees7367_LinkCleanupBackup (Id, Summary, CleanedSummary, BackedUpAt)
                VALUES (@id, @current, @cleaned, SYSUTCDATETIME());
                UPDATE dbo.ReleaseFiles SET Summary = @cleaned WHERE Id = @id;
                SET @applied = 1;
            END;
            COMMIT TRANSACTION;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            THROW;
        END CATCH;
        """;

    private class BackupRow
    {
        public Guid Id { get; set; }
        public string? Summary { get; set; }
    }
}
