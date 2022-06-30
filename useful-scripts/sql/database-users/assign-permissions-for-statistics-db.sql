--
-- User permissions for the STATISTICS database
--
CREATE USER [importer] FROM LOGIN [importer];
ALTER ROLE [db_ddladmin] ADD MEMBER [importer];
ALTER ROLE [db_datareader] ADD MEMBER [importer];
ALTER ROLE [db_datawriter] ADD MEMBER [importer];
GRANT EXECUTE ON TYPE::ObservationType TO [importer];
GRANT EXECUTE ON TYPE::ObservationFilterItemType TO [importer];
GRANT EXECUTE ON OBJECT::InsertObservationFilterItems TO [importer];

CREATE USER [publisher] FROM LOGIN [publisher];
ALTER ROLE [db_ddladmin] ADD MEMBER [publisher];
ALTER ROLE [db_datareader] ADD MEMBER [publisher];
ALTER ROLE [db_datawriter] ADD MEMBER [publisher];

CREATE USER [content] FROM LOGIN [content];
GRANT SELECT ON [dbo].[BoundaryLevel] TO content;
GRANT SELECT ON [dbo].[Filter] TO content;
GRANT SELECT ON [dbo].[FilterFootnote] TO content;
GRANT SELECT ON [dbo].[FilterGroup] TO content;
GRANT SELECT ON [dbo].[FilterGroupFootnote] TO content;
GRANT SELECT ON [dbo].[FilterItem] TO content;
GRANT SELECT ON [dbo].[FilterItemFootnote] TO content;
GRANT SELECT ON [dbo].[Footnote] TO content;
GRANT SELECT ON [dbo].[geometry] TO content;
GRANT SELECT ON [dbo].[geometry_columns] TO content;
GRANT SELECT ON [dbo].[Indicator] TO content;
GRANT SELECT ON [dbo].[IndicatorFootnote] TO content;
GRANT SELECT ON [dbo].[IndicatorGroup] TO content;
GRANT SELECT ON [dbo].[Location] TO content;
GRANT SELECT ON [dbo].[Observation] TO content;
GRANT SELECT ON [dbo].[ObservationFilterItem] TO content;
GRANT SELECT ON [dbo].[Release] TO content;
GRANT SELECT ON [dbo].[ReleaseFootnote] TO content;
GRANT SELECT ON [dbo].[ReleaseSubject] TO content;
GRANT SELECT ON [dbo].[spatial_ref_sys] TO content;
GRANT SELECT ON [dbo].[Subject] TO content;
GRANT SELECT ON [dbo].[SubjectFootnote] TO content;


CREATE USER [data] FROM LOGIN [data];
GRANT SELECT ON [dbo].[BoundaryLevel] TO data;
GRANT SELECT ON [dbo].[Filter] TO data;
GRANT SELECT ON [dbo].[FilterFootnote] TO data;
GRANT SELECT ON [dbo].[FilterGroup] TO data;
GRANT SELECT ON [dbo].[FilterGroupFootnote] TO data;
GRANT SELECT ON [dbo].[FilterItem] TO data;
GRANT SELECT ON [dbo].[FilterItemFootnote] TO data;
GRANT SELECT ON [dbo].[Footnote] TO data;
GRANT SELECT ON [dbo].[geometry] TO data;
GRANT SELECT ON [dbo].[geometry_columns] TO data;
GRANT SELECT ON [dbo].[Indicator] TO data;
GRANT SELECT ON [dbo].[IndicatorFootnote] TO data;
GRANT SELECT ON [dbo].[IndicatorGroup] TO data;
GRANT SELECT ON [dbo].[Location] TO data;
GRANT SELECT ON [dbo].[Observation] TO data;
GRANT SELECT ON [dbo].[ObservationFilterItem] TO data;
GRANT SELECT ON [dbo].[Release] TO data;
GRANT SELECT ON [dbo].[ReleaseFootnote] TO data;
GRANT SELECT ON [dbo].[ReleaseSubject] TO data;
GRANT SELECT ON [dbo].[spatial_ref_sys] TO data;
GRANT SELECT ON [dbo].[Subject] TO data;
GRANT SELECT ON [dbo].[SubjectFootnote] TO data;

GRANT EXECUTE ON TYPE::IdListGuidType TO [data];
GRANT EXECUTE ON OBJECT::FilteredFootnotes TO [data];
GRANT SELECT ON OBJECT::geojson TO [data];