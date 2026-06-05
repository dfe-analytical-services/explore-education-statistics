using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7150UseFileIdsInDataSetMappingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_DataSetMappings_OriginalDataSetId", table: "DataSetMappings");

            migrationBuilder.DropIndex(name: "IX_DataSetMappings_ReplacementDataSetId", table: "DataSetMappings");

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
                    ON DSM.ReplacementDataSetId = F.SubjectId AND F.Type = 'Data';
                """
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataSetMappings_OriginalDataFileId",
                table: "DataSetMappings",
                column: "OriginalDataFileId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataSetMappings_ReplacementDataFileId",
                table: "DataSetMappings",
                column: "ReplacementDataFileId",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DataSetMappings_Files_OriginalDataFileId",
                table: "DataSetMappings",
                column: "OriginalDataFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict // cannot be Cascade as "may cause cycles or multiple cascade paths"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_DataSetMappings_Files_ReplacementDataFileId",
                table: "DataSetMappings",
                column: "ReplacementDataFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict // cannot be Cascade as "may cause cycles or multiple cascade paths"
            );

            migrationBuilder.DropColumn(name: "OriginalDataSetId", table: "DataSetMappings");

            migrationBuilder.DropColumn(name: "ReplacementDataSetId", table: "DataSetMappings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
