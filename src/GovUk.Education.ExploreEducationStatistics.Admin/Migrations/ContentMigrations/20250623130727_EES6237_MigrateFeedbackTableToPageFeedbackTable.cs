#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES6237_MigrateFeedbackTableToPageFeedbackTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PageFeedback",
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
                Read = table.Column<bool>(type: "bit", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PageFeedback", x => x.Id);
            }
        );

        migrationBuilder.Sql(
            @"
                INSERT INTO PageFeedback(
                    Id,
                    Created,
                    Url,
                    UserAgent,
                    Response,
                    Context,
                    Issue,
                    Intent,
                    [Read]
                )
                SELECT
                    Id,
                    Created,
                    Url,
                    UserAgent,
                    Response,
                    Context,
                    Issue,
                    Intent,
                    [Read]
                FROM Feedback"
        );

        migrationBuilder.Sql("GRANT INSERT ON dbo.PageFeedback TO [content];");

        migrationBuilder.DropTable(name: "Feedback");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Feedback",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Context = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                Intent = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Issue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Read = table.Column<bool>(type: "bit", nullable: false),
                Response = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                UserAgent = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Feedback", x => x.Id);
            }
        );

        migrationBuilder.Sql(
            @"
                INSERT INTO Feedback(
                    Id,
                    Created,
                    Url,
                    UserAgent,
                    Response,
                    Context,
                    Issue,
                    Intent,
                    [Read]
                )
                SELECT
                    Id,
                    Created,
                    Url,
                    UserAgent,
                    Response,
                    Context,
                    Issue,
                    Intent,
                    [Read]
                FROM PageFeedback"
        );

        migrationBuilder.Sql("GRANT INSERT ON dbo.Feedback TO [content];");

        migrationBuilder.DropTable(name: "PageFeedback");
    }
}
