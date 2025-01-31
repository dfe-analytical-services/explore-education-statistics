IF NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'importer')
Begin
    CREATE USER [importer] FROM LOGIN [importer];

    GRANT ALL ON [dbo].[Comment] TO [importer];
    GRANT ALL ON [dbo].[Contacts] TO [importer];
    GRANT ALL ON [dbo].[ContentBlock] TO [importer];
    GRANT ALL ON [dbo].[ContentSections] TO [importer];
    GRANT ALL ON [dbo].[DataImports] TO [importer];
    GRANT ALL ON [dbo].[DataImportErrors] TO [importer];
    GRANT ALL ON [dbo].[ExternalMethodology] TO [importer];
    GRANT ALL ON [dbo].[Files] TO [importer];
    GRANT ALL ON [dbo].[GlossaryEntries] TO [importer];
    GRANT ALL ON [dbo].[LegacyReleases] TO [importer];
    GRANT ALL ON [dbo].[Methodologies] TO [importer];
    GRANT ALL ON [dbo].[MethodologyFiles] TO [importer];
    GRANT ALL ON [dbo].[MethodologyNotes] TO [importer];
    GRANT ALL ON [dbo].[MethodologyVersions] TO [importer];
    GRANT ALL ON [dbo].[Permalinks] TO [importer];
    GRANT ALL ON [dbo].[PublicationMethodologies] TO [importer];
    GRANT ALL ON [dbo].[Publications] TO [importer];
    GRANT ALL ON [dbo].[ReleaseContentBlocks] TO [importer];
    GRANT ALL ON [dbo].[ReleaseContentSections] TO [importer];
    GRANT ALL ON [dbo].[ReleaseFiles] TO [importer];
    GRANT ALL ON [dbo].[Releases] TO [importer];
    GRANT ALL ON [dbo].[ReleaseStatus] TO [importer];
    GRANT ALL ON [dbo].[Themes] TO [importer];
    GRANT ALL ON [dbo].[Topics] TO [importer];
    GRANT ALL ON [dbo].[Update] TO [importer];
End

IF NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'publisher')
Begin
    CREATE USER [publisher] FROM LOGIN [publisher];

    GRANT ALL ON [dbo].[Comment] TO [publisher];
    GRANT ALL ON [dbo].[Contacts] TO [publisher];
    GRANT ALL ON [dbo].[ContentBlock] TO [publisher];
    GRANT ALL ON [dbo].[ContentSections] TO [publisher];
    GRANT ALL ON [dbo].[DataImports] TO [publisher];
    GRANT ALL ON [dbo].[DataImportErrors] TO [publisher];
    GRANT ALL ON [dbo].[ExternalMethodology] TO [publisher];
    GRANT ALL ON [dbo].[Files] TO [publisher];
    GRANT ALL ON [dbo].[GlossaryEntries] TO [publisher];
    GRANT ALL ON [dbo].[LegacyReleases] TO [publisher];
    GRANT ALL ON [dbo].[Methodologies] TO [publisher];
    GRANT ALL ON [dbo].[MethodologyFiles] TO [publisher];
    GRANT ALL ON [dbo].[MethodologyNotes] TO [publisher];
    GRANT ALL ON [dbo].[MethodologyVersions] TO [publisher];
    GRANT ALL ON [dbo].[Permalinks] TO [publisher];
    GRANT ALL ON [dbo].[PublicationMethodologies] TO [publisher];
    GRANT ALL ON [dbo].[Publications] TO [publisher];
    GRANT ALL ON [dbo].[ReleaseContentBlocks] TO [publisher];
    GRANT ALL ON [dbo].[ReleaseContentSections] TO [publisher];
    GRANT ALL ON [dbo].[ReleaseFiles] TO [publisher];
    GRANT ALL ON [dbo].[Releases] TO [publisher];
    GRANT ALL ON [dbo].[ReleaseStatus] TO [publisher];
    GRANT ALL ON [dbo].[Themes] TO [publisher];
    GRANT ALL ON [dbo].[Topics] TO [publisher];
    GRANT ALL ON [dbo].[Update] TO [publisher];
