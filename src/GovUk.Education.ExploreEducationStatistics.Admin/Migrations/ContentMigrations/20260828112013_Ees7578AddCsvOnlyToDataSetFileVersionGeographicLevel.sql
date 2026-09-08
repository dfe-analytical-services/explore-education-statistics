-- Set CsvOnly for the DataSetFileVersionGeographicLevels rows whose value can be proven without
-- reading the data file CSV. Rows left NULL are picked up by the BAU migration endpoint, which
-- reads the CSV to find geographic levels that were never imported.

-- Every existing row is a geographic level that was imported, so nothing here can be CSV-only.
-- The only question is whether the file also contained levels that were dropped during import.

-- A file whose import dropped no rows contained no levels beyond those that were imported.
UPDATE gl
SET gl.CsvOnly = 0
FROM dbo.DataSetFileVersionGeographicLevels gl
JOIN dbo.DataImports di ON di.FileId = gl.DataSetFileVersionId
WHERE gl.CsvOnly IS NULL
  AND di.Status = 'COMPLETE'
  AND di.TotalRows IS NOT NULL
  AND di.ExpectedImportedRows IS NOT NULL
  AND di.TotalRows = di.ExpectedImportedRows;

-- Solo-importable levels are only imported when a file consists solely of that level, so a file
-- with one of them recorded contained nothing else.
UPDATE gl
SET gl.CsvOnly = 0
FROM dbo.DataSetFileVersionGeographicLevels gl
WHERE gl.CsvOnly IS NULL
  AND EXISTS (
      SELECT 1
      FROM dbo.DataSetFileVersionGeographicLevels solo
      WHERE solo.DataSetFileVersionId = gl.DataSetFileVersionId
        AND solo.GeographicLevel IN ('SCH', 'PROV', 'INST', 'PA')
  );
