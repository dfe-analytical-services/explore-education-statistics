using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class BAU459LabelPreviousReleases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("08134c1d-8a58-49a4-8d8b-22e586ffd5ae"),
                column: "Description",
                value: "Academic Year 2008/09");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("13acf54a-8016-49ff-9050-c61ebe7acad2"),
                column: "Description",
                value: "Academic Year 2011/12");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("250bace6-aeb9-4fe9-8de2-3a25e0dc717f"),
                column: "Description",
                value: "Academic Year 2009/10");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("28e53936-5a52-44be-a7a6-d2f14a426d28"),
                column: "Description",
                value: "Academic Year 2014/15");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("45bd7f62-2018-4a5c-9b93-ccece8e89c46"),
                column: "Description",
                value: "Academic Year 2012/13");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("564dacdc-f58e-4aa0-8dbd-d8368b4fb6ba"),
                column: "Description",
                value: "Academic Year 2015/16");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("75991639-ad77-4ba6-91fc-ac08c00a4ce8"),
                column: "Description",
                value: "Academic Year 2013/14");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("78239816-507d-42b7-98fd-4a71d0d4eb1f"),
                column: "Description",
                value: "Academic Year 2014/15");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("81d91c86-9bf2-496c-b026-9dc255c35635"),
                column: "Description",
                value: "Academic Year 2010/11");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("89c02688-646d-45b5-8919-9a3fafcfe0e9"),
                column: "Description",
                value: "Academic Year 2009/10");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("a319e4ef-b957-40fb-8a47-b1a97814b220"),
                column: "Description",
                value: "Academic Year 2010/11");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("ce15f487-87b0-4c07-98f1-6c6732196be7"),
                column: "Description",
                value: "Academic Year 2012/13");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("e20141d0-d894-4b8d-a78f-e41c23500786"),
                column: "Description",
                value: "Academic Year 2011/12");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("f1225f98-40d5-494c-90f9-99f9fb59ac9d"),
                column: "Description",
                value: "Academic Year 2013/14");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("08134c1d-8a58-49a4-8d8b-22e586ffd5ae"),
                column: "Description",
                value: "2008 to 2009");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("13acf54a-8016-49ff-9050-c61ebe7acad2"),
                column: "Description",
                value: "2011 to 2012");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("250bace6-aeb9-4fe9-8de2-3a25e0dc717f"),
                column: "Description",
                value: "2009 to 2010");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("28e53936-5a52-44be-a7a6-d2f14a426d28"),
                column: "Description",
                value: "2014 to 2015");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("45bd7f62-2018-4a5c-9b93-ccece8e89c46"),
                column: "Description",
                value: "2012 to 2013");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("564dacdc-f58e-4aa0-8dbd-d8368b4fb6ba"),
                column: "Description",
                value: "2015 to 2016");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("75991639-ad77-4ba6-91fc-ac08c00a4ce8"),
                column: "Description",
                value: "2013 to 2014");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("78239816-507d-42b7-98fd-4a71d0d4eb1f"),
                column: "Description",
                value: "2014 to 2015");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("81d91c86-9bf2-496c-b026-9dc255c35635"),
                column: "Description",
                value: "2010 to 2011");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("89c02688-646d-45b5-8919-9a3fafcfe0e9"),
                column: "Description",
                value: "2009 to 2010");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("a319e4ef-b957-40fb-8a47-b1a97814b220"),
                column: "Description",
                value: "2010 to 2011");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("ce15f487-87b0-4c07-98f1-6c6732196be7"),
                column: "Description",
                value: "2012 to 2013");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("e20141d0-d894-4b8d-a78f-e41c23500786"),
                column: "Description",
                value: "2011 to 2012");

            migrationBuilder.UpdateData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("f1225f98-40d5-494c-90f9-99f9fb59ac9d"),
                column: "Description",
                value: "2013 to 2014");
        }
    }
}
