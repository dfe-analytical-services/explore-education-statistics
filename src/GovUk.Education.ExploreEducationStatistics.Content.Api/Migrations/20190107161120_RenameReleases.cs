using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class RenameReleases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Links",
                keyColumn: "Id",
                keyValue: new Guid("8693c112-225e-4e09-80c2-820cb307bc58"));

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "ReleaseName",
                value: "2016 to 2017");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "ReleaseName",
                value: "2015 to 2016");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Links",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("8693c112-225e-4e09-80c2-820cb307bc58"), "2015 to 2016", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2015-to-2016" });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "ReleaseName",
                value: "2016-17");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "ReleaseName",
                value: "2015-16");
        }
    }
}
