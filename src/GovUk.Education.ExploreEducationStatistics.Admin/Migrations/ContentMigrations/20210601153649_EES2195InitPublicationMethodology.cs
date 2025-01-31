using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2195InitPublicationMethodology : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Methodologies_MethodologyParents_MethodologyParentId",
                table: "Methodologies");

            // Each existing Methodology will become the first version of a new parent Methodology
            // In preparation for this new parent, set a new random parent Id
            migrationBuilder.Sql(
                "UPDATE Methodologies SET MethodologyParentId = NEWID() WHERE MethodologyParentId IS NULL");

            // Insert a new parent for each existing Methodology using the new random parent Id's
            migrationBuilder.Sql(
                "INSERT INTO MethodologyParents (Id) SELECT MethodologyParentId FROM Methodologies WHERE MethodologyParentId IS NOT NULL");

            // Make the parent Id column not null
            migrationBuilder.AlterColumn<Guid>(
                name: "MethodologyParentId",
                table: "Methodologies",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Methodologies_MethodologyParents_MethodologyParentId",
                table: "Methodologies",
                column: "MethodologyParentId",
                principalTable: "MethodologyParents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Link all the Publications that are using Methodologies to the new parent Methodology
            migrationBuilder.Sql(@"
INSERT INTO PublicationMethodologies (PublicationId, MethodologyParentId, Owner)
SELECT P.Id, M.MethodologyParentId, 0
FROM Publications P
JOIN Methodologies M ON P.MethodologyId = M.Id
");

            // Where a Methodology is used by a single Publication, make that Publication the owner of the Methodology
            migrationBuilder.Sql(@"
UPDATE PublicationMethodologies SET Owner = 1
WHERE MethodologyParentId IN (
  SELECT MethodologyParentId
  FROM PublicationMethodologies PM
  GROUP BY PM.MethodologyParentId
  HAVING COUNT(PublicationId) = 1
)
");

            // Where a Methodology is used by more than one Publication, no owner is set automatically
            // After the migration we will need to check these cases, choose one Publication as the owner and set it
            // Existing orphaned Methodologies will remain orphaned and will need deleting after investigation
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Methodologies_MethodologyParents_MethodologyParentId",
                table: "Methodologies");

            migrationBuilder.AlterColumn<Guid>(
                name: "MethodologyParentId",
                table: "Methodologies",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_Methodologies_MethodologyParents_MethodologyParentId",
                table: "Methodologies",
                column: "MethodologyParentId",
                principalTable: "MethodologyParents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
