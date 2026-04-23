CREATE FULLTEXT CATALOG PublicationsFullTextCatalog;
CREATE FULLTEXT INDEX ON dbo.Publications
(
  Summary            -- Column 1
      Language 2057, -- UK English LCID
  Title              -- Column 2
      Language 2057  -- UK English LCID
)
KEY INDEX PK_Publications ON PublicationsFullTextCatalog --Unique index
WITH CHANGE_TRACKING AUTO; --Population type
