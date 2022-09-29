IF EXISTS (SELECT name 
            FROM [sys].[server_principals]
            WHERE name = 'adminapp')
AND NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'adminapp')
Begin
    CREATE USER [adminapp] FROM LOGIN [adminapp];
    ALTER ROLE [db_ddladmin] ADD MEMBER [adminapp];
    ALTER ROLE [db_datareader] ADD MEMBER [adminapp];
    ALTER ROLE [db_datawriter] ADD MEMBER [adminapp];
End

IF EXISTS (SELECT name 
            FROM [sys].[server_principals]
            WHERE name = 'importer')
AND NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'importer')
Begin
    CREATE USER [importer] FROM LOGIN [importer];
--     ALTER ROLE [db_ddladmin] ADD MEMBER [importer];
    ALTER ROLE [db_datareader] ADD MEMBER [importer];
    ALTER ROLE [db_datawriter] ADD MEMBER [importer];
End

IF EXISTS (SELECT name 
            FROM [sys].[server_principals]
            WHERE name = 'publisher')
AND NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'publisher')
Begin
    CREATE USER [publisher] FROM LOGIN [publisher];
--     ALTER ROLE [db_ddladmin] ADD MEMBER [publisher];
    ALTER ROLE [db_datareader] ADD MEMBER [publisher];
    ALTER ROLE [db_datawriter] ADD MEMBER [publisher];
end


IF EXISTS (SELECT name 
            FROM [sys].[server_principals]
            WHERE name = 'content')
AND NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'content')
Begin
    CREATE USER [content] FROM LOGIN [content];
    
    GRANT SELECT ON [dbo].[Comment] TO [content];
    GRANT SELECT ON [dbo].[Contacts] TO [content];
    GRANT SELECT ON [dbo].[ContentBlock] TO [content];
    GRANT SELECT ON [dbo].[ContentSections] TO [content];
    GRANT SELECT ON [dbo].[DataImports] TO [content];
    GRANT SELECT ON [dbo].[ExternalMethodology] TO [content];
    GRANT SELECT ON [dbo].[Files] TO [content];
    GRANT SELECT ON [dbo].[GlossaryEntries] TO [content];
    GRANT SELECT ON [dbo].[LegacyReleases] TO [content];
    GRANT SELECT ON [dbo].[Methodologies] TO [content];
    GRANT SELECT ON [dbo].[MethodologyFiles] TO [content];
    GRANT SELECT ON [dbo].[MethodologyNotes] TO [content];
    GRANT SELECT ON [dbo].[MethodologyVersions] TO [content];
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

IF EXISTS (SELECT name 
            FROM [sys].[server_principals]
            WHERE name = 'data')
AND NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'data')
Begin
    CREATE USER [data] FROM LOGIN [data];
    
    GRANT SELECT ON [dbo].[Comment] TO [data];
    GRANT SELECT ON [dbo].[Contacts] TO [data];
    GRANT SELECT ON [dbo].[ContentBlock] TO [data];
    GRANT SELECT ON [dbo].[ContentSections] TO [data];
    GRANT SELECT ON [dbo].[DataImports] TO [data];
    GRANT SELECT ON [dbo].[ExternalMethodology] TO [data];
    GRANT SELECT ON [dbo].[Files] TO [data];
    GRANT SELECT ON [dbo].[GlossaryEntries] TO [data];
    GRANT SELECT ON [dbo].[LegacyReleases] TO [data];
    GRANT SELECT ON [dbo].[Methodologies] TO [data];
    GRANT SELECT ON [dbo].[MethodologyFiles] TO [data];
    GRANT SELECT ON [dbo].[MethodologyNotes] TO [data];
    GRANT SELECT ON [dbo].[MethodologyVersions] TO [data];
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