using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2156_MigrateMethodologyTitleAndSlugToParentAndAddAlternativeTitleField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwningPublicationTitle",
                table: "MethodologyParents",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "MethodologyParents",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AlternativeTitle",
                table: "Methodologies",
                nullable: true);

            // Set the default Methodology titles to fall back to the owning Publications' titles.
            migrationBuilder.Sql(@"
                UPDATE MethodologyParents 
                SET MethodologyParents.OwningPublicationTitle = Publications.Title
                FROM MethodologyParents 
                JOIN PublicationMethodologies 
                  ON PublicationMethodologies.MethodologyParentId = MethodologyParents.Id 
                  AND PublicationMethodologies.Owner = 1 
                JOIN Publications
                  ON Publications.Id = PublicationMethodologies.PublicationId");
            
            // Explicitly set an alternative title for any Methodologies whose titles don't conform with those of their
            // owning Publications.
            migrationBuilder.Sql(@"
                UPDATE Methodologies
                SET Methodologies.AlternativeTitle = Methodologies.Title
                FROM Methodologies
                JOIN MethodologyParents ON MethodologyParents.Id = Methodologies.MethodologyParentId
                WHERE Methodologies.Title != MethodologyParents.OwningPublicationTitle
                AND Methodologies.Title != CONCAT(MethodologyParents.OwningPublicationTitle, ': methodology')");

            // Retain existing Methodologies' slugs, as they should never change.
            migrationBuilder.Sql(@"
                UPDATE MethodologyParents 
                SET MethodologyParents.Slug = Methodologies.Slug
                FROM MethodologyParents
                JOIN Methodologies ON Methodologies.MethodologyParentId = MethodologyParents.Id");
            
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Methodologies");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Methodologies");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwningPublicationTitle",
                table: "MethodologyParents");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "MethodologyParents");

            migrationBuilder.DropColumn(
                name: "AlternativeTitle",
                table: "Methodologies");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Methodologies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Methodologies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
