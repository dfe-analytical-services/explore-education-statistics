-- Update any public pre-release access list descriptions in the database that contain the original default text to
-- include new default text instead.
--
-- Because in Production content there are instances where authors have added additional text in the same paragraph
-- as the default text which potentially changes the context of the sentence and especially changes the case of the
-- "Beside", we will target only those instance whereby the default text is found at the very start of the
-- PreReleaseAccessList column value.
UPDATE Releases SET PreReleaseAccessList =
  REPLACE(PreReleaseAccessList,
    '<p>Beside Department for Education (DfE) professional and production staff the following post holders are given pre-release access up to 24 hours before release.</p>',
    '<p>Besides Department for Education (DfE) professional and production staff, the following post holders were given pre-release access up to 24 hours before release.</p>'
  )
WHERE PreReleaseAccessList LIKE '<p>Beside Department for Education (DfE) professional and production staff the following post holders are given pre-release access up to 24 hours before release.</p>%';
