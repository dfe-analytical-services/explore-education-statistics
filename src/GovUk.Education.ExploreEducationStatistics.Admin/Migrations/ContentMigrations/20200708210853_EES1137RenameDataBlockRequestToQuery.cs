using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1137RenameDataBlockRequestToQuery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataBlock_Request",
                table: "ContentBlock",
                newName: "DataBlock_Query"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataBlock_Query",
                table: "ContentBlock",
                newName: "DataBlock_Request"
            );
        }
    }
}
