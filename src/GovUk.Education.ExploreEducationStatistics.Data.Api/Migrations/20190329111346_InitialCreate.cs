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
                name: "Level",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Level = table.Column<string>(nullable: false),
                    Country_Code = table.Column<string>(nullable: true),
                    Country_Name = table.Column<string>(nullable: true),
                    Region_Code = table.Column<string>(nullable: true),
                    Region_Name = table.Column<string>(nullable: true),
                    LocalAuthority_Code = table.Column<string>(nullable: true),
                    LocalAuthority_Old_Code = table.Column<string>(nullable: true),
                    LocalAuthority_Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Level", x => x.Id);
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
                name: "Subject",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    ReleaseId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subject_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacteristicData",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubjectId = table.Column<long>(nullable: false),
                    LevelId = table.Column<long>(nullable: false),
                    TimePeriod = table.Column<int>(nullable: false),
                    TimeIdentifier = table.Column<string>(nullable: true),
                    SchoolType = table.Column<string>(nullable: false),
                    Indicators = table.Column<string>(nullable: true),
                    Characteristic = table.Column<string>(nullable: true),
                    CharacteristicName = table.Column<string>(nullable: true, computedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_breakdown')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacteristicData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacteristicData_Level_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Level",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacteristicData_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacteristicMeta",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    Group = table.Column<string>(nullable: true),
                    SubjectId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacteristicMeta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacteristicMeta_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeographicData",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubjectId = table.Column<long>(nullable: false),
                    LevelId = table.Column<long>(nullable: false),
                    TimePeriod = table.Column<int>(nullable: false),
                    TimeIdentifier = table.Column<string>(nullable: true),
                    SchoolType = table.Column<string>(nullable: false),
                    Indicators = table.Column<string>(nullable: true),
                    School = table.Column<string>(nullable: true),
                    SchoolLaEstab = table.Column<string>(nullable: true, computedColumnSql: "JSON_VALUE(School, '$.laestab')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeographicData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeographicData_Level_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Level",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeographicData_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndicatorMeta",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    Group = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: false),
                    KeyIndicator = table.Column<bool>(nullable: false),
                    SubjectId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorMeta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicatorMeta_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicData_CharacteristicName",
                table: "CharacteristicData",
                column: "CharacteristicName");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicData_LevelId",
                table: "CharacteristicData",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicData_SchoolType",
                table: "CharacteristicData",
                column: "SchoolType");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicData_SubjectId",
                table: "CharacteristicData",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicData_TimePeriod",
                table: "CharacteristicData",
                column: "TimePeriod");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicMeta_SubjectId",
                table: "CharacteristicMeta",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_LevelId",
                table: "GeographicData",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_SchoolLaEstab",
                table: "GeographicData",
                column: "SchoolLaEstab");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_SchoolType",
                table: "GeographicData",
                column: "SchoolType");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_SubjectId",
                table: "GeographicData",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_TimePeriod",
                table: "GeographicData",
                column: "TimePeriod");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorMeta_SubjectId",
                table: "IndicatorMeta",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Level_Country_Code",
                table: "Level",
                column: "Country_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Level_Level",
                table: "Level",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Level_LocalAuthority_Code",
                table: "Level",
                column: "LocalAuthority_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Level_Region_Code",
                table: "Level",
                column: "Region_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Release_PublicationId",
                table: "Release",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Subject_ReleaseId",
                table: "Subject",
                column: "ReleaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacteristicData");

            migrationBuilder.DropTable(
                name: "CharacteristicMeta");

            migrationBuilder.DropTable(
                name: "GeographicData");

            migrationBuilder.DropTable(
                name: "IndicatorMeta");

            migrationBuilder.DropTable(
                name: "Level");

            migrationBuilder.DropTable(
                name: "Subject");

            migrationBuilder.DropTable(
                name: "Release");
        }
    }
}
