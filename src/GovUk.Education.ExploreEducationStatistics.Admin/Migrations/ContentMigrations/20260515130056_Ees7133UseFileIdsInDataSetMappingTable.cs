using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7133UseFileIdsInDataSetMappingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OriginalDataFileId",
                table: "DataSetMappings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacementDataFileId",
                table: "DataSetMappings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.Sql(
                """
                UPDATE DSM
                SET DSM.OriginalDataFileId = F.Id
                FROM DataSetMappings AS DSM
                INNER JOIN Files AS F
                    ON DSM.OriginalDataSetId = F.SubjectId AND F.Type = 'Data';
                """
            );

            migrationBuilder.Sql(
                """
                UPDATE DSM
                SET DSM.ReplacementDataFileId = F.Id
                FROM DataSetMappings AS DSM
                INNER JOIN Files AS F
                    ON DSM.OriginalDataSetId = F.SubjectId AND F.Type = 'Data';
                """
            );

            migrationBuilder.DropColumn(name: "OriginalDataSetId", table: "DataSetMappings");

            migrationBuilder.DropColumn(name: "ReplacementDataSetId", table: "DataSetMappings");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetMappings_OriginalDataFileId",
                table: "DataSetMappings",
                column: "OriginalDataFileId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataSetMappings_ReplacementDataFileId",
                table: "DataSetMappings",
                column: "ReplacementDataFileId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DataSetMappings_Files_OriginalDataFileId",
                table: "DataSetMappings",
                column: "OriginalDataFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DataSetMappings_Files_ReplacementDataFileId",
                table: "DataSetMappings",
                column: "ReplacementDataFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "OriginalDataFileId", table: "DataSetMappings");

            migrationBuilder.DropColumn(name: "ReplacementDataFileId", table: "DataSetMappings");
        }
    }
}
