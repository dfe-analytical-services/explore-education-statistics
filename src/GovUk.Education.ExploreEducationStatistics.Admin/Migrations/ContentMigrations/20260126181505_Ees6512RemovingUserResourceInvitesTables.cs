using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees6512RemovingUserResourceInvitesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserPublicationInvites");

            migrationBuilder.DropTable(name: "UserReleaseInvites");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPublicationInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(450)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPublicationInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPublicationInvites_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_UserPublicationInvites_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "UserReleaseInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReleaseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReleaseInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReleaseInvites_ReleaseVersions_ReleaseVersionId",
                        column: x => x.ReleaseVersionId,
                        principalTable: "ReleaseVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_UserReleaseInvites_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationInvites_CreatedById",
                table: "UserPublicationInvites",
                column: "CreatedById"
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationInvites_PublicationId_Email_Role",
                table: "UserPublicationInvites",
                columns: new[] { "PublicationId", "Email", "Role" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseInvites_CreatedById",
                table: "UserReleaseInvites",
                column: "CreatedById"
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseInvites_ReleaseVersionId_Email_Role",
                table: "UserReleaseInvites",
                columns: new[] { "ReleaseVersionId", "Email", "Role" },
                unique: true
            );
        }
    }
}
