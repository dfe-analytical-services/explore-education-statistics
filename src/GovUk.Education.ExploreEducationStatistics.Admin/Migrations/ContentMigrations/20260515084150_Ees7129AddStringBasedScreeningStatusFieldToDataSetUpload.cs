using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees7129AddStringBasedScreeningStatusFieldToDataSetUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScreeningStatus",
                table: "DataSetUploads",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.Sql("UPDATE DataSetUploads SET ScreeningStatus = 'Screening' WHERE Status = '0'");
            migrationBuilder.Sql("UPDATE DataSetUploads SET ScreeningStatus = 'ScreenerError' WHERE Status = '1'");
            migrationBuilder.Sql("UPDATE DataSetUploads SET ScreeningStatus = 'FailedScreening' WHERE Status = '2'");
            migrationBuilder.Sql("UPDATE DataSetUploads SET ScreeningStatus = 'PendingReview' WHERE Status = '3'");
            migrationBuilder.Sql("UPDATE DataSetUploads SET ScreeningStatus = 'PendingImport' WHERE Status = '4'");

            migrationBuilder.AlterColumn<int>(
                name: "ScreeningStatus",
                table: "DataSetUploads",
                type: "nvarchar(max)",
                nullable: false,
                oldNullable: true,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ScreeningStatus", table: "DataSetUploads");
        }
    }
}
