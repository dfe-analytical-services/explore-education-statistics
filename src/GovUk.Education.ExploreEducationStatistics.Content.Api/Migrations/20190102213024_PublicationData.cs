using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class PublicationData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), null, null, "permanent-and-fixed-period-exclusions", "Permanent and fixed period exclusions", new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), null, null, "schools-pupils-and-their-characteristics", "Schools, pupils and their characteristics", new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("d04142bd-f448-456b-97bc-03863143836b"), null, null, "school-capacity", "School capacity", new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("a20ea465-d2d0-4fc1-96ee-6b2ca4e0520e"), null, null, "admission-appeals-in-England", "Admission appeals in England", new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("526dea0e-abf3-476e-9ca4-9dbd9b101bc8"), null, null, "early-years-foundation-stage-profile-results", "Early years foundation stage profile results", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("9674ac24-649a-400c-8a2c-871793d9cd7a"), null, null, "phonics-screening-check-and-ks1-assessments", "Phonics screening check and KS1 assessments", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("a4b22113-47d3-48fc-b2da-5336c801a31f"), null, null, "ks2-statistics", "KS2 statistics", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"), null, null, "ks4-statistics", "KS4 statistics", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("8b2c1269-3495-4f89-83eb-524fc0b6effc"), null, null, "school-workforce", "School workforce", new Guid("d5288137-e703-43a1-b634-d50fc9785cb9") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("fe94b33d-0419-4fac-bf73-28299d5e4247"), null, null, "initial-teacher-training-performance-profiles", "Initial teacher training performance profiles", new Guid("d5288137-e703-43a1-b634-d50fc9785cb9") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("bd781dc5-cfc7-4543-b8d7-a3a7b3606b3d"), null, null, "children-in-need", "Children in need", new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "Description", "NextUpdate", "Slug", "Title", "TopicId" },
                values: new object[] { new Guid("143c672b-18d7-478b-a6e7-b843c9b3fd42"), null, null, "looked-after-children", "Looked after children", new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("143c672b-18d7-478b-a6e7-b843c9b3fd42"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("526dea0e-abf3-476e-9ca4-9dbd9b101bc8"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8b2c1269-3495-4f89-83eb-524fc0b6effc"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("9674ac24-649a-400c-8a2c-871793d9cd7a"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a20ea465-d2d0-4fc1-96ee-6b2ca4e0520e"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a4b22113-47d3-48fc-b2da-5336c801a31f"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bd781dc5-cfc7-4543-b8d7-a3a7b3606b3d"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d04142bd-f448-456b-97bc-03863143836b"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fe94b33d-0419-4fac-bf73-28299d5e4247"));
        }
    }
}
