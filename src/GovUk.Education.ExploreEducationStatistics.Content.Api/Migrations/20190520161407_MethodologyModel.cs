using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class MethodologyModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Methodologies",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Summary = table.Column<string>(nullable: true),
                    Published = table.Column<DateTime>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Annexes = table.Column<string>(nullable: true),
                    PublicationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Methodologies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Methodologies_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Summary",
                value: @"Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas. 

");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "Summary",
                value: @"Read national statistical summaries and definitions, view charts and tables and download data files across a range of permanent and fixed-period exclusion subject areas. 

You can also view a regional breakdown of statistics and data within the [local authorities section](#contents-sections-heading-9)");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7ae88fb-afaf-4d51-a78a-bbb2de671daf"),
                column: "Summary",
                value: @"This statistical first release (SFR) provides information on the achievements in GCSE examinations and other qualifications of young people in academic year 2016 to 2017. This typically covers those starting the academic year aged 15. 

You can also view a regional breakdown of statistics and data within the [local authorities section](#contents-sections-content-6) 

[Find out more about our GCSE and equivalent results methodology and terminology](#extra-information-sections-heading-1)");

            migrationBuilder.CreateIndex(
                name: "IX_Methodologies_PublicationId",
                table: "Methodologies",
                column: "PublicationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Methodologies");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Summary",
                value: @"Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas. 

");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "Summary",
                value: @"Read national statistical summaries and definitions, view charts and tables and download data files across a range of permanent and fixed-period exclusion subject areas. 

You can also view a regional breakdown of statistics and data within the [local authorities section](#contents-sections-heading-9)");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7ae88fb-afaf-4d51-a78a-bbb2de671daf"),
                column: "Summary",
                value: @"This statistical first release (SFR) provides information on the achievements in GCSE examinations and other qualifications of young people in academic year 2016 to 2017. This typically covers those starting the academic year aged 15. 

You can also view a regional breakdown of statistics and data within the [local authorities section](#contents-sections-content-6) 

[Find out more about our GCSE and equivalent results methodology and terminology](#extra-information-sections-heading-1)");
        }
    }
}
