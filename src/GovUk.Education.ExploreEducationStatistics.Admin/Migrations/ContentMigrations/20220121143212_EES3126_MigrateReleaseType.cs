using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    [ExcludeFromCodeCoverage]
    public partial class EES3126_MigrateReleaseType : Migration
    {
        private static readonly Dictionary<ReleaseType, Guid> ReleaseTypeIds = new Dictionary<ReleaseType, Guid>
        {
            {ReleaseType.AdHocStatistics, new Guid("1821abb8-68b0-431b-9770-0bea65d02ff0")},
            {ReleaseType.ExperimentalStatistics, new Guid("f5de8522-3150-435d-98d5-1d14763f8c54")},
            {ReleaseType.ManagementInformation, new Guid("15bd4f57-c837-4821-b308-7f4169cd9330")},
            {ReleaseType.NationalStatistics, new Guid("8becd272-1100-4e33-8a7d-1c0c4e3b42b8")},
            {ReleaseType.OfficialStatistics, new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c")}
        };

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the new Type column and allow it to be nullable
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Releases",
                nullable: true);

            ReleaseTypeIds.ForEach(pair =>
            {
                var (type, id) = pair;
                migrationBuilder.Sql($"UPDATE dbo.Releases SET Type='{type}' WHERE TypeId='{id}'");
            });

            // Now that every row should have a value, make the column not nullable
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Releases",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Releases_Type",
                table: "Releases",
                column: "Type");

            // Drop the TypeId column
            migrationBuilder.DropForeignKey(
                name: "FK_Releases_ReleaseTypes_TypeId",
                table: "Releases");

            migrationBuilder.DropTable(
                name: "ReleaseTypes");

            migrationBuilder.DropIndex(
                name: "IX_Releases_TypeId",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Releases");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the TypeId column and allow it to be nullable
            migrationBuilder.AddColumn<Guid>(
                name: "TypeId",
                table: "Releases",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReleaseTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_ReleaseTypes", x => x.Id); });

            migrationBuilder.InsertData(
                table: "ReleaseTypes",
                columns: new[] {"Id", "Title"},
                values: new object[,]
                {
                    {ReleaseTypeIds[ReleaseType.AdHocStatistics], "Ad Hoc Statistics"},
                    {ReleaseTypeIds[ReleaseType.ExperimentalStatistics], "Experimental Statistics"},
                    {ReleaseTypeIds[ReleaseType.ManagementInformation], "Management Information"},
                    {ReleaseTypeIds[ReleaseType.NationalStatistics], "National Statistics"},
                    {ReleaseTypeIds[ReleaseType.OfficialStatistics], "Official Statistics"}
                });

            ReleaseTypeIds.ForEach(pair =>
            {
                var (type, id) = pair;
                migrationBuilder.Sql($"UPDATE dbo.Releases SET TypeId='{id}' WHERE Type='{type}'");
            });

            // Now that every row should have a value, make the column not nullable
            migrationBuilder.AlterColumn<string>(
                name: "TypeId",
                table: "Releases",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Releases_TypeId",
                table: "Releases",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Releases_ReleaseTypes_TypeId",
                table: "Releases",
                column: "TypeId",
                principalTable: "ReleaseTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Drop the Type column
            migrationBuilder.DropIndex(
                name: "IX_Releases_Type",
                table: "Releases");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Releases");
        }
    }
}
