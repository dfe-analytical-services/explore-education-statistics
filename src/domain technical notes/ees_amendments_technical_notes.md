# Explore Education Statistics (EES) — Amendments: Developer Notes

*Last updated: 8 August 2025*

> **Audience Context:** The [Analysts’ Guide](https://dfe-analytical-services.github.io/analysts-guide/statistics-production/ees.html#making-amendments) explains *what* amendments are and *when* analysts should make them, focusing on workflow, content updates, and publication policy. This document is for **developers building and maintaining the EES system**. It focuses on *how* amendments work technically—data model, storage, queries, and backend behaviours.

---

## 1. Overview

An **amendment** is a new **release version** of an already **published** release within the same **publication**. Analysts initiate amendments to correct or update published content. This process:

- Is triggered in the Admin UI by Publication Owners.
- Creates a new `ReleaseVersion` linked to the same `Publication`.
- Can be cancelled in draft; once published, it becomes live.
- Preserves historical records of previous releases.

---

## 2. Key Concepts

- **Publication**: Container for releases.
- **Release**: Specific edition/time period.
- **ReleaseVersion**: The technical entity representing a version, holding all linked content and metadata.
- **Copy vs Reference**:
  - Content tables are duplicated for the amendment.
  - Files are reused via `ReleaseFiles` → `File` links; replacements create new `File` records.

---
## 3. Amendments Data Handling and Integrity Processes

What the system does in the background:

- **Row duplication:** All content tied to the published `ReleaseVersion` (except files) is copied into new rows linked to the amended `ReleaseVersion`. This includes narrative sections, data blocks, featured tables, and methodology versions.

- **File handling:** Files are *referenced*, not cloned. A replacement uploads a new `File` record and updates `ReleaseFiles` for the amended version only.

- **Foreign key isolation:** Every table linked via FK to `ReleaseVersions` keeps the amendment’s content isolated from other versions.

- **Transaction boundaries:** Amendment creation should be atomic — copy operations and FK updates happen in a single transaction to prevent partial state.

- **Testing strategy:** Developers should run queries to:

  - Identify all rows linked to the amended `ReleaseVersion`.
  - Compare against the previous version to confirm intended changes.
  - Verify no unintended carryover from other releases.

---

## 4. Data Model

Tables linked to `ReleaseVersions` include:

- `ContentBlocks`, `ContentSections`, `DataBlockVersions`, `FeaturedTables`, `KeyStatistics`, `MethodologyVersions`, `ReleaseFiles`, `ReleasePublishingFeedback`, `ReleaseStatus`, `ReleaseVersions` (self-ref), `Updates`, `UserReleaseInvites`, `UserReleaseRoles`, `ReleaseVersionPublishingOrganisations`, `Publications`.

Any new changes made via this amendment are stored in these tables and remain inaccessible to the public users until the amendment gets published.

---

## 5. Technical notes

- Amendments should be treated as new `ReleaseVersions` with their own FK-linked content.
- Code must run all validation checks before publishing.
- Enforce correct permissions in DB and code.
- Common pitfalls:
  - Dangling FKs from removed content.
  - Accidentally repointing old releases to new files.
  - Publishing before validation.
- Entity Sketch

```
Publication 1—* Release 1—* ReleaseVersion
ReleaseVersion 1—* {ContentSections, ContentBlocks, DataBlockVersions, FeaturedTables, KeyStatistics,
                     MethodologyVersions, ReleaseFiles, ReleasePublishingFeedback, UserReleaseRoles,
                     UserReleaseInvites, ReleaseStatus, ReleaseVersionPublishingOrganisations}
ReleaseFiles *—1 File
```
- This query below lets you know which tables are associated with new changes made in an amendment 
  - as it lists all tables which have a foreign key reference to the `ReleaseVersion` 
  - which represents a new amendment.
```
SELECT
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    col.name AS ColumnName,
    ref.name AS ReferencedTableName
FROM sys.foreign_keys fk
         JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
         JOIN sys.columns col ON fkc.parent_object_id = col.object_id AND fkc.parent_column_id = col.column_id
         JOIN sys.objects ref ON fkc.referenced_object_id = ref.object_id
WHERE LOWER(fk.name) LIKE '%releaseversion%' OR LOWER(fk.name) LIKE '%releasefile%'
ORDER BY fk.name;
```
---

