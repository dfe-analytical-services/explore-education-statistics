-- Update OwningPublicationSlug so it's aligned with the owning publication's Slug
UPDATE [dbo].[Methodologies]
SET OwningPublicationSlug = P.Slug
FROM [dbo].[MethodologyVersions] MV
         JOIN [dbo].[Methodologies] M ON MV.MethodologyId = M.Id
         JOIN [dbo].[PublicationMethodologies] PM ON PM.MethodologyId = M.Id
         JOIN [dbo].[Publications] P ON P.Id = PM.PublicationId
WHERE PM.Owner = 1
AND P.Slug != M.OwningPublicationSlug;

-- If Methodology.Slug is different to the owning publication's slug,
-- it is an AlternativeSlug
UPDATE [dbo].[MethodologyVersions]
SET AlternativeSlug = M.Slug
FROM [dbo].[MethodologyVersions] MV
         JOIN [dbo].[Methodologies] M ON MV.MethodologyId = M.Id
WHERE M.Slug != M.OwningPublicationSlug;
