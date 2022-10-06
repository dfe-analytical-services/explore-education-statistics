IF NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'importer')
BEGIN
    CREATE USER [importer] FROM LOGIN [importer];

    GRANT ALL ON [dbo].[BoundaryLevel] TO [importer];
    GRANT ALL ON [dbo].[Filter] TO [importer];
    GRANT ALL ON [dbo].[FilterFootnote] TO [importer];
    GRANT ALL ON [dbo].[FilterGroup] TO [importer];
    GRANT ALL ON [dbo].[FilterGroupFootnote] TO [importer];
    GRANT ALL ON [dbo].[FilterItem] TO [importer];
    GRANT ALL ON [dbo].[FilterItemFootnote] TO [importer];
    GRANT ALL ON [dbo].[Footnote] TO [importer];
    GRANT ALL ON [dbo].[geometry] TO [importer];
    GRANT ALL ON [dbo].[geometry_columns] TO [importer];
    GRANT ALL ON [dbo].[Indicator] TO [importer];
    GRANT ALL ON [dbo].[IndicatorFootnote] TO [importer];
    GRANT ALL ON [dbo].[IndicatorGroup] TO [importer];
    GRANT ALL ON [dbo].[Location] TO [importer];
    GRANT ALL ON [dbo].[Observation] TO [importer];
    GRANT ALL ON [dbo].[ObservationFilterItem] TO [importer];
    GRANT ALL ON [dbo].[Release] TO [importer];
    GRANT ALL ON [dbo].[ReleaseFootnote] TO [importer];
    GRANT ALL ON [dbo].[ReleaseSubject] TO [importer];
    GRANT ALL ON [dbo].[spatial_ref_sys] TO [importer];
    GRANT ALL ON [dbo].[Subject] TO [importer];
    GRANT ALL ON [dbo].[SubjectFootnote] TO [importer];

    GRANT EXECUTE ON TYPE::ObservationType TO [importer];
    GRANT EXECUTE ON TYPE::ObservationFilterItemType TO [importer];
    GRANT EXECUTE ON OBJECT::InsertObservations TO [importer];
    GRANT EXECUTE ON OBJECT::InsertObservationFilterItems TO [importer];
END


IF NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'publisher')
BEGIN
    CREATE USER [publisher] FROM LOGIN [publisher];

    GRANT ALL ON [dbo].[BoundaryLevel] TO [publisher];
    GRANT ALL ON [dbo].[Filter] TO [publisher];
    GRANT ALL ON [dbo].[FilterFootnote] TO [publisher];
    GRANT ALL ON [dbo].[FilterGroup] TO [publisher];
    GRANT ALL ON [dbo].[FilterGroupFootnote] TO [publisher];
    GRANT ALL ON [dbo].[FilterItem] TO [publisher];
    GRANT ALL ON [dbo].[FilterItemFootnote] TO [publisher];
    GRANT ALL ON [dbo].[Footnote] TO [publisher];
    GRANT ALL ON [dbo].[geometry] TO [publisher];
    GRANT ALL ON [dbo].[geometry_columns] TO [publisher];
    GRANT ALL ON [dbo].[Indicator] TO [publisher];
    GRANT ALL ON [dbo].[IndicatorFootnote] TO [publisher];
    GRANT ALL ON [dbo].[IndicatorGroup] TO [publisher];
    GRANT ALL ON [dbo].[Location] TO [publisher];
    GRANT ALL ON [dbo].[Observation] TO [publisher];
    GRANT ALL ON [dbo].[ObservationFilterItem] TO [publisher];
    GRANT ALL ON [dbo].[Release] TO [publisher];
    GRANT ALL ON [dbo].[ReleaseFootnote] TO [publisher];
    GRANT ALL ON [dbo].[ReleaseSubject] TO [publisher];
    GRANT ALL ON [dbo].[spatial_ref_sys] TO [publisher];
    GRANT ALL ON [dbo].[Subject] TO [publisher];
    GRANT ALL ON [dbo].[SubjectFootnote] TO [publisher];
