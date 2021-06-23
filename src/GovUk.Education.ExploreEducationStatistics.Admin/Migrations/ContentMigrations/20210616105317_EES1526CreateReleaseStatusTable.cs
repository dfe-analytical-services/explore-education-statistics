using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1526CreateReleaseStatusTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReleaseStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    InternalReleaseNote = table.Column<string>(nullable: false),
                    ApprovalStatus = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: true),
                    CreatedById = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseStatus_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReleaseStatus_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseStatus_CreatedById",
                table: "ReleaseStatus",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseStatus_ReleaseId",
                table: "ReleaseStatus",
                column: "ReleaseId");

            migrationBuilder.Sql(@"
                INSERT INTO ReleaseStatus (Id, ReleaseId, InternalReleaseNote, ApprovalStatus)
                SELECT NEWID(), R.Id, R.InternalReleaseNote, R.ApprovalStatus FROM Releases R
                WHERE R.InternalReleaseNote IS NOT NULL
                AND R.Id NOT IN (SELECT ReleaseStatus.ReleaseId FROM ReleaseStatus);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseStatus");
        }
    }
}
