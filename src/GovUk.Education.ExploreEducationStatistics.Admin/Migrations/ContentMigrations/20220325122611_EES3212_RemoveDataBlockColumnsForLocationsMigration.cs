using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3212_RemoveDataBlockColumnsForLocationsMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop two columns which were used to flag the status of Data Blocks during the migration
            migrationBuilder.DropColumn(
                name: "DataBlock_LocationsMigrated",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "DataBlock_TableHeaderCountChanged",
                table: "ContentBlock");

            // Drop the original Data Block config columns 
            migrationBuilder.DropColumn(
                name: "DataBlock_Charts",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "DataBlock_Table",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "DataBlock_Query",
                table: "ContentBlock");

            // Rename the migrated Data Block config columns
            migrationBuilder.RenameColumn(
                name: "DataBlock_ChartsMigrated",
                newName: "DataBlock_Charts",
                table: "ContentBlock");

            migrationBuilder.RenameColumn(
                name: "DataBlock_TableMigrated",
                newName: "DataBlock_Table",
                table: "ContentBlock");

            migrationBuilder.RenameColumn(
                name: "DataBlock_QueryMigrated",
                newName: "DataBlock_Query",
                table: "ContentBlock");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataBlock_ChartsMigrated",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DataBlock_TableHeaderCountChanged",
                table: "ContentBlock",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.RenameColumn(
                name: "DataBlock_Charts",
                newName: "DataBlock_ChartsMigrated",
                table: "ContentBlock");

            migrationBuilder.RenameColumn(
                name: "DataBlock_Table",
                newName: "DataBlock_TableMigrated",
                table: "ContentBlock");

            migrationBuilder.RenameColumn(
                name: "DataBlock_Query",
                newName: "DataBlock_QueryMigrated",
                table: "ContentBlock");
        }
    }
}