END


IF NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'content')
BEGIN
    CREATE USER [content] FROM LOGIN [content];
    
    GRANT SELECT ON [dbo].[BoundaryLevel] TO [content];
    GRANT SELECT ON [dbo].[Filter] TO [content];
    GRANT SELECT ON [dbo].[FilterFootnote] TO [content];
    GRANT SELECT ON [dbo].[FilterGroup] TO [content];
    GRANT SELECT ON [dbo].[FilterGroupFootnote] TO [content];
    GRANT SELECT ON [dbo].[FilterItem] TO [content];
    GRANT SELECT ON [dbo].[FilterItemFootnote] TO [content];
    GRANT SELECT ON [dbo].[Footnote] TO [content];
    GRANT SELECT ON [dbo].[geometry] TO [content];
    GRANT SELECT ON [dbo].[geometry_columns] TO [content];
    GRANT SELECT ON [dbo].[Indicator] TO [content];
    GRANT SELECT ON [dbo].[IndicatorFootnote] TO [content];
    GRANT SELECT ON [dbo].[IndicatorGroup] TO [content];
    GRANT SELECT ON [dbo].[Location] TO [content];
    GRANT SELECT ON [dbo].[Observation] TO [content];
    GRANT SELECT ON [dbo].[ObservationFilterItem] TO [content];
    GRANT SELECT ON [dbo].[Release] TO [content];
    GRANT SELECT ON [dbo].[ReleaseFootnote] TO [content];
    GRANT SELECT ON [dbo].[ReleaseSubject] TO [content];
    GRANT SELECT ON [dbo].[spatial_ref_sys] TO [content];
    GRANT SELECT ON [dbo].[Subject] TO [content];
    GRANT SELECT ON [dbo].[SubjectFootnote] TO [content];
END


IF NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'data')
BEGIN
    CREATE USER [data] FROM LOGIN [data];
    
    GRANT SELECT ON [dbo].[BoundaryLevel] TO [data];
    GRANT SELECT ON [dbo].[Filter] TO [data];
    GRANT SELECT ON [dbo].[FilterFootnote] TO [data];
    GRANT SELECT ON [dbo].[FilterGroup] TO [data];
    GRANT SELECT ON [dbo].[FilterGroupFootnote] TO [data];
    GRANT SELECT ON [dbo].[FilterItem] TO [data];
    GRANT SELECT ON [dbo].[FilterItemFootnote] TO [data];
    GRANT SELECT ON [dbo].[Footnote] TO [data];
    GRANT SELECT ON [dbo].[geometry] TO [data];
    GRANT SELECT ON [dbo].[geometry_columns] TO [data];
    GRANT SELECT ON [dbo].[Indicator] TO [data];
    GRANT SELECT ON [dbo].[IndicatorFootnote] TO [data];
    GRANT SELECT ON [dbo].[IndicatorGroup] TO [data];
    GRANT SELECT ON [dbo].[Location] TO [data];
    GRANT SELECT ON [dbo].[Observation] TO [data];
    GRANT SELECT ON [dbo].[ObservationFilterItem] TO [data];
    GRANT SELECT ON [dbo].[Release] TO [data];
    GRANT SELECT ON [dbo].[ReleaseFootnote] TO [data];
    GRANT SELECT ON [dbo].[ReleaseSubject] TO [data];
    GRANT SELECT ON [dbo].[spatial_ref_sys] TO [data];
    GRANT SELECT ON [dbo].[Subject] TO [data];
    GRANT SELECT ON [dbo].[SubjectFootnote] TO [data];
    
    GRANT EXECUTE ON TYPE::IdListGuidType TO [data];
    GRANT EXECUTE ON OBJECT::FilteredFootnotes TO [data];
    GRANT SELECT ON OBJECT::geojson TO [data];
END