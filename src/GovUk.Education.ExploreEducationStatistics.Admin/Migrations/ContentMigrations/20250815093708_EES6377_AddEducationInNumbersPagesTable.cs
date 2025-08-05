using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class EES6377_AddEducationInNumbersPagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EducationInNumbersPages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2047)", maxLength: 2047, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Published = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationInNumbersPages", x => x.Id);
                });

            migrationBuilder.Sql("""
                                INSERT INTO [dbo].[EducationInNumbersPages] (
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
                                    [UpdatedById]
                                )
                                SELECT
                                    'C82315D6-C4D8-4433-9512-7C54049681CA', -- Id - hardcoded Id so can directly reference it
                                    'Education in numbers', -- Title
                                    NULL, -- Slug - NULL for main page
                                    'Description for Education in numbers page', -- Description @MarkFix update?
                                    0, -- Version
                                    0, -- Order
                                    NULL, -- Published
                                    GETUTCDATE(), -- Created
                                    'B99E8358-9A5E-4A3A-9288-6F94C7E1E3DD', -- CreatedById - Bau1
                                    NULL, -- Updated
                                    NULL -- UpdatedById
                                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EducationInNumbersPages");
        }
    }
}
