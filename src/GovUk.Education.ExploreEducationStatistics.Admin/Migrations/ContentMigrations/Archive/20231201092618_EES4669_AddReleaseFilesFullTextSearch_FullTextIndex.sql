CREATE FULLTEXT CATALOG ReleaseFilesFullTextCatalog;

CREATE FULLTEXT INDEX ON dbo.ReleaseFiles
    (
        Name           -- Column 1
        Language 2057, -- UK English LCID
        Summary        -- Column 2
        Language 2057  -- UK English LCID
    )
KEY INDEX PK_ReleaseFiles ON ReleaseFilesFullTextCatalog -- Unique index
WITH CHANGE_TRACKING AUTO; -- Population type
