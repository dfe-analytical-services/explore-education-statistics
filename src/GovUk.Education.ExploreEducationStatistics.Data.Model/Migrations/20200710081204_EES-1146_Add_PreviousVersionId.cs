using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;


namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1146_Add_PreviousVersionId : Migration
    {
        private const string MigrationId = "20200710081204";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PreviousVersionId",
                table: "Release",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Release_PreviousVersionId",
                table: "Release",
                column: "PreviousVersionId");
            
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Update_PreviousVersionId.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Release_PreviousVersionId",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "Release");
        }
    }
}