end


IF NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'content')
Begin
    CREATE USER [content] FROM LOGIN [content];
    
    GRANT SELECT ON [dbo].[Comment] TO [content];
    GRANT SELECT ON [dbo].[Contacts] TO [content];
    GRANT SELECT ON [dbo].[ContentBlock] TO [content];
    GRANT SELECT ON [dbo].[ContentSections] TO [content];
    GRANT SELECT ON [dbo].[DataImports] TO [content];
    GRANT SELECT ON [dbo].[DataImportErrors] TO [content];
    GRANT SELECT ON [dbo].[ExternalMethodology] TO [content];
    GRANT SELECT ON [dbo].[Files] TO [content];
    GRANT SELECT ON [dbo].[GlossaryEntries] TO [content];
    GRANT SELECT ON [dbo].[LegacyReleases] TO [content];
    GRANT SELECT ON [dbo].[Methodologies] TO [content];
    GRANT SELECT ON [dbo].[MethodologyFiles] TO [content];
    GRANT SELECT ON [dbo].[MethodologyNotes] TO [content];
    GRANT SELECT ON [dbo].[MethodologyVersions] TO [content];
    GRANT SELECT ON [dbo].[Permalinks] TO [content];
    GRANT SELECT ON [dbo].[PublicationMethodologies] TO [content];
    GRANT SELECT ON [dbo].[Publications] TO [content];
    GRANT SELECT ON [dbo].[ReleaseContentBlocks] TO [content];
    GRANT SELECT ON [dbo].[ReleaseContentSections] TO [content];
    GRANT SELECT ON [dbo].[ReleaseFiles] TO [content];
    GRANT SELECT ON [dbo].[Releases] TO [content];
    GRANT SELECT ON [dbo].[ReleaseStatus] TO [content];
    GRANT SELECT ON [dbo].[Themes] TO [content];
    GRANT SELECT ON [dbo].[Topics] TO [content];
    GRANT SELECT ON [dbo].[Update] TO [content];
end

IF NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'data')
Begin
    CREATE USER [data] FROM LOGIN [data];
    
    GRANT SELECT ON [dbo].[Comment] TO [data];
    GRANT SELECT ON [dbo].[Contacts] TO [data];
    GRANT SELECT ON [dbo].[ContentBlock] TO [data];
    GRANT SELECT ON [dbo].[ContentSections] TO [data];
    GRANT SELECT ON [dbo].[DataImports] TO [data];
    GRANT SELECT ON [dbo].[DataImportErrors] TO [data];
    GRANT SELECT ON [dbo].[ExternalMethodology] TO [data];
    GRANT SELECT ON [dbo].[Files] TO [data];
    GRANT SELECT ON [dbo].[GlossaryEntries] TO [data];
    GRANT SELECT ON [dbo].[LegacyReleases] TO [data];
    GRANT SELECT ON [dbo].[Methodologies] TO [data];
    GRANT SELECT ON [dbo].[MethodologyFiles] TO [data];
    GRANT SELECT ON [dbo].[MethodologyNotes] TO [data];
    GRANT SELECT ON [dbo].[MethodologyVersions] TO [data];
    GRANT INSERT ON [dbo].[Permalinks] TO [data];
    GRANT SELECT ON [dbo].[PublicationMethodologies] TO [data];
    GRANT SELECT ON [dbo].[Publications] TO [data];
    GRANT SELECT ON [dbo].[ReleaseContentBlocks] TO [data];
    GRANT SELECT ON [dbo].[ReleaseContentSections] TO [data];
    GRANT SELECT ON [dbo].[ReleaseFiles] TO [data];
    GRANT SELECT ON [dbo].[Releases] TO [data];
    GRANT SELECT ON [dbo].[ReleaseStatus] TO [data];
    GRANT SELECT ON [dbo].[Themes] TO [data];
    GRANT SELECT ON [dbo].[Topics] TO [data];
    GRANT SELECT ON [dbo].[Update] TO [data];
End