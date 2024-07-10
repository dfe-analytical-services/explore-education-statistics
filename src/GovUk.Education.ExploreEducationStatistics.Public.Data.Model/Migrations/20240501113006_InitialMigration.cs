using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilterOptionMetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Label = table.Column<string>(type: "text", nullable: false),
                    IsAggregate = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterOptionMetas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationOptionMetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<string>(type: "text", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    OldCode = table.Column<string>(type: "text", nullable: true),
                    Urn = table.Column<string>(type: "text", nullable: true),
                    LaEstab = table.Column<string>(type: "text", nullable: true),
                    Ukprn = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOptionMetas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetFilterOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetFilterOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetFilters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetFilters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetIndicators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetIndicators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeSetTimePeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeSetTimePeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    PublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SupersedingDataSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    LatestDraftVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    LatestLiveVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Published = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Withdrawn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSets_DataSets_SupersedingDataSetId",
                        column: x => x.SupersedingDataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DataSetVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReleaseFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionMajor = table.Column<int>(type: "integer", nullable: false),
                    VersionMinor = table.Column<int>(type: "integer", nullable: false),
                    VersionPatch = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    TotalResults = table.Column<long>(type: "bigint", nullable: false),
                    MetaSummary = table.Column<string>(type: "jsonb", nullable: true),
                    Published = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Withdrawn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetVersions_DataSets_DataSetId",
                        column: x => x.DataSetId,
                        principalTable: "DataSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataSetVersionImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    Completed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSetVersionImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSetVersionImports_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilterMetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Hint = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilterMetas_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeographicLevelMetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Levels = table.Column<List<string>>(type: "text[]", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeographicLevelMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeographicLevelMetas_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndicatorMetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: true),
                    DecimalPlaces = table.Column<byte>(type: "smallint", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicatorMetas_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationMetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationMetas_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimePeriodMetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Period = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimePeriodMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimePeriodMetas_DataSetVersions_DataSetVersionId",
                        column: x => x.DataSetVersionId,
                        principalTable: "DataSetVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilterOptionMetaLinks",
                columns: table => new
                {
                    MetaId = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterOptionMetaLinks", x => new { x.MetaId, x.OptionId });
                    table.ForeignKey(
                        name: "FK_FilterOptionMetaLinks_FilterMetas_MetaId",
                        column: x => x.MetaId,
                        principalTable: "FilterMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterOptionMetaLinks_FilterOptionMetas_OptionId",
                        column: x => x.OptionId,
                        principalTable: "FilterOptionMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationOptionMetaLinks",
                columns: table => new
                {
                    MetaId = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOptionMetaLinks", x => new { x.MetaId, x.OptionId });
                    table.ForeignKey(
                        name: "FK_LocationOptionMetaLinks_LocationMetas_MetaId",
                        column: x => x.MetaId,
                        principalTable: "LocationMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationOptionMetaLinks_LocationOptionMetas_OptionId",
                        column: x => x.OptionId,
                        principalTable: "LocationOptionMetas",
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

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_LatestDraftVersionId",
                table: "DataSets",
                column: "LatestDraftVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_LatestLiveVersionId",
                table: "DataSets",
                column: "LatestLiveVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataSets_SupersedingDataSetId",
                table: "DataSets",
                column: "SupersedingDataSetId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetVersionImports_DataSetVersionId",
                table: "DataSetVersionImports",
                column: "DataSetVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSetVersionImports_InstanceId",
                table: "DataSetVersionImports",
                column: "InstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataSetVersions_DataSetId_VersionNumber",
                table: "DataSetVersions",
                columns: new[] { "DataSetId", "VersionMajor", "VersionMinor", "VersionPatch" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataSetVersions_ReleaseFileId",
                table: "DataSetVersions",
                column: "ReleaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterMetas_DataSetVersionId_PublicId",
                table: "FilterMetas",
                columns: new[] { "DataSetVersionId", "PublicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FilterOptionMetaLinks_MetaId_PublicId",
                table: "FilterOptionMetaLinks",
                columns: new[] { "MetaId", "PublicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FilterOptionMetaLinks_OptionId",
                table: "FilterOptionMetaLinks",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterOptionMetaLinks_PublicId",
                table: "FilterOptionMetaLinks",
                column: "PublicId");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicLevelMetas_DataSetVersionId",
                table: "GeographicLevelMetas",
                column: "DataSetVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorMetas_DataSetVersionId_PublicId",
                table: "IndicatorMetas",
                columns: new[] { "DataSetVersionId", "PublicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationMetas_DataSetVersionId_Level",
                table: "LocationMetas",
                columns: new[] { "DataSetVersionId", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetaLinks_OptionId",
                table: "LocationOptionMetaLinks",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetas_All",
                table: "LocationOptionMetas",
                columns: new[] { "Type", "Label", "Code", "OldCode", "Urn", "LaEstab", "Ukprn" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetas_Code",
                table: "LocationOptionMetas",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetas_LaEstab",
                table: "LocationOptionMetas",
                column: "LaEstab");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetas_OldCode",
                table: "LocationOptionMetas",
                column: "OldCode");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetas_PublicId",
                table: "LocationOptionMetas",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetas_Type",
                table: "LocationOptionMetas",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetas_Ukprn",
                table: "LocationOptionMetas",
                column: "Ukprn");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOptionMetas_Urn",
                table: "LocationOptionMetas",
                column: "Urn");

            migrationBuilder.CreateIndex(
                name: "IX_TimePeriodMetas_DataSetVersionId_Code_Period",
                table: "TimePeriodMetas",
                columns: new[] { "DataSetVersionId", "Code", "Period" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeSetFilterOptions_DataSetVersions_DataSetVersionId",
                table: "ChangeSetFilterOptions",
                column: "DataSetVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeSetFilters_DataSetVersions_DataSetVersionId",
                table: "ChangeSetFilters",
                column: "DataSetVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeSetIndicators_DataSetVersions_DataSetVersionId",
                table: "ChangeSetIndicators",
                column: "DataSetVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeSetLocations_DataSetVersions_DataSetVersionId",
                table: "ChangeSetLocations",
                column: "DataSetVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeSetTimePeriods_DataSetVersions_DataSetVersionId",
                table: "ChangeSetTimePeriods",
                column: "DataSetVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataSets_DataSetVersions_LatestDraftVersionId",
                table: "DataSets",
                column: "LatestDraftVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSets_DataSetVersions_LatestLiveVersionId",
                table: "DataSets",
                column: "LatestLiveVersionId",
                principalTable: "DataSetVersions",
                principalColumn: "Id");

            var isLocalEnvironment = 
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;

            // Grant privileges on objects created by this resource's database user to the Admin App Service user.
            var adminAppServiceRoleName = isLocalEnvironment
                ? "app_admin" 
                : Environment.GetEnvironmentVariable("AdminAppServiceIdentityName");
            if (adminAppServiceRoleName != null)
            {
                migrationBuilder.Sql(
                    $"""GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO {adminAppServiceRoleName}""");
                migrationBuilder.Sql(
                    $"""ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {adminAppServiceRoleName}""");
            }

            // Grant privileges on objects created by this resource's database user to the Public API Data Processor Function App user.
            var dataProcessorFunctionAppRoleName = isLocalEnvironment
                ? "app_public_data_processor" 
                : Environment.GetEnvironmentVariable("DataProcessorFunctionAppIdentityName");
            if (dataProcessorFunctionAppRoleName != null)
            {
                migrationBuilder.Sql(
                    $"""GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO {dataProcessorFunctionAppRoleName}""");
                migrationBuilder.Sql(
                    $"""ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {dataProcessorFunctionAppRoleName}""");

                migrationBuilder.Sql(
                    $"""GRANT SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA public TO {dataProcessorFunctionAppRoleName}""");
                migrationBuilder.Sql(
                    $"""ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, UPDATE ON SEQUENCES TO {dataProcessorFunctionAppRoleName}""");
            }

            // Grant privileges on objects created by this resource's database user to the Publisher Function App user.
            var publisherFunctionAppRoleName = isLocalEnvironment
                ? "app_publisher" 
                : Environment.GetEnvironmentVariable("PublisherFunctionAppIdentityName");
            if (publisherFunctionAppRoleName != null)
            {
                migrationBuilder.Sql(
                    $"""GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO {publisherFunctionAppRoleName}""");
                migrationBuilder.Sql(
                    $"""ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {publisherFunctionAppRoleName}""");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataSets_DataSetVersions_LatestDraftVersionId",
                table: "DataSets");

            migrationBuilder.DropForeignKey(
                name: "FK_DataSets_DataSetVersions_LatestLiveVersionId",
                table: "DataSets");

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

            migrationBuilder.DropTable(
                name: "DataSetVersionImports");

            migrationBuilder.DropTable(
                name: "FilterOptionMetaLinks");

            migrationBuilder.DropTable(
                name: "GeographicLevelMetas");

            migrationBuilder.DropTable(
                name: "IndicatorMetas");

            migrationBuilder.DropTable(
                name: "LocationOptionMetaLinks");

            migrationBuilder.DropTable(
                name: "TimePeriodMetas");

            migrationBuilder.DropTable(
                name: "FilterMetas");

            migrationBuilder.DropTable(
                name: "FilterOptionMetas");

            migrationBuilder.DropTable(
                name: "LocationMetas");

            migrationBuilder.DropTable(
                name: "LocationOptionMetas");

            migrationBuilder.DropTable(
                name: "DataSetVersions");

            migrationBuilder.DropTable(
                name: "DataSets");
        }
    }
}
