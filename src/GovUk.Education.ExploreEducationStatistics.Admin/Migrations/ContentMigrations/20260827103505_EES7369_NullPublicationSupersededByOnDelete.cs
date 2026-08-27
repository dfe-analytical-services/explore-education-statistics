using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES7369_NullPublicationSupersededByOnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Publications_Publications_SupersededById", table: "Publications");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Publications_SupersededById",
                table: "Publications",
                column: "SupersededById",
                principalTable: "Publications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Publications_Publications_SupersededById", table: "Publications");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Publications_SupersededById",
                table: "Publications",
                column: "SupersededById",
                principalTable: "Publications",
                principalColumn: "Id"
            );
        }
    }
}
