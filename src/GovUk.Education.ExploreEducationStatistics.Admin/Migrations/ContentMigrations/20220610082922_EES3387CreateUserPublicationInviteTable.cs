using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3387CreateUserPublicationInviteTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM UserReleaseInvites WHERE Accepted = 1;");

            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "UserReleaseInvites");

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "UserReleaseInvites",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserPublicationInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPublicationInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPublicationInvites_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPublicationInvites_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationInvites_CreatedById",
                table: "UserPublicationInvites",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationInvites_PublicationId",
                table: "UserPublicationInvites",
                column: "PublicationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPublicationInvites");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "UserReleaseInvites");

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "UserReleaseInvites",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
