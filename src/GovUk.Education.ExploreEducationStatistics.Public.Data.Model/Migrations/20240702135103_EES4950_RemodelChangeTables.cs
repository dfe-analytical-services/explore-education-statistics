using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class EES4950_RemodelChangeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeSetFilterOptions");

            migrationBuilder.DropTable(
                name: "ChangeSetFilters");

            migrationBuilder.DropTable(
                name: "ChangeSetIndicators");

            migrationBuilder.DropTable(
                name: "ChangeSetLocations");

            migrationBuilder.DropTable(
                name: "ChangeSetTimePeriods");

            migrationBuilder.CreateTable(
                name: "FilterMetaChanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrentStateId = table.Column<int>(type: "integer", nullable: true),
                    PreviousStateId = table.Column<int>(type: "integer", nullable: true),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterMetaChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilterMetaChanges_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterMetaChanges_FilterMetas_CurrentStateId",
                        column: x => x.CurrentStateId,
                        principalTable: "FilterMetas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FilterMetaChanges_FilterMetas_PreviousStateId",
                        column: x => x.PreviousStateId,
                        principalTable: "FilterMetas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FilterOptionMetaChanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState_MetaId = table.Column<int>(type: "integer", nullable: true),
                    CurrentState_OptionId = table.Column<int>(type: "integer", nullable: true),
                    CurrentState_PublicId = table.Column<string>(type: "text", nullable: true),
                    PreviousState_MetaId = table.Column<int>(type: "integer", nullable: true),
                    PreviousState_OptionId = table.Column<int>(type: "integer", nullable: true),
                    PreviousState_PublicId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterOptionMetaChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilterOptionMetaChanges_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterOptionMetaChanges_FilterMetas_CurrentState_MetaId",
                        column: x => x.CurrentState_MetaId,
                        principalTable: "FilterMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterOptionMetaChanges_FilterMetas_PreviousState_MetaId",
                        column: x => x.PreviousState_MetaId,
                        principalTable: "FilterMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterOptionMetaChanges_FilterOptionMetas_CurrentState_Opti~",
                        column: x => x.CurrentState_OptionId,
                        principalTable: "FilterOptionMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterOptionMetaChanges_FilterOptionMetas_PreviousState_Opt~",
                        column: x => x.PreviousState_OptionId,
                        principalTable: "FilterOptionMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeographicLevelMetaChanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrentStateId = table.Column<int>(type: "integer", nullable: true),
                    PreviousStateId = table.Column<int>(type: "integer", nullable: true),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeographicLevelMetaChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeographicLevelMetaChanges_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeographicLevelMetaChanges_GeographicLevelMetas_CurrentStat~",
                        column: x => x.CurrentStateId,
                        principalTable: "GeographicLevelMetas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GeographicLevelMetaChanges_GeographicLevelMetas_PreviousSta~",
                        column: x => x.PreviousStateId,
                        principalTable: "GeographicLevelMetas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IndicatorMetaChanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrentStateId = table.Column<int>(type: "integer", nullable: true),
                    PreviousStateId = table.Column<int>(type: "integer", nullable: true),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorMetaChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicatorMetaChanges_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndicatorMetaChanges_IndicatorMetas_CurrentStateId",
                        column: x => x.CurrentStateId,
                        principalTable: "IndicatorMetas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IndicatorMetaChanges_IndicatorMetas_PreviousStateId",
                        column: x => x.PreviousStateId,
                        principalTable: "IndicatorMetas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocationMetaChanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrentStateId = table.Column<int>(type: "integer", nullable: true),
                    PreviousStateId = table.Column<int>(type: "integer", nullable: true),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationMetaChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationMetaChanges_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationMetaChanges_LocationMetas_CurrentStateId",
                        column: x => x.CurrentStateId,
                        principalTable: "LocationMetas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationMetaChanges_LocationMetas_PreviousStateId",
                        column: x => x.PreviousStateId,
                        principalTable: "LocationMetas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocationOptionMetaChanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState_MetaId = table.Column<int>(type: "integer", nullable: true),
                    CurrentState_OptionId = table.Column<int>(type: "integer", nullable: true),
                    PreviousState_MetaId = table.Column<int>(type: "integer", nullable: true),
                    PreviousState_OptionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOptionMetaChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationOptionMetaChanges_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationOptionMetaChanges_LocationMetas_CurrentState_MetaId",
                        column: x => x.CurrentState_MetaId,
                        principalTable: "LocationMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationOptionMetaChanges_LocationMetas_PreviousState_MetaId",
                        column: x => x.PreviousState_MetaId,
                        principalTable: "LocationMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationOptionMetaChanges_LocationOptionMetas_CurrentState_~",
                        column: x => x.CurrentState_OptionId,
                        principalTable: "LocationOptionMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationOptionMetaChanges_LocationOptionMetas_PreviousState~",
                        column: x => x.PreviousState_OptionId,
                        principalTable: "LocationOptionMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimePeriodMetaChanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrentStateId = table.Column<int>(type: "integer", nullable: true),
                    PreviousStateId = table.Column<int>(type: "integer", nullable: true),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimePeriodMetaChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimePeriodMetaChanges_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimePeriodMetaChanges_TimePeriodMetas_CurrentStateId",
                        column: x => x.CurrentStateId,
                        principalTable: "TimePeriodMetas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimePeriodMetaChanges_TimePeriodMetas_PreviousStateId",
                        column: x => x.PreviousStateId,
                        principalTable: "TimePeriodMetas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilterMetaChanges_CurrentStateId",
                table: "FilterMetaChanges",
                column: "CurrentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterMetaChanges_DataSetVersionId",
                table: "FilterMetaChanges",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterMetaChanges_PreviousStateId",
                table: "FilterMetaChanges",
                column: "PreviousStateId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterOptionMetaChanges_CurrentState_MetaId",
                table: "FilterOptionMetaChanges",
                column: "CurrentState_MetaId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterOptionMetaChanges_CurrentState_OptionId",
                table: "FilterOptionMetaChanges",
                column: "CurrentState_OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterOptionMetaChanges_DataSetVersionId",
                table: "FilterOptionMetaChanges",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterOptionMetaChanges_PreviousState_MetaId",
                table: "FilterOptionMetaChanges",
                column: "PreviousState_MetaId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterOptionMetaChanges_PreviousState_OptionId",
                table: "FilterOptionMetaChanges",
                column: "PreviousState_OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicLevelMetaChanges_CurrentStateId",
                table: "GeographicLevelMetaChanges",
                column: "CurrentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicLevelMetaChanges_DataSetVersionId",
                table: "GeographicLevelMetaChanges",
                column: "DataSetVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeographicLevelMetaChanges_PreviousStateId",
                table: "GeographicLevelMetaChanges",
                column: "PreviousStateId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorMetaChanges_CurrentStateId",
                table: "IndicatorMetaChanges",
                column: "CurrentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorMetaChanges_DataSetVersionId",
                table: "IndicatorMetaChanges",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorMetaChanges_PreviousStateId",
                table: "IndicatorMetaChanges",
                column: "PreviousStateId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationMetaChanges_CurrentStateId",
                table: "LocationMetaChanges",
                column: "CurrentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationMetaChanges_DataSetVersionId",
                table: "LocationMetaChanges",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationMetaChanges_PreviousStateId",
                table: "LocationMetaChanges",
                column: "PreviousStateId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetaChanges_CurrentState_MetaId",
                table: "LocationOptionMetaChanges",
                column: "CurrentState_MetaId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetaChanges_CurrentState_OptionId",
                table: "LocationOptionMetaChanges",
                column: "CurrentState_OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetaChanges_DataSetVersionId",
                table: "LocationOptionMetaChanges",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetaChanges_PreviousState_MetaId",
                table: "LocationOptionMetaChanges",
                column: "PreviousState_MetaId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetaChanges_PreviousState_OptionId",
                table: "LocationOptionMetaChanges",
                column: "PreviousState_OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TimePeriodMetaChanges_CurrentStateId",
                table: "TimePeriodMetaChanges",
                column: "CurrentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_TimePeriodMetaChanges_DataSetVersionId",
                table: "TimePeriodMetaChanges",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_TimePeriodMetaChanges_PreviousStateId",
                table: "TimePeriodMetaChanges",
                column: "PreviousStateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilterMetaChanges");

            migrationBuilder.DropTable(
                name: "FilterOptionMetaChanges");

            migrationBuilder.DropTable(
                name: "GeographicLevelMetaChanges");

            migrationBuilder.DropTable(
                name: "IndicatorMetaChanges");

            migrationBuilder.DropTable(
                name: "LocationMetaChanges");

            migrationBuilder.DropTable(
                name: "LocationOptionMetaChanges");

            migrationBuilder.DropTable(
                name: "TimePeriodMetaChanges");

            migrationBuilder.CreateTable(
                name: "ChangeSetFilterOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetFilterOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeSetFilterOptions_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeSetFilters_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetIndicators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeSetIndicators_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeSetLocations_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetTimePeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetTimePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeSetTimePeriods_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeSetFilterOptions_DataSetVersionId",
                table: "ChangeSetFilterOptions",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeSetFilters_DataSetVersionId",
                table: "ChangeSetFilters",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeSetIndicators_DataSetVersionId",
                table: "ChangeSetIndicators",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeSetLocations_DataSetVersionId",
                table: "ChangeSetLocations",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeSetTimePeriods_DataSetVersionId",
                table: "ChangeSetTimePeriods",
                column: "DataSetVersionId");
        }
    }
}
