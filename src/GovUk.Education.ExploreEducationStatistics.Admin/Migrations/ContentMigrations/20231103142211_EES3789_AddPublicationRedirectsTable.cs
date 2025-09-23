#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

// ReSharper disable once InconsistentNaming
public partial class EES3789_AddPublicationRedirectsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PublicationRedirects",
            columns: table => new
            {
                Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                PublicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PublicationRedirects", x => new { x.PublicationId, x.Slug });
                table.ForeignKey(
                    name: "FK_PublicationRedirects_Publications_PublicationId",
                    column: x => x.PublicationId,
                    principalTable: "Publications",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql("GRANT SELECT ON dbo.PublicationRedirects TO [content];");
        migrationBuilder.Sql("GRANT SELECT ON dbo.PublicationRedirects TO [publisher];");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PublicationRedirects");
    }
}
