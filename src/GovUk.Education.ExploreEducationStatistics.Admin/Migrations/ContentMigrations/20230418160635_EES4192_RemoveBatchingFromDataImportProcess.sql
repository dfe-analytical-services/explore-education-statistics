UPDATE DataImports 
SET 
    ImportedRows = ExpectedImportedRows,
    LastProcessedRowIndex = TotalRows - 1
WHERE 
    Status = 'COMPLETE';