using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddBoundaryLevel : Migration
    {
        private const string _migrationsPath = "Migrations/";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoundaryLevel",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Level = table.Column<string>(nullable: false),
                    Label = table.Column<string>(nullable: true),
                    Published = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoundaryLevel", x => x.Id);
                });
            
            // Add new boundary level data
            ExecuteFile(migrationBuilder, _migrationsPath + "20190723150254_BoundaryLevelData.sql");
            
            // Add a new boundary level fk to the geometry table
            migrationBuilder.Sql("ALTER TABLE geometry ADD boundary_level_id bigint");
            migrationBuilder.Sql(
                "ALTER TABLE geometry ADD CONSTRAINT geometry_BoundaryLevel_Id_fk FOREIGN KEY (boundary_level_id) REFERENCES BoundaryLevel");

            // Update the geojson view to include the new boundary level fk field
            ExecuteFile(migrationBuilder, _migrationsPath + "20190723150254_UpdateGeoJsonView.sql");
            
            // Set the boundary level fk on existing geometry data
            ExecuteFile(migrationBuilder, _migrationsPath + "20190723150254_GeometryUpdates.sql");

            // Drop unused fields
            migrationBuilder.Sql("ALTER TABLE geometry DROP COLUMN geographicLevel");
            migrationBuilder.Sql("ALTER TABLE geometry DROP COLUMN year");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE geometry ADD geographiclevel nvarchar(max)");
            migrationBuilder.Sql("ALTER TABLE geometry ADD year int");

            // Revert the geometry data
            ExecuteFile(migrationBuilder, _migrationsPath + "20190723150254_GeometryUpdates_Revert.sql");
            
            // Revert to the previous version of the geojson view
            ExecuteFile(migrationBuilder, _migrationsPath + "20190524165101_AddGeoJsonView.sql");

            // Drop the boundary level fk
            migrationBuilder.Sql("ALTER TABLE geometry DROP CONSTRAINT geometry_BoundaryLevel_Id_fk");
            migrationBuilder.Sql("ALTER TABLE geometry DROP COLUMN boundary_level_id");
            
            migrationBuilder.DropTable(
                name: "BoundaryLevel");
        }
        
        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
