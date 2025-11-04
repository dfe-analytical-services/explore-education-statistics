#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5817_AddReleasePublishingFeedbackTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ReleasePublishingFeedback",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmailToken = table.Column<string>(type: "nvarchar(55)", maxLength: 55, nullable: false),
                ReleaseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserPublicationRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Response = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                AdditionalFeedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                FeedbackReceived = table.Column<DateTime>(type: "datetime2", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReleasePublishingFeedback", x => x.Id);
                table.ForeignKey(
                    name: "FK_ReleasePublishingFeedback_ReleaseVersions_ReleaseVersionId",
                    column: x => x.ReleaseVersionId,
                    principalTable: "ReleaseVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_ReleasePublishingFeedback_ReleaseVersionId",
            table: "ReleasePublishingFeedback",
            column: "ReleaseVersionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_ReleasePublishingFeedback_Token",
            table: "ReleasePublishingFeedback",
            column: "EmailToken",
            unique: true
        );

        migrationBuilder.Sql("GRANT INSERT ON dbo.ReleasePublishingFeedback TO [content];");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ReleasePublishingFeedback");
    }
}
