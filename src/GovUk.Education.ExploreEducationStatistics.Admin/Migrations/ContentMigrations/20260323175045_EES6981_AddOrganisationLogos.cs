using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES6981_AddOrganisationLogos : Migration
    {
        /// <inheritdoc />
        private const string TableName = "Organisations";

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
                $"UPDATE dbo.Organisations SET Updated = '{DateTimeOffset.UtcNow:O}', UseGISLogo = 'True', GISLogoHexCode = '#003764', LogoFileName = 'govuk-crest.svg' WHERE Id = '5e089801-cf1a-b375-acd3-88e9d8aece66';"
            );

            // Update Skills England
            migrationBuilder.Sql(
                $"UPDATE dbo.Organisations SET Updated = '{DateTimeOffset.UtcNow:O}', UseGISLogo = 'True', GISLogoHexCode = '#00bcb5', LogoFileName = 'govuk-crest.svg' WHERE Id = '5e089801-ce1a-e274-9915-e83f3e978699';"
            );

            // Add Department for Work & Pensions
            migrationBuilder.Sql(
                $"INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) VALUES (N'ECCFF2B6-7F81-4AF3-917E-7BB7D5650975', N'Department for Work & Pensions', N'https://www.gov.uk/government/organisations/department-for-work-pensions', '{DateTimeOffset.UtcNow:O}', null, 'True', '#00bcb5', 'govuk-crest.svg');"
            );

            // Add Ofsted
            migrationBuilder.Sql(
                $"INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) VALUES (N'DCDA7BE0-FCDB-4671-960E-3AD49BD47097', N'Ofsted', N'https://www.gov.uk/government/organisations/ofsted', '{DateTimeOffset.UtcNow:O}', null, 'False', null, 'ofsted-logo.png');"
            );

            // Add Ofqual
            migrationBuilder.Sql(
                $"INSERT INTO dbo.Organisations (Id, Title, Url, Created, Updated, UseGISLogo, GISLogoHexCode, LogoFileName) VALUES (N'FFF076EF-C9FD-45F7-A0A5-1266755BD168', N'Ofqual', N'https://www.gov.uk/government/organisations/ofqual', '{DateTimeOffset.UtcNow:O}', null, 'False', null, 'ofqual-logo.png');"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(name: "CK_Organisations_GISLogoHexCode", table: "Organisations");

            migrationBuilder.DropColumn(name: "GISLogoHexCode", table: TableName);

            migrationBuilder.DropColumn(name: "LogoFileName", table: TableName);

            migrationBuilder.DropColumn(name: "UseGISLogo", table: TableName);

            // Revert Department for Education
            migrationBuilder.Sql(
                $"UPDATE dbo.Organisations SET Updated = null, WHERE Id = '5e089801-cf1a-b375-acd3-88e9d8aece66';"
            );

            // Revert Skills England
            migrationBuilder.Sql(
                $"UPDATE dbo.Organisations SET Updated = null, WHERE Id = '5e089801-ce1a-e274-9915-e83f3e978699';"
            );

            // Delete Department for Work & Pensions, Ofsted and Ofqual
            migrationBuilder.Sql(
                $"DELETE FROM dbo.Organisations WHERE Id IN ('ECCFF2B6-7F81-4AF3-917E-7BB7D5650975', 'DCDA7BE0-FCDB-4671-960E-3AD49BD47097', 'FFF076EF-C9FD-45F7-A0A5-1266755BD168');"
            );
        }
    }
}
