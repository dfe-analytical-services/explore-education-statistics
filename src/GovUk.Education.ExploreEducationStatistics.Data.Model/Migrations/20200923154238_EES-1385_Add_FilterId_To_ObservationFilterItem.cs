using System;
using Microsoft.EntityFrameworkCore.Migrations;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1385_Add_FilterId_To_ObservationFilterItem : Migration
    {
        private const string MigrationId = "20200923154238";
        private const string PreviousInsertObservationFilterItemsMigrationId = "20200304145743";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FilterId",
                table: "ObservationFilterItem",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObservationFilterItem_FilterId",
                table: "ObservationFilterItem",
                column: "FilterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ObservationFilterItem_Filter_FilterId",
                table: "ObservationFilterItem",
                column: "FilterId",
                principalTable: "Filter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservationFilterItems");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationFilterItemType");

            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ObservationFilterItemType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousInsertObservationFilterItemsMigrationId}_Routine_InsertObservationFilterItems.sql");
            
            //migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Populate_Ofi_FilterId.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservations");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationFilterItemType");

            migrationBuilder.DropForeignKey(
                name: "FK_ObservationFilterItem_Filter_FilterId",
                table: "ObservationFilterItem");

            migrationBuilder.DropIndex(
                name: "IX_ObservationFilterItem_FilterId",
                table: "ObservationFilterItem");

            migrationBuilder.DropColumn(
                name: "FilterId",
                table: "ObservationFilterItem");
            
            migrationBuilder.Sql("CREATE TYPE ObservationFilterItemType AS table( ObservationId uniqueidentifier NOT NULL, FilterItemId  uniqueidentifier NOT NULL)");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousInsertObservationFilterItemsMigrationId}_Routine_InsertObservationFilterItems.sql");
        }
    }
}
