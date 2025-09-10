#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES5469_AddFeedbackTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Feedback",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                UserAgent = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                Response = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Context = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Issue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Intent = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Read = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Feedback", x => x.Id);
            });

        migrationBuilder.Sql("GRANT INSERT ON dbo.Feedback TO [content];");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Feedback");
    }
}
