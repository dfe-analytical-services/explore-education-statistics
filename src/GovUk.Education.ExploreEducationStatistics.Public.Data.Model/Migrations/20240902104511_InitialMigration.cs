using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder.IsEnvironment(Environments.Production))
        {
            // Grant privileges to the 'public_data_read_write' group role for objects in the public schema
            // subsequently created by this applications user role. Membership of the role will be granted to other
            // application and indvidual user roles who require read and write privileges on public schema objects.

            // In Azure `public_data_read_write` should be created manually before the initial migration is run using:
            // `CREATE ROLE public_data_read_write WITH NOLOGIN`.

            // Local development environments skip this since the Docker entrypoint script handles role creation
            // and privilege grants.

            // A check ensures the role exists for cases where the migration is applied outside an app deployment,
            // e.g. with the EF Core CLI command `dotnet ef database update command`, when no environment variable
            // is set. This also applies to running integration tests which identify as a Production environment
            // if the enviroment is not set. Integration tests connect as the Postgres superuser without running
            // the Docker script.

            // A separate grant is needed for the __EFMigrationsHistory table, as it was created prior to this migration.
            migrationBuilder.Sql(
                $"""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '{PublicDataDbContext.PublicDataReadWriteRole}') THEN
                        GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES ON "__EFMigrationsHistory" TO {PublicDataDbContext.PublicDataReadWriteRole};
                        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES ON TABLES TO {PublicDataDbContext.PublicDataReadWriteRole};
                        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, UPDATE ON SEQUENCES TO {PublicDataDbContext.PublicDataReadWriteRole};
                    END IF;
                END $$;
                """
            );
        }

        migrationBuilder.CreateSequence<int>(name: "FilterOptionMetaLink_seq");

        migrationBuilder.CreateTable(
            name: "FilterOptionMetas",
            columns: table => new
            {
                Id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                IsAggregate = table.Column<bool>(type: "boolean", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FilterOptionMetas", x => x.Id);
            }
        );

        migrationBuilder.CreateTable(
            name: "LocationOptionMetas",
            columns: table => new
            {
                Id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                OldCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                Urn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                LaEstab = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                Ukprn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LocationOptionMetas", x => x.Id);
            }
        );

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
                Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataSets", x => x.Id);
                table.ForeignKey(
                    name: "FK_DataSets_DataSets_SupersedingDataSetId",
                    column: x => x.SupersedingDataSetId,
                    principalTable: "DataSets",
                    principalColumn: "Id"
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "DataSetVersions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DataSetId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                VersionMajor = table.Column<int>(type: "integer", nullable: false),
                VersionMinor = table.Column<int>(type: "integer", nullable: false),
                VersionPatch = table.Column<int>(type: "integer", nullable: false),
                Release_Title = table.Column<string>(type: "text", nullable: false),
                Release_Slug = table.Column<string>(type: "text", nullable: false),
                Release_DataSetFileId = table.Column<Guid>(type: "uuid", nullable: false),
                Release_ReleaseFileId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

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
                Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataSetVersionImports", x => x.Id);
                table.ForeignKey(
                    name: "FK_DataSetVersionImports_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "DataSetVersionMappings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SourceDataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                TargetDataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                LocationMappingPlan = table.Column<string>(type: "jsonb", nullable: false),
                FilterMappingPlan = table.Column<string>(type: "jsonb", nullable: false),
                LocationMappingsComplete = table.Column<bool>(type: "boolean", nullable: false),
                FilterMappingsComplete = table.Column<bool>(type: "boolean", nullable: false),
                Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataSetVersionMappings", x => x.Id);
                table.ForeignKey(
                    name: "FK_DataSetVersionMappings_DataSetVersions_SourceDataSetVersion~",
                    column: x => x.SourceDataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_DataSetVersionMappings_DataSetVersions_TargetDataSetVersion~",
                    column: x => x.TargetDataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "FilterMetas",
            columns: table => new
            {
                Id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                PublicId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                Column = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Hint = table.Column<string>(type: "text", nullable: false),
                Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FilterMetas", x => x.Id);
                table.ForeignKey(
                    name: "FK_FilterMetas_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "GeographicLevelMetas",
            columns: table => new
            {
                Id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                Levels = table.Column<List<string>>(type: "text[]", nullable: false),
                Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GeographicLevelMetas", x => x.Id);
                table.ForeignKey(
                    name: "FK_GeographicLevelMetas_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "IndicatorMetas",
            columns: table => new
            {
                Id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                PublicId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                Column = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Unit = table.Column<string>(type: "text", nullable: true),
                DecimalPlaces = table.Column<byte>(type: "smallint", nullable: true),
                Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IndicatorMetas", x => x.Id);
                table.ForeignKey(
                    name: "FK_IndicatorMetas_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "LocationMetas",
            columns: table => new
            {
                Id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                Level = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LocationMetas", x => x.Id);
                table.ForeignKey(
                    name: "FK_LocationMetas_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "PreviewTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Expiry = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PreviewTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_PreviewTokens_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "TimePeriodMetas",
            columns: table => new
            {
                Id = table
                    .Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                Period = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TimePeriodMetas", x => x.Id);
                table.ForeignKey(
                    name: "FK_TimePeriodMetas_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "FilterMetaChanges",
            columns: table => new
            {
                Id = table
                    .Column<long>(type: "bigint", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                CurrentStateId = table.Column<int>(type: "integer", nullable: true),
                PreviousStateId = table.Column<int>(type: "integer", nullable: true),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FilterMetaChanges", x => x.Id);
                table.ForeignKey(
                    name: "FK_FilterMetaChanges_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_FilterMetaChanges_FilterMetas_CurrentStateId",
                    column: x => x.CurrentStateId,
                    principalTable: "FilterMetas",
                    principalColumn: "Id"
                );
                table.ForeignKey(
                    name: "FK_FilterMetaChanges_FilterMetas_PreviousStateId",
                    column: x => x.PreviousStateId,
                    principalTable: "FilterMetas",
                    principalColumn: "Id"
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "FilterOptionMetaChanges",
            columns: table => new
            {
                Id = table
                    .Column<long>(type: "bigint", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                CurrentState_MetaId = table.Column<int>(type: "integer", nullable: true),
                CurrentState_OptionId = table.Column<int>(type: "integer", nullable: true),
                CurrentState_PublicId = table.Column<string>(type: "text", nullable: true),
                PreviousState_MetaId = table.Column<int>(type: "integer", nullable: true),
                PreviousState_OptionId = table.Column<int>(type: "integer", nullable: true),
                PreviousState_PublicId = table.Column<string>(type: "text", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FilterOptionMetaChanges", x => x.Id);
                table.ForeignKey(
                    name: "FK_FilterOptionMetaChanges_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_FilterOptionMetaChanges_FilterMetas_CurrentState_MetaId",
                    column: x => x.CurrentState_MetaId,
                    principalTable: "FilterMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_FilterOptionMetaChanges_FilterMetas_PreviousState_MetaId",
                    column: x => x.PreviousState_MetaId,
                    principalTable: "FilterMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_FilterOptionMetaChanges_FilterOptionMetas_CurrentState_Opti~",
                    column: x => x.CurrentState_OptionId,
                    principalTable: "FilterOptionMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_FilterOptionMetaChanges_FilterOptionMetas_PreviousState_Opt~",
                    column: x => x.PreviousState_OptionId,
                    principalTable: "FilterOptionMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "FilterOptionMetaLinks",
            columns: table => new
            {
                MetaId = table.Column<int>(type: "integer", nullable: false),
                OptionId = table.Column<int>(type: "integer", nullable: false),
                PublicId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FilterOptionMetaLinks", x => new { x.MetaId, x.OptionId });
                table.ForeignKey(
                    name: "FK_FilterOptionMetaLinks_FilterMetas_MetaId",
                    column: x => x.MetaId,
                    principalTable: "FilterMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_FilterOptionMetaLinks_FilterOptionMetas_OptionId",
                    column: x => x.OptionId,
                    principalTable: "FilterOptionMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "GeographicLevelMetaChanges",
            columns: table => new
            {
                Id = table
                    .Column<long>(type: "bigint", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                CurrentStateId = table.Column<int>(type: "integer", nullable: true),
                PreviousStateId = table.Column<int>(type: "integer", nullable: true),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GeographicLevelMetaChanges", x => x.Id);
                table.ForeignKey(
                    name: "FK_GeographicLevelMetaChanges_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_GeographicLevelMetaChanges_GeographicLevelMetas_CurrentStat~",
                    column: x => x.CurrentStateId,
                    principalTable: "GeographicLevelMetas",
                    principalColumn: "Id"
                );
                table.ForeignKey(
                    name: "FK_GeographicLevelMetaChanges_GeographicLevelMetas_PreviousSta~",
                    column: x => x.PreviousStateId,
                    principalTable: "GeographicLevelMetas",
                    principalColumn: "Id"
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "IndicatorMetaChanges",
            columns: table => new
            {
                Id = table
                    .Column<long>(type: "bigint", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                CurrentStateId = table.Column<int>(type: "integer", nullable: true),
                PreviousStateId = table.Column<int>(type: "integer", nullable: true),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_IndicatorMetaChanges", x => x.Id);
                table.ForeignKey(
                    name: "FK_IndicatorMetaChanges_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_IndicatorMetaChanges_IndicatorMetas_CurrentStateId",
                    column: x => x.CurrentStateId,
                    principalTable: "IndicatorMetas",
                    principalColumn: "Id"
                );
                table.ForeignKey(
                    name: "FK_IndicatorMetaChanges_IndicatorMetas_PreviousStateId",
                    column: x => x.PreviousStateId,
                    principalTable: "IndicatorMetas",
                    principalColumn: "Id"
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "LocationMetaChanges",
            columns: table => new
            {
                Id = table
                    .Column<long>(type: "bigint", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                CurrentStateId = table.Column<int>(type: "integer", nullable: true),
                PreviousStateId = table.Column<int>(type: "integer", nullable: true),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LocationMetaChanges", x => x.Id);
                table.ForeignKey(
                    name: "FK_LocationMetaChanges_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_LocationMetaChanges_LocationMetas_CurrentStateId",
                    column: x => x.CurrentStateId,
                    principalTable: "LocationMetas",
                    principalColumn: "Id"
                );
                table.ForeignKey(
                    name: "FK_LocationMetaChanges_LocationMetas_PreviousStateId",
                    column: x => x.PreviousStateId,
                    principalTable: "LocationMetas",
                    principalColumn: "Id"
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "LocationOptionMetaChanges",
            columns: table => new
            {
                Id = table
                    .Column<long>(type: "bigint", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                CurrentState_MetaId = table.Column<int>(type: "integer", nullable: true),
                CurrentState_OptionId = table.Column<int>(type: "integer", nullable: true),
                CurrentState_PublicId = table.Column<string>(type: "text", nullable: true),
                PreviousState_MetaId = table.Column<int>(type: "integer", nullable: true),
                PreviousState_OptionId = table.Column<int>(type: "integer", nullable: true),
                PreviousState_PublicId = table.Column<string>(type: "text", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LocationOptionMetaChanges", x => x.Id);
                table.ForeignKey(
                    name: "FK_LocationOptionMetaChanges_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_LocationOptionMetaChanges_LocationMetas_CurrentState_MetaId",
                    column: x => x.CurrentState_MetaId,
                    principalTable: "LocationMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_LocationOptionMetaChanges_LocationMetas_PreviousState_MetaId",
                    column: x => x.PreviousState_MetaId,
                    principalTable: "LocationMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_LocationOptionMetaChanges_LocationOptionMetas_CurrentState_~",
                    column: x => x.CurrentState_OptionId,
                    principalTable: "LocationOptionMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_LocationOptionMetaChanges_LocationOptionMetas_PreviousState~",
                    column: x => x.PreviousState_OptionId,
                    principalTable: "LocationOptionMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "LocationOptionMetaLinks",
            columns: table => new
            {
                MetaId = table.Column<int>(type: "integer", nullable: false),
                OptionId = table.Column<int>(type: "integer", nullable: false),
                PublicId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LocationOptionMetaLinks", x => new { x.MetaId, x.OptionId });
                table.ForeignKey(
                    name: "FK_LocationOptionMetaLinks_LocationMetas_MetaId",
                    column: x => x.MetaId,
                    principalTable: "LocationMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_LocationOptionMetaLinks_LocationOptionMetas_OptionId",
                    column: x => x.OptionId,
                    principalTable: "LocationOptionMetas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "TimePeriodMetaChanges",
            columns: table => new
            {
                Id = table
                    .Column<long>(type: "bigint", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    ),
                CurrentStateId = table.Column<int>(type: "integer", nullable: true),
                PreviousStateId = table.Column<int>(type: "integer", nullable: true),
                DataSetVersionId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TimePeriodMetaChanges", x => x.Id);
                table.ForeignKey(
                    name: "FK_TimePeriodMetaChanges_DataSetVersions_DataSetVersionId",
                    column: x => x.DataSetVersionId,
                    principalTable: "DataSetVersions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_TimePeriodMetaChanges_TimePeriodMetas_CurrentStateId",
                    column: x => x.CurrentStateId,
                    principalTable: "TimePeriodMetas",
                    principalColumn: "Id"
                );
                table.ForeignKey(
                    name: "FK_TimePeriodMetaChanges_TimePeriodMetas_PreviousStateId",
                    column: x => x.PreviousStateId,
                    principalTable: "TimePeriodMetas",
                    principalColumn: "Id"
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSets_LatestDraftVersionId",
            table: "DataSets",
            column: "LatestDraftVersionId",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSets_LatestLiveVersionId",
            table: "DataSets",
            column: "LatestLiveVersionId",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSets_SupersedingDataSetId",
            table: "DataSets",
            column: "SupersedingDataSetId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersionImports_DataSetVersionId",
            table: "DataSetVersionImports",
            column: "DataSetVersionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersionImports_InstanceId",
            table: "DataSetVersionImports",
            column: "InstanceId",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersionMappings_SourceDataSetVersionId",
            table: "DataSetVersionMappings",
            column: "SourceDataSetVersionId",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersionMappings_TargetDataSetVersionId",
            table: "DataSetVersionMappings",
            column: "TargetDataSetVersionId",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersions_DataSetId_VersionNumber",
            table: "DataSetVersions",
            columns: new[] { "DataSetId", "VersionMajor", "VersionMinor", "VersionPatch" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersions_Release_DataSetFileId",
            table: "DataSetVersions",
            column: "Release_DataSetFileId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataSetVersions_Release_ReleaseFileId",
            table: "DataSetVersions",
            column: "Release_ReleaseFileId",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterMetaChanges_CurrentStateId",
            table: "FilterMetaChanges",
            column: "CurrentStateId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterMetaChanges_DataSetVersionId",
            table: "FilterMetaChanges",
            column: "DataSetVersionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterMetaChanges_PreviousStateId",
            table: "FilterMetaChanges",
            column: "PreviousStateId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterMetas_DataSetVersionId_Column",
            table: "FilterMetas",
            columns: new[] { "DataSetVersionId", "Column" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterMetas_DataSetVersionId_PublicId",
            table: "FilterMetas",
            columns: new[] { "DataSetVersionId", "PublicId" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterOptionMetaChanges_CurrentState_MetaId",
            table: "FilterOptionMetaChanges",
            column: "CurrentState_MetaId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterOptionMetaChanges_CurrentState_OptionId",
            table: "FilterOptionMetaChanges",
            column: "CurrentState_OptionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterOptionMetaChanges_DataSetVersionId",
            table: "FilterOptionMetaChanges",
            column: "DataSetVersionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterOptionMetaChanges_PreviousState_MetaId",
            table: "FilterOptionMetaChanges",
            column: "PreviousState_MetaId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterOptionMetaChanges_PreviousState_OptionId",
            table: "FilterOptionMetaChanges",
            column: "PreviousState_OptionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterOptionMetaLinks_MetaId_PublicId",
            table: "FilterOptionMetaLinks",
            columns: new[] { "MetaId", "PublicId" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_FilterOptionMetaLinks_OptionId",
            table: "FilterOptionMetaLinks",
            column: "OptionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_GeographicLevelMetaChanges_CurrentStateId",
            table: "GeographicLevelMetaChanges",
            column: "CurrentStateId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_GeographicLevelMetaChanges_DataSetVersionId",
            table: "GeographicLevelMetaChanges",
            column: "DataSetVersionId",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_GeographicLevelMetaChanges_PreviousStateId",
            table: "GeographicLevelMetaChanges",
            column: "PreviousStateId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_GeographicLevelMetas_DataSetVersionId",
            table: "GeographicLevelMetas",
            column: "DataSetVersionId",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_IndicatorMetaChanges_CurrentStateId",
            table: "IndicatorMetaChanges",
            column: "CurrentStateId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_IndicatorMetaChanges_DataSetVersionId",
            table: "IndicatorMetaChanges",
            column: "DataSetVersionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_IndicatorMetaChanges_PreviousStateId",
            table: "IndicatorMetaChanges",
            column: "PreviousStateId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_IndicatorMetas_DataSetVersionId_Column",
            table: "IndicatorMetas",
            columns: new[] { "DataSetVersionId", "Column" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_IndicatorMetas_DataSetVersionId_PublicId",
            table: "IndicatorMetas",
            columns: new[] { "DataSetVersionId", "PublicId" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationMetaChanges_CurrentStateId",
            table: "LocationMetaChanges",
            column: "CurrentStateId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationMetaChanges_DataSetVersionId",
            table: "LocationMetaChanges",
            column: "DataSetVersionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationMetaChanges_PreviousStateId",
            table: "LocationMetaChanges",
            column: "PreviousStateId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationMetas_DataSetVersionId_Level",
            table: "LocationMetas",
            columns: new[] { "DataSetVersionId", "Level" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationOptionMetaChanges_CurrentState_MetaId",
            table: "LocationOptionMetaChanges",
            column: "CurrentState_MetaId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationOptionMetaChanges_CurrentState_OptionId",
            table: "LocationOptionMetaChanges",
            column: "CurrentState_OptionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationOptionMetaChanges_DataSetVersionId",
            table: "LocationOptionMetaChanges",
            column: "DataSetVersionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationOptionMetaChanges_PreviousState_MetaId",
            table: "LocationOptionMetaChanges",
            column: "PreviousState_MetaId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationOptionMetaChanges_PreviousState_OptionId",
            table: "LocationOptionMetaChanges",
            column: "PreviousState_OptionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationOptionMetaLinks_MetaId_PublicId",
            table: "LocationOptionMetaLinks",
            columns: new[] { "MetaId", "PublicId" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationOptionMetaLinks_OptionId",
            table: "LocationOptionMetaLinks",
            column: "OptionId"
        );

        migrationBuilder
            .CreateIndex(
                name: "IX_LocationOptionMetas_All",
                table: "LocationOptionMetas",
                columns: new[] { "Type", "Label", "Code", "OldCode", "Urn", "LaEstab", "Ukprn" },
                unique: true
            )
            .Annotation("Npgsql:NullsDistinct", false);

        migrationBuilder.CreateIndex(name: "IX_LocationOptionMetas_Code", table: "LocationOptionMetas", column: "Code");

        migrationBuilder.CreateIndex(
            name: "IX_LocationOptionMetas_LaEstab",
            table: "LocationOptionMetas",
            column: "LaEstab"
        );

        migrationBuilder.CreateIndex(
            name: "IX_LocationOptionMetas_OldCode",
            table: "LocationOptionMetas",
            column: "OldCode"
        );

        migrationBuilder.CreateIndex(name: "IX_LocationOptionMetas_Type", table: "LocationOptionMetas", column: "Type");

        migrationBuilder.CreateIndex(
            name: "IX_LocationOptionMetas_Ukprn",
            table: "LocationOptionMetas",
            column: "Ukprn"
        );

        migrationBuilder.CreateIndex(name: "IX_LocationOptionMetas_Urn", table: "LocationOptionMetas", column: "Urn");

        migrationBuilder.CreateIndex(
            name: "IX_PreviewTokens_DataSetVersionId",
            table: "PreviewTokens",
            column: "DataSetVersionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_TimePeriodMetaChanges_CurrentStateId",
            table: "TimePeriodMetaChanges",
            column: "CurrentStateId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_TimePeriodMetaChanges_DataSetVersionId",
            table: "TimePeriodMetaChanges",
            column: "DataSetVersionId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_TimePeriodMetaChanges_PreviousStateId",
            table: "TimePeriodMetaChanges",
            column: "PreviousStateId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_TimePeriodMetas_DataSetVersionId_Code_Period",
            table: "TimePeriodMetas",
            columns: new[] { "DataSetVersionId", "Code", "Period" },
            unique: true
        );

        migrationBuilder.AddForeignKey(
            name: "FK_DataSets_DataSetVersions_LatestDraftVersionId",
            table: "DataSets",
            column: "LatestDraftVersionId",
            principalTable: "DataSetVersions",
            principalColumn: "Id"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_DataSets_DataSetVersions_LatestLiveVersionId",
            table: "DataSets",
            column: "LatestLiveVersionId",
            principalTable: "DataSetVersions",
            principalColumn: "Id"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_DataSets_DataSetVersions_LatestDraftVersionId", table: "DataSets");

        migrationBuilder.DropForeignKey(name: "FK_DataSets_DataSetVersions_LatestLiveVersionId", table: "DataSets");

        migrationBuilder.DropTable(name: "DataSetVersionImports");

        migrationBuilder.DropTable(name: "DataSetVersionMappings");

        migrationBuilder.DropTable(name: "FilterMetaChanges");

        migrationBuilder.DropTable(name: "FilterOptionMetaChanges");

        migrationBuilder.DropTable(name: "FilterOptionMetaLinks");

        migrationBuilder.DropTable(name: "GeographicLevelMetaChanges");

        migrationBuilder.DropTable(name: "IndicatorMetaChanges");

        migrationBuilder.DropTable(name: "LocationMetaChanges");

        migrationBuilder.DropTable(name: "LocationOptionMetaChanges");

        migrationBuilder.DropTable(name: "LocationOptionMetaLinks");

        migrationBuilder.DropTable(name: "PreviewTokens");

        migrationBuilder.DropTable(name: "TimePeriodMetaChanges");

        migrationBuilder.DropTable(name: "FilterMetas");

        migrationBuilder.DropTable(name: "FilterOptionMetas");

        migrationBuilder.DropTable(name: "GeographicLevelMetas");

        migrationBuilder.DropTable(name: "IndicatorMetas");

        migrationBuilder.DropTable(name: "LocationMetas");

        migrationBuilder.DropTable(name: "LocationOptionMetas");

        migrationBuilder.DropTable(name: "TimePeriodMetas");

        migrationBuilder.DropTable(name: "DataSetVersions");

        migrationBuilder.DropTable(name: "DataSets");

        migrationBuilder.DropSequence(name: "FilterOptionMetaLink_seq");
    }
}
