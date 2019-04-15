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
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Country_Code = table.Column<string>(nullable: true),
                    Country_Name = table.Column<string>(nullable: true),
                    Region_Code = table.Column<string>(nullable: true),
                    Region_Name = table.Column<string>(nullable: true),
                    LocalAuthority_Code = table.Column<string>(nullable: true),
                    LocalAuthority_Old_Code = table.Column<string>(nullable: true),
                    LocalAuthority_Name = table.Column<string>(nullable: true),
                    LocalAuthorityDistrict_Code = table.Column<string>(nullable: true),
                    LocalAuthorityDistrict_Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
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
                name: "School",
                columns: table => new
                {
                    LaEstab = table.Column<string>(nullable: false),
                    AcademyOpenDate = table.Column<string>(nullable: true),
                    AcademyType = table.Column<string>(nullable: true),
                    Estab = table.Column<string>(nullable: true),
                    Urn = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_School", x => x.LaEstab);
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
                name: "FilterGroup",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(nullable: true),
                    SubjectId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilterGroup_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndicatorGroup",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(nullable: true),
                    SubjectId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndicatorGroup_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Observation",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubjectId = table.Column<long>(nullable: false),
                    Level = table.Column<string>(nullable: false),
                    LocationId = table.Column<long>(nullable: false),
                    SchoolLaEstab = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    TimePeriod = table.Column<int>(nullable: false),
                    Measures = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Observation_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Observation_School_SchoolLaEstab",
                        column: x => x.SchoolLaEstab,
                        principalTable: "School",
                        principalColumn: "LaEstab",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Observation_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Filter",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(nullable: true),
                    Hint = table.Column<string>(nullable: true),
                    FilterGroupId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Filter_FilterGroup_FilterGroupId",
                        column: x => x.FilterGroupId,
                        principalTable: "FilterGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Indicator",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: false),
                    IndicatorGroupId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indicator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Indicator_IndicatorGroup_IndicatorGroupId",
                        column: x => x.IndicatorGroupId,
                        principalTable: "IndicatorGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilterItem",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(nullable: true),
                    FilterId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilterItem_Filter_FilterId",
                        column: x => x.FilterId,
                        principalTable: "Filter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObservationFilterItem",
                columns: table => new
                {
                    ObservationId = table.Column<long>(nullable: false),
                    FilterItemId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObservationFilterItem", x => new { x.ObservationId, x.FilterItemId });
                    table.ForeignKey(
                        name: "FK_ObservationFilterItem_FilterItem_FilterItemId",
                        column: x => x.FilterItemId,
                        principalTable: "FilterItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ObservationFilterItem_Observation_ObservationId",
                        column: x => x.ObservationId,
                        principalTable: "Observation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Filter_FilterGroupId",
                table: "Filter",
                column: "FilterGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterGroup_SubjectId",
                table: "FilterGroup",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterItem_FilterId",
                table: "FilterItem",
                column: "FilterId");

            migrationBuilder.CreateIndex(
                name: "IX_Indicator_IndicatorGroupId",
                table: "Indicator",
                column: "IndicatorGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorGroup_SubjectId",
                table: "IndicatorGroup",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Country_Code",
                table: "Location",
                column: "Country_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocalAuthority_Code",
                table: "Location",
                column: "LocalAuthority_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocalAuthorityDistrict_Code",
                table: "Location",
                column: "LocalAuthorityDistrict_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Region_Code",
                table: "Location",
                column: "Region_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_LocationId",
                table: "Observation",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_SchoolLaEstab",
                table: "Observation",
                column: "SchoolLaEstab");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_SubjectId",
                table: "Observation",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_TimePeriod",
                table: "Observation",
                column: "TimePeriod");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_Year",
                table: "Observation",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_ObservationFilterItem_FilterItemId",
                table: "ObservationFilterItem",
                column: "FilterItemId");

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
                name: "Indicator");

            migrationBuilder.DropTable(
                name: "ObservationFilterItem");

            migrationBuilder.DropTable(
                name: "IndicatorGroup");

            migrationBuilder.DropTable(
                name: "FilterItem");

            migrationBuilder.DropTable(
                name: "Observation");

            migrationBuilder.DropTable(
                name: "Filter");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "School");

            migrationBuilder.DropTable(
                name: "FilterGroup");

            migrationBuilder.DropTable(
                name: "Subject");

            migrationBuilder.DropTable(
                name: "Release");
        }
    }
}
