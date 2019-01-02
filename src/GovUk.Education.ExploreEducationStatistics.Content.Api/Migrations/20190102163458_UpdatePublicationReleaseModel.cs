using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class UpdatePublicationReleaseModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Published",
                table: "Releases",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextUpdate",
                table: "Publications",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "PublicationId", "Published", "Title" },
                values: new object[] { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016-17" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.DropColumn(
                name: "Published",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "NextUpdate",
                table: "Publications");
        }
    }
}
