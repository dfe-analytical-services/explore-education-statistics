using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class AdditionalExclusionContentKeyStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataQuery\":{\"path\":\"/api/tablebuilder/characteristics/national\",\"method\":\"POST\",\"body\":\"{ \\\"indicators\\\": [\\\"perm_excl_rate\\\",\\\"perm_excl\\\",\\\"fixed_excl_rate\\\" ], \\\"characteristics\\\": [ \\\"Total\\\" ], \\\"endYear\\\": 201617, \\\"publicationId\\\": \\\"bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9\\\", \\\"schoolTypes\\\": [ \\\"Total\\\" ], \\\"startYear\\\": 201213}\"},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"perm_excl_rate\",\"perm_excl\",\"fixed_excl_rate\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall rate of permanent exclusions has increased from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17 \\n * number of exclusions has also increased, from 6,685 to 7,720 \\n * overall rate of fixed period exclusions increased, from 4.29 per cent of pupil enrolments in 2015/16 to 4.76 per cent in 2016/17 \\n * number of exclusions has also increased, from 339,360 to 381,865. \\n\"}}}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataQuery\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall rate of permanent exclusions has increased from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17 \\n * number of exclusions has also increased, from 6,685 to 7,720 \\n * overall rate of fixed period exclusions increased, from 4.29 per cent of pupil enrolments in 2015/16 to 4.76 per cent in 2016/17 \\n * number of exclusions has also increased, from 339,360 to 381,865. \\n\"}}}");
        }
    }
}
