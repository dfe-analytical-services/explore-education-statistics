using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class Renaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseAttributeMeta");

            migrationBuilder.DropTable(
                name: "AttributeMeta");

            migrationBuilder.DropColumn(
                name: "Attributes",
                table: "GeographicData");

            migrationBuilder.DropColumn(
                name: "Attributes",
                table: "CharacteristicDataNational");

            migrationBuilder.DropColumn(
                name: "Attributes",
                table: "CharacteristicDataLa");

            migrationBuilder.RenameColumn(
                name: "Year",
                table: "GeographicData",
                newName: "TimePeriod");

            migrationBuilder.RenameColumn(
                name: "Term",
                table: "GeographicData",
                newName: "TimeIdentifier");

            migrationBuilder.RenameIndex(
                name: "IX_GeographicData_Year",
                table: "GeographicData",
                newName: "IX_GeographicData_TimePeriod");

            migrationBuilder.RenameColumn(
                name: "Year",
                table: "CharacteristicDataNational",
                newName: "TimePeriod");

            migrationBuilder.RenameColumn(
                name: "Term",
                table: "CharacteristicDataNational",
                newName: "TimeIdentifier");

            migrationBuilder.RenameIndex(
                name: "IX_CharacteristicDataNational_Year",
                table: "CharacteristicDataNational",
                newName: "IX_CharacteristicDataNational_TimePeriod");

            migrationBuilder.RenameColumn(
                name: "Year",
                table: "CharacteristicDataLa",
                newName: "TimePeriod");

            migrationBuilder.RenameColumn(
                name: "Term",
                table: "CharacteristicDataLa",
                newName: "TimeIdentifier");

            migrationBuilder.RenameIndex(
                name: "IX_CharacteristicDataLa_Year",
                table: "CharacteristicDataLa",
                newName: "IX_CharacteristicDataLa_TimePeriod");

            migrationBuilder.AddColumn<string>(
                name: "Indicators",
                table: "GeographicData",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Indicators",
                table: "CharacteristicDataNational",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Indicators",
                table: "CharacteristicDataLa",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IndicatorMeta",
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
                    table.PrimaryKey("PK_IndicatorMeta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseIndicatorMeta",
                columns: table => new
                {
                    ReleaseId = table.Column<long>(nullable: false),
                    IndicatorMetaId = table.Column<long>(nullable: false),
                    DataType = table.Column<string>(nullable: false),
                    Group = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseIndicatorMeta", x => new { x.ReleaseId, x.IndicatorMetaId, x.DataType });
                    table.ForeignKey(
                        name: "FK_ReleaseIndicatorMeta_IndicatorMeta_IndicatorMetaId",
                        column: x => x.IndicatorMetaId,
                        principalTable: "IndicatorMeta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseIndicatorMeta_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseIndicatorMeta_IndicatorMetaId",
                table: "ReleaseIndicatorMeta",
                column: "IndicatorMetaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseIndicatorMeta");

            migrationBuilder.DropTable(
                name: "IndicatorMeta");

            migrationBuilder.DropColumn(
                name: "Indicators",
                table: "GeographicData");

            migrationBuilder.DropColumn(
                name: "Indicators",
                table: "CharacteristicDataNational");

            migrationBuilder.DropColumn(
                name: "Indicators",
                table: "CharacteristicDataLa");

            migrationBuilder.RenameColumn(
                name: "TimePeriod",
                table: "GeographicData",
                newName: "Year");

            migrationBuilder.RenameColumn(
                name: "TimeIdentifier",
                table: "GeographicData",
                newName: "Term");

            migrationBuilder.RenameIndex(
                name: "IX_GeographicData_TimePeriod",
                table: "GeographicData",
                newName: "IX_GeographicData_Year");

            migrationBuilder.RenameColumn(
                name: "TimePeriod",
                table: "CharacteristicDataNational",
                newName: "Year");

            migrationBuilder.RenameColumn(
                name: "TimeIdentifier",
                table: "CharacteristicDataNational",
                newName: "Term");

            migrationBuilder.RenameIndex(
                name: "IX_CharacteristicDataNational_TimePeriod",
                table: "CharacteristicDataNational",
                newName: "IX_CharacteristicDataNational_Year");

            migrationBuilder.RenameColumn(
                name: "TimePeriod",
                table: "CharacteristicDataLa",
                newName: "Year");

            migrationBuilder.RenameColumn(
                name: "TimeIdentifier",
                table: "CharacteristicDataLa",
                newName: "Term");

            migrationBuilder.RenameIndex(
                name: "IX_CharacteristicDataLa_TimePeriod",
                table: "CharacteristicDataLa",
                newName: "IX_CharacteristicDataLa_Year");

            migrationBuilder.AddColumn<string>(
                name: "Attributes",
                table: "GeographicData",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attributes",
                table: "CharacteristicDataNational",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attributes",
                table: "CharacteristicDataLa",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttributeMeta",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    KeyIndicator = table.Column<bool>(nullable: false),
                    Label = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeMeta", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseAttributeMeta_AttributeMetaId",
                table: "ReleaseAttributeMeta",
                column: "AttributeMetaId");
        }
    }
}
