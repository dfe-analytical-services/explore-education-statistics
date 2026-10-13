-- Mirrors Data and Metadata Files, and the ReleaseFiles of Data files, into the DataSetFile tables.
-- Idempotent, so it's also run by the BAU resync endpoint to repair any rows which have drifted.

IF EXISTS (SELECT 1
           FROM dbo.Files
           WHERE Type = 'Data'
             AND (DataSetFileId IS NULL OR DataSetFileVersion IS NULL OR SubjectId IS NULL))
    THROW 50000, 'Data files exist with no DataSetFileId, DataSetFileVersion or SubjectId.', 1;

IF EXISTS (SELECT 1
           FROM dbo.Files
           WHERE Type = 'Data'
           GROUP BY SubjectId
           HAVING COUNT(*) > 1)
    THROW 50000, 'Subjects exist with more than one data file.', 1;

IF EXISTS (SELECT 1
           FROM dbo.Files
           WHERE Type = 'Metadata'
             AND SubjectId IS NOT NULL
           GROUP BY SubjectId
           HAVING COUNT(*) > 1)
    THROW 50000, 'Subjects exist with more than one meta file.', 1;

IF EXISTS (SELECT 1
           FROM dbo.Files DataFile
                    JOIN dbo.Files MetaFile ON MetaFile.SubjectId = DataFile.SubjectId AND MetaFile.Type = 'Metadata'
           WHERE DataFile.Type = 'Data'
             AND MetaFile.RootPath <> DataFile.RootPath)
    THROW 50000, 'Meta files exist with a different RootPath to their data file.', 1;

DECLARE @ReleaseVersionDataSetFileVersionsDeleted INT;
DECLARE @DataSetFileVersionsDeleted INT;
DECLARE @DataSetFilesDeleted INT;
DECLARE @DataSetFilesInserted INT;
DECLARE @DataSetFileVersionsInserted INT;
DECLARE @ReleaseVersionDataSetFileVersionsInserted INT;

-- Remove rows whose Files or ReleaseFiles row no longer exists

DELETE Link
FROM dbo.ReleaseVersionDataSetFileVersions Link
WHERE NOT EXISTS (SELECT 1
                  FROM dbo.ReleaseFiles ReleaseFile
                           JOIN dbo.Files DataFile ON DataFile.Id = ReleaseFile.FileId AND DataFile.Type = 'Data'
                  WHERE ReleaseFile.Id = Link.Id);

SET @ReleaseVersionDataSetFileVersionsDeleted = @@ROWCOUNT;

UPDATE DataSetFileVersion
SET ReplacedById = NULL
FROM dbo.DataSetFileVersions DataSetFileVersion
WHERE DataSetFileVersion.ReplacedById IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.Files DataFile WHERE DataFile.Id = DataSetFileVersion.ReplacedById AND DataFile.Type = 'Data');

UPDATE DataSetFileVersion
SET ReplacingId = NULL
FROM dbo.DataSetFileVersions DataSetFileVersion
WHERE DataSetFileVersion.ReplacingId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.Files DataFile WHERE DataFile.Id = DataSetFileVersion.ReplacingId AND DataFile.Type = 'Data');

DELETE DataSetFileVersion
FROM dbo.DataSetFileVersions DataSetFileVersion
WHERE NOT EXISTS (SELECT 1 FROM dbo.Files DataFile WHERE DataFile.Id = DataSetFileVersion.Id AND DataFile.Type = 'Data');

SET @DataSetFileVersionsDeleted = @@ROWCOUNT;

DELETE DataSetFile
FROM dbo.DataSetFiles DataSetFile
WHERE NOT EXISTS (SELECT 1
                  FROM dbo.Files DataFile
                  WHERE DataFile.DataSetFileId = DataSetFile.Id
                    AND DataFile.Type = 'Data');

SET @DataSetFilesDeleted = @@ROWCOUNT;

-- Insert rows which are missing

INSERT INTO dbo.DataSetFiles (Id, Created)
SELECT DataFile.DataSetFileId, MIN(DataFile.Created)
FROM dbo.Files DataFile
WHERE DataFile.Type = 'Data'
  AND NOT EXISTS (SELECT 1 FROM dbo.DataSetFiles DataSetFile WHERE DataSetFile.Id = DataFile.DataSetFileId)
GROUP BY DataFile.DataSetFileId;

SET @DataSetFilesInserted = @@ROWCOUNT;

-- Replacement links are set afterwards so that the rows they reference exist
INSERT INTO dbo.DataSetFileVersions (Id,
                                     DataSetFileId,
                                     [Version],
                                     SubjectId,
                                     RootPath,
                                     Filename,
                                     ContentType,
                                     ContentLength,
                                     MetaFileId,
                                     MetaFilename,
                                     MetaContentType,
                                     MetaContentLength,
                                     DataSetFileMeta,
                                     FilterHierarchies,
                                     Created,
                                     CreatedById)
