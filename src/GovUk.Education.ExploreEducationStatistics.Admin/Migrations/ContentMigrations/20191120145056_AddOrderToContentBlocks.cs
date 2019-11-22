using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AddOrderToContentBlocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "ContentBlock",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("13e4577a-2291-4ce4-a8c9-6c76baa06092"),
                column: "Order",
                value: 2);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("87f5343b-b7a5-4775-b483-d1668fac03fb"),
                column: "Order",
                value: 2);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                column: "Order",
                value: 2);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("8e10ad6c-9a68-4162-84f9-81fb6dc93ae3"),
                column: "Order",
                value: 2);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                column: "Order",
                value: 2);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("3913a0af-9455-4802-a037-c4cfd4719b18"),
                column: "Order",
                value: 3);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4aa06200-406b-4f5a-bee4-19e3b83eb1d2"),
                column: "Order",
                value: 2);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("33c3a82e-7d8d-47fc-9019-2fe5344ec32d"),
                column: "Order",
                value: 2);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                column: "Order",
                value: 2);

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("81d8eba2-9cba-4b04-bb02-e00ace5c4418"),
                column: "Order",
                value: 2);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                column: "Order",
                value: 3);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                column: "Order",
                value: 2);
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("e9462ed0-10dc-4ff5-8cda-f8c3b66f2714"),
                column: "Order",
                value: 3);
        }
    }
}
