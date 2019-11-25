using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    [ExcludeFromCodeCoverage]
    public partial class RemoveEmptyRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "InternalReleaseNote", "KeyStatistics", "NextReleaseDate", "Order", "PublicationId", "PublishScheduled", "Published", "ReleaseName", "Slug", "Status", "Summary", "TimePeriodCoverage", "TypeId" },
                values: new object[] { new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"), "[{\"Order\":1,\"Heading\":\"About these statistics\",\"Caption\":\"\",\"Content\":null},{\"Order\":2,\"Heading\":\"Absence rates\",\"Caption\":\"\",\"Content\":null},{\"Order\":3,\"Heading\":\"Persistent absence\",\"Caption\":\"\",\"Content\":null},{\"Order\":4,\"Heading\":\"Distribution of absence\",\"Caption\":\"\",\"Content\":null},{\"Order\":5,\"Heading\":\"Absence for four year olds\",\"Caption\":\"\",\"Content\":null},{\"Order\":6,\"Heading\":\"Pupil referral unit absence\",\"Caption\":\"\",\"Content\":null},{\"Order\":7,\"Heading\":\"Pupil absence by local authority\",\"Caption\":\"\",\"Content\":null}]", null, "{\"Type\":\"DataBlock\",\"Id\":\"8a1a6a1e-5da2-45b0-a63d-6338a12585f1\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"dataSummary\":[\"\",\"\",\"\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}", null, 0, new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), null, new DateTime(2016, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2015", "2015-16", "Draft", "Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas.", "AY", new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c") });
        }
    }
}