SELECT DataFile.Id,
       DataFile.DataSetFileId,
       DataFile.DataSetFileVersion,
       DataFile.SubjectId,
       DataFile.RootPath,
       DataFile.Filename,
       DataFile.ContentType,
       DataFile.ContentLength,
       MetaFile.Id,
       MetaFile.Filename,
       MetaFile.ContentType,
       MetaFile.ContentLength,
       DataFile.DataSetFileMeta,
       DataFile.FilterHierarchies,
       DataFile.Created,
       DataFile.CreatedById
FROM dbo.Files DataFile
         OUTER APPLY (SELECT TOP 1 Id, Filename, ContentType, ContentLength
                      FROM dbo.Files
                      WHERE Type = 'Metadata'
                        AND SubjectId = DataFile.SubjectId
                      ORDER BY Id) MetaFile
WHERE DataFile.Type = 'Data'
  AND NOT EXISTS (SELECT 1 FROM dbo.DataSetFileVersions DataSetFileVersion WHERE DataSetFileVersion.Id = DataFile.Id);

SET @DataSetFileVersionsInserted = @@ROWCOUNT;

UPDATE DataSetFileVersion
SET ReplacedById = DataFile.ReplacedById
FROM dbo.DataSetFileVersions DataSetFileVersion
         JOIN dbo.Files DataFile ON DataFile.Id = DataSetFileVersion.Id
WHERE DataFile.ReplacedById IS NOT NULL
  AND (DataSetFileVersion.ReplacedById IS NULL OR DataSetFileVersion.ReplacedById <> DataFile.ReplacedById)
  AND EXISTS (SELECT 1 FROM dbo.DataSetFileVersions Replacement WHERE Replacement.Id = DataFile.ReplacedById);

UPDATE DataSetFileVersion
SET ReplacingId = DataFile.ReplacingId
FROM dbo.DataSetFileVersions DataSetFileVersion
         JOIN dbo.Files DataFile ON DataFile.Id = DataSetFileVersion.Id
WHERE DataFile.ReplacingId IS NOT NULL
  AND (DataSetFileVersion.ReplacingId IS NULL OR DataSetFileVersion.ReplacingId <> DataFile.ReplacingId)
  AND EXISTS (SELECT 1 FROM dbo.DataSetFileVersions Original WHERE Original.Id = DataFile.ReplacingId);

-- ReleaseFiles of release versions which are soft deleted are included, as plain SQL ignores EF query filters
INSERT INTO dbo.ReleaseVersionDataSetFileVersions (Id,
                                                   ReleaseVersionId,
                                                   DataSetFileVersionId,
                                                   Name,
                                                   Summary,
                                                   [Order],
                                                   PublicApiCompatible,
                                                   PublicApiDataSetId,
                                                   PublicApiDataSetVersion,
                                                   FilterSequence,
                                                   IndicatorSequence,
                                                   Published)
SELECT ReleaseFile.Id,
       ReleaseFile.ReleaseVersionId,
       ReleaseFile.FileId,
       ReleaseFile.Name,
       ReleaseFile.Summary,
       ReleaseFile.[Order],
       ReleaseFile.PublicApiCompatible,
       ReleaseFile.PublicApiDataSetId,
       ReleaseFile.PublicApiDataSetVersion,
       ReleaseFile.FilterSequence,
       ReleaseFile.IndicatorSequence,
       ReleaseFile.Published
FROM dbo.ReleaseFiles ReleaseFile
         JOIN dbo.Files DataFile ON DataFile.Id = ReleaseFile.FileId AND DataFile.Type = 'Data'
WHERE NOT EXISTS (SELECT 1 FROM dbo.ReleaseVersionDataSetFileVersions Link WHERE Link.Id = ReleaseFile.Id);

SET @ReleaseVersionDataSetFileVersionsInserted = @@ROWCOUNT;

IF (SELECT COUNT(*) FROM dbo.DataSetFiles)
    <> (SELECT COUNT(DISTINCT DataSetFileId) FROM dbo.Files WHERE Type = 'Data')
    THROW 50000, 'DataSetFiles row count does not match the number of distinct data set files in Files.', 1;

IF (SELECT COUNT(*) FROM dbo.DataSetFileVersions)
    <> (SELECT COUNT(*) FROM dbo.Files WHERE Type = 'Data')
    THROW 50000, 'DataSetFileVersions row count does not match the number of data files in Files.', 1;

IF (SELECT COUNT(*) FROM dbo.ReleaseVersionDataSetFileVersions)
    <> (SELECT COUNT(*)
        FROM dbo.ReleaseFiles ReleaseFile
                 JOIN dbo.Files DataFile ON DataFile.Id = ReleaseFile.FileId AND DataFile.Type = 'Data')
    THROW 50000, 'ReleaseVersionDataSetFileVersions row count does not match the number of data file ReleaseFiles.', 1;

SELECT @DataSetFilesInserted AS DataSetFilesInserted,
       @DataSetFilesDeleted AS DataSetFilesDeleted,
       @DataSetFileVersionsInserted AS DataSetFileVersionsInserted,
       @DataSetFileVersionsDeleted AS DataSetFileVersionsDeleted,
       @ReleaseVersionDataSetFileVersionsInserted AS ReleaseVersionDataSetFileVersionsInserted,
       @ReleaseVersionDataSetFileVersionsDeleted AS ReleaseVersionDataSetFileVersionsDeleted;
