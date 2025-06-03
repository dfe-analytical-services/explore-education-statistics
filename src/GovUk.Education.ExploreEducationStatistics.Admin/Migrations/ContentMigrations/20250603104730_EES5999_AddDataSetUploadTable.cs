using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES5999_AddDataSetUploadTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSetUploads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReleaseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataSetTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetaFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetaFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetUploads", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataSetUploads");
        }
    }
}
