-- We've renamed Methodologies.Slug to Methodologies.OwningPublicationSlug
-- This means that in some cases the OwningPublicationSlug is incorrect. In these cases,
-- we need to set AlternativeSlug to maintain the custom slug, and then correct OwningPublicationSlug.
-- That's what we're doing in this migration.
--
-- The one complication is that previously a methodology slug couldn't be changed if the methodology was published.
-- Now we have redirects, if there are cases where the slug isn't aligned with the title, we can fix this.
-- So if a methodology has a custom slug, but uses the owning publication's title, we change the slug to use the owning
-- publication's slug. We then create redirects so we don't break existing links that use the old custom slug.

-- If OwningPublicationSlug is different to the owning publication's slug,
-- it is an AlternativeSlug
UPDATE [dbo].[MethodologyVersions]
SET AlternativeSlug = M.OwningPublicationSlug
FROM [dbo].[MethodologyVersions] MV
JOIN [dbo].[Methodologies] M ON MV.MethodologyId = M.Id
JOIN [dbo].[PublicationMethodologies] PM ON PM.MethodologyId = M.Id
JOIN [dbo].[Publications] P ON P.Id = PM.PublicationId
WHERE PM.Owner = 1
AND P.Slug != M.OwningPublicationSlug
-- If there is no AlternativeTitle set, we don't want to set the
-- AlternativeSlug - we want to use the owning publication's slug.
-- But by not setting AlternativeSlug, we're changing the slug
AND MV.AlternativeTitle IS NOT NULL;

-- Since we're changing the slug if there is no AlternativeTitle set,
-- we need to create a redirect for those cases
INSERT INTO [dbo].[MethodologyRedirects] (Slug, MethodologyVersionId, Created)
SELECT M.OwningPublicationSlug AS Slug,
       MV.Id AS MethodologyVersionId,
       GETUTCDATE() AS Created
FROM [dbo].[MethodologyVersions] MV
JOIN [dbo].[Methodologies] M ON MV.MethodologyId = M.Id
JOIN [dbo].[PublicationMethodologies] PM ON PM.MethodologyId = M.Id
JOIN [dbo].[Publications] P ON P.Id = PM.PublicationId
WHERE PM.Owner = 1
AND P.Slug != M.OwningPublicationSlug
AND MV.AlternativeTitle IS NULL;

-- Update OwningPublicationSlug so it's aligned with the owning publication's Slug
UPDATE [dbo].[Methodologies]
SET OwningPublicationSlug = P.Slug
FROM [dbo].[MethodologyVersions] MV
JOIN [dbo].[Methodologies] M ON MV.MethodologyId = M.Id
JOIN [dbo].[PublicationMethodologies] PM ON PM.MethodologyId = M.Id
JOIN [dbo].[Publications] P ON P.Id = PM.PublicationId
WHERE PM.Owner = 1
AND P.Slug != M.OwningPublicationSlug

/* Queries to check migration has run correctly
  -- Run this before migration to determine what AlternativeSlugs should be set after migration
  SELECT M.Slug AS FutureAlternativeSlug
  FROM [content].[dbo].[MethodologyVersions] MV
  JOIN Methodologies M ON M.Id = MV.MethodologyId
  JOIN PublicationMethodologies PM ON PM.MethodologyId = M.Id
  JOIN Publications P ON P.Id = PM.PublicationId
  WHERE P.Slug != M.Slug
  AND PM.Owner = 1;

  -- Run after migration. Should return no results
  SELECT *
  FROM [content].[dbo].[MethodologyVersions] MV
  JOIN Methodologies M ON M.Id = MV.MethodologyId
  JOIN PublicationMethodologies PM ON PM.MethodologyId = M.Id
  JOIN Publications P ON P.Id = PM.PublicationId
  WHERE P.Slug != M.OwningPublicationSlug
  AND PM.Owner = 1;

  -- @MarkFix query to check redirects are correctly created

  -- Run after migration. Should return the same slugs as query run before migration
  SELECT AlternativeSlug
  FROM [content].[dbo].[MethodologyVersions]
  WHERE AlternativeSlug IS NOT NULL;
*/
