using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class LegacyReleaseLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Links",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    PublicationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Links_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Links",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("8693c112-225e-4e09-80c2-820cb307bc58"), "2015 to 2016", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2015-to-2016" });

            migrationBuilder.InsertData(
                table: "Links",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("45bc02ff-de90-489b-b78e-cdc7db662353"), "2014 to 2015", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015" });

            migrationBuilder.InsertData(
                table: "Links",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("82292fe7-1545-44eb-a094-80c5064701a7"), "2013 to 2014", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014" });

            migrationBuilder.InsertData(
                table: "Links",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("6907625d-0c2e-4fd8-8e96-aedd85b2ff97"), "2012 to 2013", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013" });

            migrationBuilder.InsertData(
                table: "Links",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("a538e57a-da5e-4a2c-a89e-b74dbae0c30b"), "2011 to 2012", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics" });

            migrationBuilder.InsertData(
                table: "Links",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("18b24d60-c56e-44f0-8baa-6db4c6e7deee"), "2010 to 2011", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011" });

            migrationBuilder.InsertData(
                table: "Links",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("c5444f5a-6ba5-4c80-883c-6bca0d8a9eb5"), "2009 to 2010", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010" });

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "NextUpdate",
                value: new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Links_PublicationId",
                table: "Links",
                column: "PublicationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Links");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                column: "NextUpdate",
                value: null);
        }
    }
}
