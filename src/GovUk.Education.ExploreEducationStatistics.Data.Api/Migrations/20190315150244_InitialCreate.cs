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
                    Name = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: false),
                    KeyIndicator = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeMeta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacteristicMeta",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    Group = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacteristicMeta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Release",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ReleaseDate = table.Column<DateTime>(nullable: false),
                    PublicationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Release", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacteristicDataLa",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PublicationId = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<long>(nullable: false),
                    Term = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    Level = table.Column<string>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    SchoolType = table.Column<string>(nullable: false),
                    Attributes = table.Column<string>(nullable: true),
                    Characteristic = table.Column<string>(nullable: true),
                    CharacteristicName = table.Column<string>(nullable: true, computedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_1')"),
                    Region = table.Column<string>(nullable: true),
                    RegionCode = table.Column<string>(nullable: true, computedColumnSql: "JSON_VALUE(Region, '$.region_code')"),
                    LocalAuthority = table.Column<string>(nullable: true),
                    LocalAuthorityCode = table.Column<string>(nullable: true, computedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacteristicDataLa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacteristicDataLa_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacteristicDataNational",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PublicationId = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<long>(nullable: false),
                    Term = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    Level = table.Column<string>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    SchoolType = table.Column<string>(nullable: false),
                    Attributes = table.Column<string>(nullable: true),
                    Characteristic = table.Column<string>(nullable: true),
                    CharacteristicName = table.Column<string>(nullable: true, computedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_1')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacteristicDataNational", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacteristicDataNational_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeographicData",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PublicationId = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<long>(nullable: false),
                    Term = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    Level = table.Column<string>(nullable: false),
                    Country = table.Column<string>(nullable: true),
                    SchoolType = table.Column<string>(nullable: false),
                    Attributes = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true),
                    RegionCode = table.Column<string>(nullable: true, computedColumnSql: "JSON_VALUE(Region, '$.region_code')"),
                    LocalAuthority = table.Column<string>(nullable: true),
                    LocalAuthorityCode = table.Column<string>(nullable: true, computedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')"),
                    School = table.Column<string>(nullable: true),
                    SchoolLaEstab = table.Column<string>(nullable: true, computedColumnSql: "JSON_VALUE(School, '$.laestab')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeographicData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeographicData_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseAttributeMeta",
                columns: table => new
                {
                    ReleaseId = table.Column<long>(nullable: false),
                    AttributeMetaId = table.Column<long>(nullable: false),
                    DataType = table.Column<string>(nullable: false),
                    Group = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseAttributeMeta", x => new { x.ReleaseId, x.AttributeMetaId, x.DataType });
                    table.ForeignKey(
                        name: "FK_ReleaseAttributeMeta_AttributeMeta_AttributeMetaId",
                        column: x => x.AttributeMetaId,
                        principalTable: "AttributeMeta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseAttributeMeta_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseCharacteristicMeta",
                columns: table => new
                {
                    ReleaseId = table.Column<long>(nullable: false),
                    CharacteristicMetaId = table.Column<long>(nullable: false),
                    DataType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseCharacteristicMeta", x => new { x.ReleaseId, x.CharacteristicMetaId, x.DataType });
                    table.ForeignKey(
                        name: "FK_ReleaseCharacteristicMeta_CharacteristicMeta_CharacteristicMetaId",
                        column: x => x.CharacteristicMetaId,
                        principalTable: "CharacteristicMeta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseCharacteristicMeta_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_CharacteristicName",
                table: "CharacteristicDataLa",
                column: "CharacteristicName");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_Level",
                table: "CharacteristicDataLa",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_PublicationId",
                table: "CharacteristicDataLa",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_ReleaseId",
                table: "CharacteristicDataLa",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_SchoolType",
                table: "CharacteristicDataLa",
                column: "SchoolType");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_Year",
                table: "CharacteristicDataLa",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_CharacteristicName",
                table: "CharacteristicDataNational",
                column: "CharacteristicName");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_Level",
                table: "CharacteristicDataNational",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_PublicationId",
                table: "CharacteristicDataNational",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_ReleaseId",
                table: "CharacteristicDataNational",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_SchoolType",
                table: "CharacteristicDataNational",
                column: "SchoolType");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_Year",
                table: "CharacteristicDataNational",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_Level",
                table: "GeographicData",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_PublicationId",
                table: "GeographicData",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_ReleaseId",
                table: "GeographicData",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_SchoolType",
                table: "GeographicData",
                column: "SchoolType");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_Year",
                table: "GeographicData",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_Release_PublicationId",
                table: "Release",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseAttributeMeta_AttributeMetaId",
                table: "ReleaseAttributeMeta",
                column: "AttributeMetaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseCharacteristicMeta_CharacteristicMetaId",
                table: "ReleaseCharacteristicMeta",
                column: "CharacteristicMetaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacteristicDataLa");

            migrationBuilder.DropTable(
                name: "CharacteristicDataNational");

            migrationBuilder.DropTable(
                name: "GeographicData");

            migrationBuilder.DropTable(
                name: "ReleaseAttributeMeta");

            migrationBuilder.DropTable(
                name: "ReleaseCharacteristicMeta");

            migrationBuilder.DropTable(
                name: "AttributeMeta");

            migrationBuilder.DropTable(
                name: "CharacteristicMeta");

            migrationBuilder.DropTable(
                name: "Release");
        }
    }
}
