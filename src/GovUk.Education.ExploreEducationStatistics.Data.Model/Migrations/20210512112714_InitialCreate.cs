using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoundaryLevel",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<string>(nullable: false),
                    Label = table.Column<string>(nullable: true),
                    Published = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoundaryLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Footnote",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Footnote", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Country_Code = table.Column<string>(nullable: true),
                    Country_Name = table.Column<string>(nullable: true),
                    EnglishDevolvedArea_Code = table.Column<string>(nullable: true),
                    EnglishDevolvedArea_Name = table.Column<string>(nullable: true),
                    Institution_Code = table.Column<string>(nullable: true),
                    Institution_Name = table.Column<string>(nullable: true),
                    LocalAuthority_Code = table.Column<string>(nullable: true),
                    LocalAuthority_OldCode = table.Column<string>(nullable: true),
                    LocalAuthority_Name = table.Column<string>(nullable: true),
                    LocalAuthorityDistrict_Code = table.Column<string>(nullable: true),
                    LocalAuthorityDistrict_Name = table.Column<string>(nullable: true),
                    LocalEnterprisePartnership_Code = table.Column<string>(nullable: true),
                    LocalEnterprisePartnership_Name = table.Column<string>(nullable: true),
                    MayoralCombinedAuthority_Code = table.Column<string>(nullable: true),
                    MayoralCombinedAuthority_Name = table.Column<string>(nullable: true),
                    MultiAcademyTrust_Code = table.Column<string>(nullable: true),
                    MultiAcademyTrust_Name = table.Column<string>(nullable: true),
                    OpportunityArea_Code = table.Column<string>(nullable: true),
                    OpportunityArea_Name = table.Column<string>(nullable: true),
                    ParliamentaryConstituency_Code = table.Column<string>(nullable: true),
                    ParliamentaryConstituency_Name = table.Column<string>(nullable: true),
                    Region_Code = table.Column<string>(nullable: true),
                    Region_Name = table.Column<string>(nullable: true),
                    RscRegion_Code = table.Column<string>(nullable: true),
                    Sponsor_Code = table.Column<string>(nullable: true),
                    Sponsor_Name = table.Column<string>(nullable: true),
                    Ward_Code = table.Column<string>(nullable: true),
                    Ward_Name = table.Column<string>(nullable: true),
                    PlanningArea_Code = table.Column<string>(nullable: true),
                    PlanningArea_Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subject",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SoftDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subject", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Theme",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Theme", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Filter",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Hint = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    SubjectId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Filter_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndicatorGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Label = table.Column<string>(nullable: true),
                    SubjectId = table.Column<Guid>(nullable: false)
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
                    Id = table.Column<Guid>(nullable: false),
                    SubjectId = table.Column<Guid>(nullable: false),
                    GeographicLevel = table.Column<string>(maxLength: 6, nullable: false),
                    LocationId = table.Column<Guid>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    TimeIdentifier = table.Column<string>(maxLength: 6, nullable: false),
                    Measures = table.Column<string>(nullable: true),
                    CsvRow = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Observation_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Observation_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectFootnote",
                columns: table => new
                {
                    SubjectId = table.Column<Guid>(nullable: false),
                    FootnoteId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectFootnote", x => new { x.SubjectId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_SubjectFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectFootnote_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Topic",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    ThemeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topic_Theme_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Theme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilterFootnote",
                columns: table => new
                {
                    FilterId = table.Column<Guid>(nullable: false),
                    FootnoteId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterFootnote", x => new { x.FilterId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_FilterFootnote_Filter_FilterId",
                        column: x => x.FilterId,
                        principalTable: "Filter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilterGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FilterId = table.Column<Guid>(nullable: false),
                    Label = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilterGroup_Filter_FilterId",
                        column: x => x.FilterId,
                        principalTable: "Filter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Indicator",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Label = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: false),
                    IndicatorGroupId = table.Column<Guid>(nullable: false),
                    DecimalPlaces = table.Column<int>(nullable: true)
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
                name: "Publication",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    TopicId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Publication_Topic_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilterGroupFootnote",
                columns: table => new
                {
                    FilterGroupId = table.Column<Guid>(nullable: false),
                    FootnoteId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterGroupFootnote", x => new { x.FilterGroupId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_FilterGroupFootnote_FilterGroup_FilterGroupId",
                        column: x => x.FilterGroupId,
                        principalTable: "FilterGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterGroupFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilterItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Label = table.Column<string>(nullable: true),
                    FilterGroupId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilterItem_FilterGroup_FilterGroupId",
                        column: x => x.FilterGroupId,
                        principalTable: "FilterGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndicatorFootnote",
                columns: table => new
                {
                    IndicatorId = table.Column<Guid>(nullable: false),
                    FootnoteId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorFootnote", x => new { x.IndicatorId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_IndicatorFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndicatorFootnote_Indicator_IndicatorId",
                        column: x => x.IndicatorId,
                        principalTable: "Indicator",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Release",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Published = table.Column<DateTime>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    PublicationId = table.Column<Guid>(nullable: false),
                    TimeIdentifier = table.Column<string>(maxLength: 6, nullable: false),
                    Year = table.Column<int>(nullable: false),
                    PreviousVersionId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Release", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Release_Publication_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilterItemFootnote",
                columns: table => new
                {
                    FilterItemId = table.Column<Guid>(nullable: false),
                    FootnoteId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterItemFootnote", x => new { x.FilterItemId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_FilterItemFootnote_FilterItem_FilterItemId",
                        column: x => x.FilterItemId,
                        principalTable: "FilterItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterItemFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ObservationFilterItem",
                columns: table => new
                {
                    ObservationId = table.Column<Guid>(nullable: false),
                    FilterItemId = table.Column<Guid>(nullable: false),
                    FilterId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObservationFilterItem", x => new { x.ObservationId, x.FilterItemId });
                    table.ForeignKey(
                        name: "FK_ObservationFilterItem_Filter_FilterId",
                        column: x => x.FilterId,
                        principalTable: "Filter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "ReleaseFootnote",
                columns: table => new
                {
                    FootnoteId = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseFootnote", x => new { x.ReleaseId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_ReleaseFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseFootnote_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseSubject",
                columns: table => new
                {
                    SubjectId = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    MetaGuidance = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseSubject", x => new { x.ReleaseId, x.SubjectId });
                    table.ForeignKey(
                        name: "FK_ReleaseSubject_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseSubject_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Filter_Name",
                table: "Filter",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Filter_SubjectId",
                table: "Filter",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterFootnote_FootnoteId",
                table: "FilterFootnote",
                column: "FootnoteId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterGroup_FilterId",
                table: "FilterGroup",
                column: "FilterId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterGroupFootnote_FootnoteId",
                table: "FilterGroupFootnote",
                column: "FootnoteId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterItem_FilterGroupId",
                table: "FilterItem",
                column: "FilterGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterItemFootnote_FootnoteId",
                table: "FilterItemFootnote",
                column: "FootnoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Indicator_IndicatorGroupId",
                table: "Indicator",
                column: "IndicatorGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Indicator_Name",
                table: "Indicator",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorFootnote_FootnoteId",
                table: "IndicatorFootnote",
                column: "FootnoteId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorGroup_SubjectId",
                table: "IndicatorGroup",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Country_Code",
                table: "Location",
                column: "Country_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_EnglishDevolvedArea_Code",
                table: "Location",
                column: "EnglishDevolvedArea_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Institution_Code",
                table: "Location",
                column: "Institution_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocalAuthorityDistrict_Code",
                table: "Location",
                column: "LocalAuthorityDistrict_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocalAuthority_Code",
                table: "Location",
                column: "LocalAuthority_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocalAuthority_OldCode",
                table: "Location",
                column: "LocalAuthority_OldCode");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocalEnterprisePartnership_Code",
                table: "Location",
                column: "LocalEnterprisePartnership_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_MayoralCombinedAuthority_Code",
                table: "Location",
                column: "MayoralCombinedAuthority_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_MultiAcademyTrust_Code",
                table: "Location",
                column: "MultiAcademyTrust_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_OpportunityArea_Code",
                table: "Location",
                column: "OpportunityArea_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ParliamentaryConstituency_Code",
                table: "Location",
                column: "ParliamentaryConstituency_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_PlanningArea_Code",
                table: "Location",
                column: "PlanningArea_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Region_Code",
                table: "Location",
                column: "Region_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_RscRegion_Code",
                table: "Location",
                column: "RscRegion_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Sponsor_Code",
                table: "Location",
                column: "Sponsor_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Ward_Code",
                table: "Location",
                column: "Ward_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_GeographicLevel",
                table: "Observation",
                column: "GeographicLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_LocationId",
                table: "Observation",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_SubjectId",
                table: "Observation",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_TimeIdentifier",
                table: "Observation",
                column: "TimeIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Observation_Year",
                table: "Observation",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_ObservationFilterItem_FilterId",
                table: "ObservationFilterItem",
                column: "FilterId");

            migrationBuilder.CreateIndex(
                name: "IX_ObservationFilterItem_FilterItemId",
                table: "ObservationFilterItem",
                column: "FilterItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Publication_TopicId",
                table: "Publication",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Release_PreviousVersionId",
                table: "Release",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Release_PublicationId",
                table: "Release",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFootnote_FootnoteId",
                table: "ReleaseFootnote",
                column: "FootnoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseSubject_SubjectId",
                table: "ReleaseSubject",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectFootnote_FootnoteId",
                table: "SubjectFootnote",
                column: "FootnoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Topic_ThemeId",
                table: "Topic",
                column: "ThemeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoundaryLevel");

            migrationBuilder.DropTable(
                name: "FilterFootnote");

            migrationBuilder.DropTable(
                name: "FilterGroupFootnote");

            migrationBuilder.DropTable(
                name: "FilterItemFootnote");

            migrationBuilder.DropTable(
                name: "IndicatorFootnote");

            migrationBuilder.DropTable(
                name: "ObservationFilterItem");

            migrationBuilder.DropTable(
                name: "ReleaseFootnote");

            migrationBuilder.DropTable(
                name: "ReleaseSubject");

            migrationBuilder.DropTable(
                name: "SubjectFootnote");

            migrationBuilder.DropTable(
                name: "Indicator");

            migrationBuilder.DropTable(
                name: "FilterItem");

            migrationBuilder.DropTable(
                name: "Observation");

            migrationBuilder.DropTable(
                name: "Release");

            migrationBuilder.DropTable(
                name: "Footnote");

            migrationBuilder.DropTable(
                name: "IndicatorGroup");

            migrationBuilder.DropTable(
                name: "FilterGroup");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "Publication");

            migrationBuilder.DropTable(
                name: "Filter");

            migrationBuilder.DropTable(
                name: "Topic");

            migrationBuilder.DropTable(
                name: "Subject");

            migrationBuilder.DropTable(
                name: "Theme");
        }
    }
}
