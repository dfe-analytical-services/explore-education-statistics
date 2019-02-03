using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class SeedContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    Summary = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ThemeId = table.Column<Guid>(nullable: false),
                    Summary = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topics_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Publications",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DataSource = table.Column<string>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    NextUpdate = table.Column<DateTime>(nullable: true),
                    TopicId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Publications_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Link",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    PublicationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Link", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Link_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Releases",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    ReleaseName = table.Column<string>(nullable: true),
                    Published = table.Column<DateTime>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    PublicationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Releases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Releases_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Update",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    On = table.Column<DateTime>(nullable: false),
                    Reason = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Update", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Update_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "early-years-and-schools", "Lorem ipsum dolor sit amet.", "Early years and schools" });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), "social-care", "Lorem ipsum dolor sit amet.", "Social Care" });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), "higher-education", "Lorem ipsum dolor sit amet.", "Higher education" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), null, "absence-and-exclusions", "Pupil absence and permanent and fixed-period exclusions statistics and data", new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Absence and exclusions" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), null, "school-and-pupil-numbers", "Schools, pupils and their characteristics, SEN and EHC plans, SEN in England", new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "School & pupil numbers" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"), null, "capacity-admissions", "School capacity, admission appeals", new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Capacity and admissions" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), null, "results", "Local authority and school finance", new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Results" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"), null, "school-finance", "Local authority and school finance", new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "School finance" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"), null, "teacher-numbers", "The number and characteristics of teachers", new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), "Teacher Numbers" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"), null, "number-of-children", "Lorem ipsum dolor sit amet.", new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), "Number of Children" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"), null, "vulnerable-children", "Lorem ipsum dolor sit amet.", new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), "Vulnerable Children" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"), null, "further-education", "Lorem ipsum dolor sit amet.", new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), "Further Education" });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("4c658598-450b-4493-b972-8812acd154a7"), null, "higher-education", "Lorem ipsum dolor sit amet.", new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), "Higher Education" });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "[Pupil absence statistics: guide](https://www.gov.uk/government/publications/absence-statistics-guide#)", null, new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "pupil-absence-in-schools-in-england", "View statistics, create charts and tables and download data files for authorised, overall, persistent and unauthorised absence", "Pupil absence in schools in England", new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("ad81ebdd-2bbc-47e8-a32c-f396d6e2bb72"), null, null, null, "further-education-and-skills", "Lorem ipsum dolor sit amet.", "Further education and skills", new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("d0e56978-c944-4b12-9156-bfe50c94c2a0"), null, null, null, "destination-of-leavers", "Lorem ipsum dolor sit amet.", "Destination of leavers", new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("70902b3c-0bb4-457d-b40a-2a959cdc7d00"), null, null, null, "16-to-18-school-performance", "Lorem ipsum dolor sit amet.", "16 to 18 school performance", new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("143c672b-18d7-478b-a6e7-b843c9b3fd42"), null, null, null, "looked-after-children", "Lorem ipsum dolor sit amet.", "Looked after children", new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("bd781dc5-cfc7-4543-b8d7-a3a7b3606b3d"), null, null, null, "children-in-need", "Lorem ipsum dolor sit amet.", "Children in need", new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("fe94b33d-0419-4fac-bf73-28299d5e4247"), null, null, null, "initial-teacher-training-performance-profiles", "Lorem ipsum dolor sit amet.", "Initial teacher training performance profiles", new Guid("d5288137-e703-43a1-b634-d50fc9785cb9") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("8b2c1269-3495-4f89-83eb-524fc0b6effc"), null, null, null, "school-workforce", "Lorem ipsum dolor sit amet.", "School workforce", new Guid("d5288137-e703-43a1-b634-d50fc9785cb9") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"), null, null, null, "ks4-statistics", "Lorem ipsum dolor sit amet.", "KS4 statistics", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("a4b22113-47d3-48fc-b2da-5336c801a31f"), null, null, null, "ks2-statistics", "Lorem ipsum dolor sit amet.", "KS2 statistics", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("9674ac24-649a-400c-8a2c-871793d9cd7a"), null, null, null, "phonics-screening-check-and-ks1-assessments", "Lorem ipsum dolor sit amet.", "Phonics screening check and KS1 assessments", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("526dea0e-abf3-476e-9ca4-9dbd9b101bc8"), null, null, null, "early-years-foundation-stage-profile-results", "Lorem ipsum dolor sit amet.", "Early years foundation stage profile results", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("a20ea465-d2d0-4fc1-96ee-6b2ca4e0520e"), null, null, null, "admission-appeals-in-England", "Lorem ipsum dolor sit amet.", "Admission appeals in England", new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("d04142bd-f448-456b-97bc-03863143836b"), null, null, null, "school-capacity", "Lorem ipsum dolor sit amet.", "School capacity", new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), null, null, null, "schools-pupils-and-their-characteristics", "Lorem ipsum dolor sit amet.", "Schools, pupils and their characteristics", new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), null, null, null, "permanent-and-fixed-period-exclusions", "View statistics, create charts and tables and download data files for fixed-period and permanent exclusion statistics", "Permanent and fixed period exclusions", new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("201cb72d-ef35-4680-ade7-b09a8dca9cc1"), null, null, null, "apprenticeship-and-levy-statistics", "Lorem ipsum dolor sit amet.", "Apprenticeship and levy statistics", new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("7bd128a3-ae7f-4e1b-984e-d1b795c61630"), null, null, null, "apprenticeships-and-traineeships", "Lorem ipsum dolor sit amet.", "Apprenticeships and traineeships", new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860") });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("45bc02ff-de90-489b-b78e-cdc7db662353"), "2014 to 2015", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("82292fe7-1545-44eb-a094-80c5064701a7"), "2013 to 2014", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("6907625d-0c2e-4fd8-8e96-aedd85b2ff97"), "2012 to 2013", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("a538e57a-da5e-4a2c-a89e-b74dbae0c30b"), "2011 to 2012", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("18b24d60-c56e-44f0-8baa-6db4c6e7deee"), "2010 to 2011", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("c5444f5a-6ba5-4c80-883c-6bca0d8a9eb5"), "2009 to 2010", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2017, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016 to 2017", "2016-17", "This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools. It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:", "Pupil absence data and statistics for schools in England" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"), new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2016, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2015 to 2016", "2015-16", "This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools. It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:", "Pupil absence data and statistics for schools in England" });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"), new DateTime(2017, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Underlying data file updated to include absence data by pupil residency and school location, andupdated metadata document.", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("18e0d40e-bdf7-4c84-99dd-732e72e9c9a5"), new DateTime(2017, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("51bd1e2f-2669-4708-b300-799b6be9ec9a"), new DateTime(2016, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d") });

            migrationBuilder.CreateIndex(
                name: "IX_Link_PublicationId",
                table: "Link",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_TopicId",
                table: "Publications",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_PublicationId",
                table: "Releases",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_ThemeId",
                table: "Topics",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Update_ReleaseId",
                table: "Update",
                column: "ReleaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Link");

            migrationBuilder.DropTable(
                name: "Update");

            migrationBuilder.DropTable(
                name: "Releases");

            migrationBuilder.DropTable(
                name: "Publications");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
