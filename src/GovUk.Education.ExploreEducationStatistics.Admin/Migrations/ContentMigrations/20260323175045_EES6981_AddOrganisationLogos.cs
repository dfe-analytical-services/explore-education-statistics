using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES6981_AddOrganisationLogos : Migration
    {
        private const string TableName = "Organisations";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GISLogoHexCode",
                table: TableName,
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "LogoFileName",
                table: TableName,
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<bool>(
                name: "UseGISLogo",
                table: TableName,
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddCheckConstraint(
                name: "CK_Organisations_GISLogoHexCode",
                table: "Organisations",
                sql: "[UseGISLogo] = CASE WHEN [GISLogoHexCode] IS NULL THEN 0 ELSE 1 END"
            );

            // Update Department for Education
            migrationBuilder.Sql(
                $"UPDATE dbo.Organisations SET Updated = SYSDATETIMEOFFSET(), UseGISLogo = 'True', GISLogoHexCode = '#003764', LogoFileName = 'govuk-crest.svg' WHERE Title = 'Department for Education';"
            );

            // Update Skills England
            migrationBuilder.Sql(
                $"UPDATE dbo.Organisations SET Updated = SYSDATETIMEOFFSET(), UseGISLogo = 'True', GISLogoHexCode = '#00bcb5', LogoFileName = 'govuk-crest.svg' WHERE Title = 'Skills England';"
            );

            // Add Department for Work and Pensions
            // or update it to use 'govuk-crest.svg' as LogoFileName and `UseGISLogo` = truu
            // if it already exists.
            migrationBuilder.Sql(
                $@"
                IF NOT EXISTS (SELECT 1 FROM dbo.Organisations WHERE Title = N'Department for Work and Pensions')
                BEGIN
                    INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) 
                    VALUES (NEWID(), N'Department for Work and Pensions', N'https://www.gov.uk/government/organisations/department-for-work-pensions', SYSDATETIMEOFFSET(), null, 'True', '#00bcb5', 'govuk-crest.svg');
                END
                ELSE
                BEGIN
                    UPDATE dbo.Organisations 
                    SET Updated = SYSDATETIMEOFFSET(), 
                        UseGISLogo = 'True', 
                        GISLogoHexCode = '#00bcb5', 
                        LogoFileName = 'govuk-crest.svg' 
                    WHERE Title = 'Department for Work and Pensions';
                END"
            );

            // Add Ofsted
            migrationBuilder.Sql(
                $@"
                IF NOT EXISTS (SELECT 1 FROM dbo.Organisations WHERE Title = N'Ofsted')
                BEGIN
                    INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) 
                    VALUES (NEWID(), N'Ofsted', N'https://www.gov.uk/government/organisations/ofsted', SYSDATETIMEOFFSET(), null, 'False', null, 'ofsted-logo.png');
                END"
            );

            // Add Ofqual
            migrationBuilder.Sql(
                $@"
                IF NOT EXISTS (SELECT 1 FROM dbo.Organisations WHERE Title = N'Ofqual')
                BEGIN
                    INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) 
                    VALUES (NEWID(), N'Ofqual', N'https://www.gov.uk/government/organisations/ofqual', SYSDATETIMEOFFSET(), null, 'False', null, 'ofqual-logo.png');
                END"
            );

            // Add Department for Health and Social Care
            // or update it to use 'govuk-crest.svg' as LogoFileName and `UseGISLogo` = true
            migrationBuilder.Sql(
                $@"
                IF NOT EXISTS (SELECT 1 FROM dbo.Organisations WHERE Title = N'Department for Health and Social Care')
                BEGIN   
                    INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) 
                    VALUES (NEWID(), N'Department for Health and Social Care', N'https://www.gov.uk/government/organisations/department-of-health-and-social-care', SYSDATETIMEOFFSET(), null, 'True', '#00ad93', 'govuk-crest.svg');
                END
                ELSE
                BEGIN
                    UPDATE dbo.Organisations 
                    SET Updated = SYSDATETIMEOFFSET(), 
                        UseGISLogo = 'True', 
                        GISLogoHexCode = '#00ad93', 
                        LogoFileName = 'govuk-crest.svg' 
                    WHERE Title = 'Department for Health and Social Care';
                END"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
