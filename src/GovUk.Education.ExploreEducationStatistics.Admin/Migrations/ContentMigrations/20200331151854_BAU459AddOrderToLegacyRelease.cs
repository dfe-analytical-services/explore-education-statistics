using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class BAU459AddOrderToLegacyRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Link");

            migrationBuilder.CreateTable(
                name: "LegacyRelease",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Order = table.Column<int>(nullable: false),
                    PublicationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegacyRelease", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegacyRelease_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LegacyRelease",
                columns: new[] { "Id", "Description", "Order", "PublicationId", "Url" },
                values: new object[,]
                {
                    { new Guid("08134c1d-8a58-49a4-8d8b-22e586ffd5ae"), "Academic Year 2008/09", 0, new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-academic-year-2008-to-2009" },
                    { new Guid("5e244416-6f2a-4d22-bea4-c22a229befef"), "January 2015", 5, new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2015" },
                    { new Guid("398ba8c6-3ea0-49da-8645-ceb3c7fb9860"), "January 2014", 4, new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2014" },
                    { new Guid("e6b36ee8-ef66-4864-a4b3-9047ee3da338"), "January 2013", 3, new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2013" },
                    { new Guid("181ec43e-cf22-4cab-a128-0a5702468566"), "January 2012", 2, new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2012" },
                    { new Guid("b086ba70-703c-40dd-aaef-d2e19335188e"), "January 2011", 1, new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2011" },
                    { new Guid("dc8b0d8c-08bb-47cc-b3a1-9e6ac9c2c268"), "January 2010", 0, new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2010" },
                    { new Guid("28e53936-5a52-44be-a7a6-d2f14a426d28"), "Academic Year 2014/15", 5, new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015" },
                    { new Guid("75991639-ad77-4ba6-91fc-ac08c00a4ce8"), "Academic Year 2013/14", 4, new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014" },
                    { new Guid("ce15f487-87b0-4c07-98f1-6c6732196be7"), "Academic Year 2012/13", 3, new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013" },
                    { new Guid("e20141d0-d894-4b8d-a78f-e41c23500786"), "Academic Year 2011/12", 2, new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics" },
                    { new Guid("81d91c86-9bf2-496c-b026-9dc255c35635"), "Academic Year 2010/11", 1, new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011" },
                    { new Guid("89c02688-646d-45b5-8919-9a3fafcfe0e9"), "Academic Year 2009/10", 0, new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010" },
                    { new Guid("564dacdc-f58e-4aa0-8dbd-d8368b4fb6ba"), "Academic Year 2015/16", 7, new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016" },
                    { new Guid("78239816-507d-42b7-98fd-4a71d0d4eb1f"), "Academic Year 2014/15", 6, new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015" },
                    { new Guid("f1225f98-40d5-494c-90f9-99f9fb59ac9d"), "Academic Year 2013/14", 5, new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014" },
                    { new Guid("45bd7f62-2018-4a5c-9b93-ccece8e89c46"), "Academic Year 2012/13", 4, new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2012-to-2013" },
                    { new Guid("13acf54a-8016-49ff-9050-c61ebe7acad2"), "Academic Year 2011/12", 3, new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-2011-to-2012-academic-year" },
                    { new Guid("a319e4ef-b957-40fb-8a47-b1a97814b220"), "Academic Year 2010/11", 2, new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2010-to-2011" },
                    { new Guid("250bace6-aeb9-4fe9-8de2-3a25e0dc717f"), "Academic Year 2009/10", 1, new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2009-to-2010" },
                    { new Guid("e3c1db23-8a8f-47fe-b2cd-8e677db700a2"), "January 2016", 6, new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2016" },
                    { new Guid("313435b3-fe56-4b92-8e13-670dbf510062"), "January 2017", 7, new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2017" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegacyRelease_PublicationId",
                table: "LegacyRelease",
                column: "PublicationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegacyRelease");

            migrationBuilder.CreateTable(
                name: "Link",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[,]
                {
                    { new Guid("08134c1d-8a58-49a4-8d8b-22e586ffd5ae"), "Academic Year 2008/09", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-academic-year-2008-to-2009" },
                    { new Guid("5e244416-6f2a-4d22-bea4-c22a229befef"), "January 2015", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2015" },
                    { new Guid("398ba8c6-3ea0-49da-8645-ceb3c7fb9860"), "January 2014", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2014" },
                    { new Guid("e6b36ee8-ef66-4864-a4b3-9047ee3da338"), "January 2013", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2013" },
                    { new Guid("181ec43e-cf22-4cab-a128-0a5702468566"), "January 2012", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2012" },
                    { new Guid("b086ba70-703c-40dd-aaef-d2e19335188e"), "January 2011", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2011" },
                    { new Guid("dc8b0d8c-08bb-47cc-b3a1-9e6ac9c2c268"), "January 2010", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2010" },
                    { new Guid("28e53936-5a52-44be-a7a6-d2f14a426d28"), "Academic Year 2014/15", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015" },
                    { new Guid("75991639-ad77-4ba6-91fc-ac08c00a4ce8"), "Academic Year 2013/14", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014" },
                    { new Guid("ce15f487-87b0-4c07-98f1-6c6732196be7"), "Academic Year 2012/13", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013" },
                    { new Guid("e20141d0-d894-4b8d-a78f-e41c23500786"), "Academic Year 2011/12", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics" },
                    { new Guid("81d91c86-9bf2-496c-b026-9dc255c35635"), "Academic Year 2010/11", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011" },
                    { new Guid("89c02688-646d-45b5-8919-9a3fafcfe0e9"), "Academic Year 2009/10", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010" },
                    { new Guid("564dacdc-f58e-4aa0-8dbd-d8368b4fb6ba"), "Academic Year 2015/16", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016" },
                    { new Guid("78239816-507d-42b7-98fd-4a71d0d4eb1f"), "Academic Year 2014/15", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015" },
                    { new Guid("f1225f98-40d5-494c-90f9-99f9fb59ac9d"), "Academic Year 2013/14", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014" },
                    { new Guid("45bd7f62-2018-4a5c-9b93-ccece8e89c46"), "Academic Year 2012/13", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2012-to-2013" },
                    { new Guid("13acf54a-8016-49ff-9050-c61ebe7acad2"), "Academic Year 2011/12", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-2011-to-2012-academic-year" },
                    { new Guid("a319e4ef-b957-40fb-8a47-b1a97814b220"), "Academic Year 2010/11", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2010-to-2011" },
                    { new Guid("250bace6-aeb9-4fe9-8de2-3a25e0dc717f"), "Academic Year 2009/10", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2009-to-2010" },
                    { new Guid("e3c1db23-8a8f-47fe-b2cd-8e677db700a2"), "January 2016", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2016" },
                    { new Guid("313435b3-fe56-4b92-8e13-670dbf510062"), "January 2017", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2017" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Link_PublicationId",
                table: "Link",
                column: "PublicationId");
        }
    }
}
