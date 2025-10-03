using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class EES5170_AddBoundaryDataTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "Updated",
            table: "BoundaryLevel",
            type: "datetime2",
            nullable: true
        );

        migrationBuilder.CreateTable(
            name: "BoundaryData",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Code = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                GeoJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                BoundaryLevelId = table.Column<long>(type: "bigint", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BoundaryData", x => x.Id);
                table.ForeignKey(
                    name: "FK_BoundaryData_BoundaryLevel_BoundaryLevelId",
                    column: x => x.BoundaryLevelId,
                    principalTable: "BoundaryLevel",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_BoundaryData_BoundaryLevelId",
            table: "BoundaryData",
            column: "BoundaryLevelId"
        );

        migrationBuilder.CreateIndex(name: "IX_BoundaryData_Code", table: "BoundaryData", column: "Code");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "BoundaryData");

        migrationBuilder.DropColumn(name: "Updated", table: "BoundaryLevel");
    }
}
