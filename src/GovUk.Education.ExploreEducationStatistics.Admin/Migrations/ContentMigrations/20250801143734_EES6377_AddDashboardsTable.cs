using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES6377_AddDashboardsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dashboards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2047)", maxLength: 2047, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Published = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentDashboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dashboards", x => x.Id);
                });

            // Add Education in numbers parent dashboard
            migrationBuilder.Sql("""
                                 INSERT INTO [dbo].[Dashboards] (
                                     [Id],
                                     [Title],
                                     [Slug],
                                     [Description],
                                     [Version],
                                     [Order],
                                     [Published],
                                     [Created],
                                     [CreatedById],
                                     [Updated],
                                     [UpdatedById],
                                     [ParentDashboardId]
                                 )
                                 SELECT
                                     'C82315D6-C4D8-4433-9512-7C54049681CA', -- Id - hardcode Id so we have the option to directly reference EiN parent dashboard
                                     'Education in numbers', -- Title
                                     'education-in-numbers', -- Slug
                                     'Description for Education in numbers page', -- Description @MarkFix update?
                                     0, -- Version
                                     0, -- Order
                                     NULL, -- Published
                                     GETUTCDATE(), -- Created
                                     'B99E8358-9A5E-4A3A-9288-6F94C7E1E3DD', -- CreatedById - Bau1
                                     NULL, -- Updated
                                     NULL, -- UpdatedById
                                     NULL; -- ParentDashboardId
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dashboards");
        }
    }
}
