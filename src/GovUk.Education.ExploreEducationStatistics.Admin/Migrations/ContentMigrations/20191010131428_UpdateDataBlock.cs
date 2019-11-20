using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    [ExcludeFromCodeCoverage]
    public partial class UpdateDataBlock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomFootnotes",
                table: "ContentBlock",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ContentBlock",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "ContentBlock",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"177\",\"180\",\"181\"],\"tableHeaders\":null}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"179\",\"181\",\"178\"],\"tableHeaders\":null}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"212\",\"211\",\"216\",\"217\",\"218\",\"221\",\"222\"],\"tableHeaders\":null}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"220\",\"221\",\"222\"],\"tableHeaders\":null}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"23\",\"26\",\"28\"],\"tableHeaders\":null}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"23\",\"26\",\"28\"],\"tableHeaders\":null}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"220\",\"221\",\"222\"],\"tableHeaders\":null}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"177\",\"178\",\"179\"],\"tableHeaders\":null}]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomFootnotes",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "ContentBlock");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"177\",\"180\",\"181\"]}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"179\",\"181\",\"178\"]}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"212\",\"211\",\"216\",\"217\",\"218\",\"221\",\"222\"]}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"220\",\"221\",\"222\"]}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"23\",\"26\",\"28\"]}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"23\",\"26\",\"28\"]}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"220\",\"221\",\"222\"]}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                column: "DataBlock_Tables",
                value: "[{\"indicators\":[\"177\",\"178\",\"179\"]}]");
        }
    }
}
