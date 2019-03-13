using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {            
            migrationBuilder.CreateTable(
                name: "AttributeMeta",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PublicationId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    Unit = table.Column<int>(nullable: false),
                    Group = table.Column<string>(nullable: true),
                    KeyIndicator = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeMeta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacteristicDataLa",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PublicationId = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<int>(nullable: false),
                    ReleaseDate = table.Column<DateTime>(nullable: false),
                    Term = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    Level = table.Column<string>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    SchoolType = table.Column<string>(nullable: false),
                    Attributes = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true),
                    LocalAuthority = table.Column<string>(nullable: true),
                    Characteristic = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacteristicDataLa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacteristicDataNational",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PublicationId = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<int>(nullable: false),
                    ReleaseDate = table.Column<DateTime>(nullable: false),
                    Term = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    Level = table.Column<string>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    SchoolType = table.Column<string>(nullable: false),
                    Attributes = table.Column<string>(nullable: true),
                    Characteristic = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacteristicDataNational", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacteristicMeta",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PublicationId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    Group = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacteristicMeta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeographicData",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PublicationId = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<int>(nullable: false),
                    ReleaseDate = table.Column<DateTime>(nullable: false),
                    Term = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    Level = table.Column<string>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    SchoolType = table.Column<string>(nullable: false),
                    Attributes = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true),
                    LocalAuthority = table.Column<string>(nullable: true),
                    School = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeographicData", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttributeMeta");

            migrationBuilder.DropTable(
                name: "CharacteristicDataLa");

            migrationBuilder.DropTable(
                name: "CharacteristicDataNational");

            migrationBuilder.DropTable(
                name: "CharacteristicMeta");

            migrationBuilder.DropTable(
                name: "GeographicData");
        }
    }
}
