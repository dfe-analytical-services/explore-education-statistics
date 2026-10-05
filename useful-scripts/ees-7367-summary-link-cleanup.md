# EES-7367 summary link destination cleanup

This BAU-only endpoint removes destinations appended by `HtmlToTextUtils.HtmlToText` to anchor labels. It operates only on `ReleaseFiles.Summary`. Existing callers of the converter still include destinations by default. The new option removes all nonblank anchor destinations, including HTTP, HTTPS, relative, fragment and mailto targets; it never searches arbitrary parenthesized text for URLs.

- Preview: `GET /api/bau/remove-release-file-summary-link-destinations?limit=500&offset=0`
- Apply: `PUT /api/bau/remove-release-file-summary-link-destinations?limit=500&offset=0`

Run the HTML stripping migration separately before using this endpoint. Neither calling preview nor building/testing this change runs that migration.

The endpoint loads source HTML from `dbo.ReleaseFiles_Ees7367_SummaryBackup`. It converts the source with the existing behavior and requires ordinal equality with the current summary. Only then does it propose or apply the conversion with `includeLinkDestinations: false`. Missing sources, summaries still containing HTML, and edits since migration are skipped. It cannot safely infer provenance when the backup is absent or stale. Intentional URLs in prose and anchor labels remain. Nested lists and tables use the same converter; table column widths are recalculated when destinations are omitted.

## Paging and report

`limit` must be 1–500, and `offset` non-negative. Each page includes summary count, inspected count, proposed/applied changes, applied count, skipped rows with reasons, and `NextOffset`. Continue at `NextOffset` until it is null. Skipped and cleaned rows remain in the scan, so repeating offset 0 does not advance through the database. Apply rebuilds the page from current values rather than applying a previously captured report. Concurrent insertions/deletions or changes to null/soft-delete status can shift offset pages; after such changes rerun a complete scan from zero.

Skip reasons:

- `MissingSourceBackup`: original migration backup table is unavailable.
- `MissingSourceHtml`: no backup row or its source is null.
- `NoLinkDestinations`: both converter options produce identical text.
- `AlreadyCleaned`: current text already equals the destination-free conversion.
- `CurrentSummaryDoesNotMatchSourceConversion`: source cannot prove the current value came from the migration.
- `ConcurrentChangeOrExistingCleanupBackup`: the final exact-value guard rejected the write, the release was deleted, or this row already has a cleanup backup.

## Atomic writes and reversal

Each changed row is independently transactional. The SQL guard locks the release file and release version and requires an exact UTF-16 byte and byte-length match against the value read for that report. The backup insert and update commit together. A transaction application lock serializes backup-table creation and endpoint writes. A timeout or failure can leave earlier rows committed; repeating the same page safely skips them.

The endpoint creates `dbo.ReleaseFiles_Ees7367_LinkCleanupBackup` with `Id`, `Summary`, `CleanedSummary`, and `BackedUpAt`, and never overwrites an existing row. It leaves the original HTML migration backup untouched. The application database principal needs permissions to create this BAU backup table and execute `sp_getapplock`, as well as normal read/update permissions.

Review scope before executing this reversal in the intended database. The guard preserves subsequent edits. Reversal is not part of the endpoint:

```sql
UPDATE rf SET Summary = backup.Summary
FROM dbo.ReleaseFiles rf
JOIN dbo.ReleaseFiles_Ees7367_LinkCleanupBackup backup ON backup.Id = rf.Id
WHERE DATALENGTH(rf.Summary) = DATALENGTH(backup.CleanedSummary)
  AND CONVERT(varbinary(max), rf.Summary) = CONVERT(varbinary(max), backup.CleanedSummary);
```

Do not delete or overwrite backup rows to retry cleanup without investigating why a row differs. Once reversed, that row remains protected by its existing cleanup backup.

## Local seed caveat

The separate test seed table `CodexHtmlLinkTest_20261002_Backup` stores source HTML in `SeededSummary`; its `OriginalSummary` contains the values from before test seeding. The endpoint intentionally does not use this table. If the migration backup predates the seeds, those test rows are skipped because the recorded source does not match. Retain both backups and arrange a separately reviewed local test source before exercising writes; never substitute pre-seed `OriginalSummary` for migration HTML.

## Validation

Focused HTML converter tests: 84 passed, including default behavior and links in lists, descriptions and tables. Focused new controller and existing HTML stripping controller tests: 22 passed. SQL Server 2019 session-local temporary fixtures exercised the production SQL's stale-case/whitespace guard, atomic backup and update, retry, preservation of existing backup rows, soft-delete guard and failure rollback. No persistent database migration or cleanup was run.

A read-only probe also verified the actual repository's backup-table existence check, parameterized OPENJSON ID selection, EF mapping and empty-ID behavior against the local database. CSharpier checked all 8 changed C# files; git diff --check passed. Existing project dependency/deprecation warnings remain; no new cleanup-specific compiler warning was observed.
