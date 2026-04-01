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
                table: TableName,
                sql: "[UseGISLogo] = CASE WHEN [GISLogoHexCode] IS NULL THEN 0 ELSE 1 END"
            );

            var now = DateTimeOffset.UtcNow.ToString("O");

            // Update Department for Education
            migrationBuilder.Sql(
                $"UPDATE dbo.Organisations SET Updated = '{now}', UseGISLogo = 'True', GISLogoHexCode = '#003764', LogoFileName = 'govuk-crest.svg' WHERE Id = '5e089801-cf1a-b375-acd3-88e9d8aece66';"
            );

            // Update Skills England
            migrationBuilder.Sql(
                $"UPDATE dbo.Organisations SET Updated = '{now}', UseGISLogo = 'True', GISLogoHexCode = '#00bcb5', LogoFileName = 'govuk-crest.svg' WHERE Id = '5e089801-ce1a-e274-9915-e83f3e978699';"
            );

            // Add Department for Work and Pensions
            migrationBuilder.Sql(
                $@"
                IF NOT EXISTS (SELECT 1 FROM dbo.Organisations WHERE Title = N'Department for Work and Pensions')
                BEGIN
                    INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) 
                    VALUES (N'ECCFF2B6-7F81-4AF3-917E-7BB7D5650975', N'Department for Work and Pensions', N'https://www.gov.uk/government/organisations/department-for-work-pensions', '{now}', null, 'True', '#00bcb5', 'govuk-crest.svg');
                END"
            );

            // Add Ofsted
            migrationBuilder.Sql(
                $@"
                IF NOT EXISTS (SELECT 1 FROM dbo.Organisations WHERE Title = N'Ofsted')
                BEGIN
                    INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) 
                    VALUES (N'DCDA7BE0-FCDB-4671-960E-3AD49BD47097', N'Ofsted', N'https://www.gov.uk/government/organisations/ofsted', '{now}', null, 'False', null, 'ofsted-logo.png');
                END"
            );

            // Add Ofqual
            migrationBuilder.Sql(
                $@"
                IF NOT EXISTS (SELECT 1 FROM dbo.Organisations WHERE Title = N'Ofqual')
                BEGIN
                    INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) 
                    VALUES (N'FFF076EF-C9FD-45F7-A0A5-1266755BD168', N'Ofqual', N'https://www.gov.uk/government/organisations/ofqual', '{now}', null, 'False', null, 'ofqual-logo.png');
                END"
            );

            // Add Department for Health and Social Care
            // or update it to have use 'govuk-crest.svg' as LogoFileName and `UseGISLogo` = true
            migrationBuilder.Sql(
                $@"
                IF NOT EXISTS (SELECT 1 FROM dbo.Organisations WHERE Title = N'Department for Education')
                BEGIN
                    INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) 
                    VALUES (N'85087F95-654A-4869-906C-774A1A70308C', N'Department for Health and Social Care', N'https://www.gov.uk/government/organisations/department-of-health-and-social-care', '{now}', null, 'True', '#00ad93', 'govuk-crest.svg');
                END
                ELSE
                BEGIN
                    UPDATE dbo.Organisations 
                    SET Updated = '{now}', 
                        UseGISLogo = 'True', 
                        GISLogoHexCode = '#00ad93', 
                        LogoFileName = 'govuk-crest.svg' 
                    WHERE Id = '85087F95-654A-4869-906C-774A1A70308C';
                END"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(name: "CK_Organisations_GISLogoHexCode", table: TableName);
            migrationBuilder.DropColumn(name: "GISLogoHexCode", table: TableName);
            migrationBuilder.DropColumn(name: "LogoFileName", table: TableName);
            migrationBuilder.DropColumn(name: "UseGISLogo", table: TableName);

            // Revert Updates
            migrationBuilder.Sql(
                "UPDATE dbo.Organisations SET Updated = null WHERE Id IN ('5e089801-cf1a-b375-acd3-88e9d8aece66', '5e089801-ce1a-e274-9915-e83f3e978699');"
            );

            // Delete Added Rows
            migrationBuilder.Sql(
                """
                    DELETE FROM dbo.Organisations 
                    WHERE Id IN (
                        'ECCFF2B6-7F81-4AF3-917E-7BB7D5650975', 
                        'DCDA7BE0-FCDB-4671-960E-3AD49BD47097', 
                        'FFF076EF-C9FD-45F7-A0A5-1266755BD168', 
                        '85087F95-654A-4869-906C-774A1A70308C'
                    );
                """
            );
        }
    }
}
