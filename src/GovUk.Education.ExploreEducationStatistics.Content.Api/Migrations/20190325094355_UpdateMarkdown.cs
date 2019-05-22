using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class UpdateMarkdown : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":\"Latest headline facts and figures - 2016 to 2017\",\"DataQuery\":{\"path\":\"/api/tablebuilder/characteristics/national\",\"method\":\"POST\",\"body\":\"{ \\\"indicators\\\": [\\\"enrolments\\\",\\\"sess_authorised\\\",\\\"sess_overall\\\",\\\"enrolments_PA_10_exact\\\",\\\"sess_unauthorised_percent\\\",\\\"enrolments_pa_10_exact_percent\\\",\\\"sess_authorised_percent\\\",\\\"sess_overall_percent\\\" ], \\\"characteristics\\\": [ \\\"Total\\\" ], \\\"endYear\\\": 201617, \\\"publicationId\\\": \\\"cbbd299f-8297-44bc-92ac-558bcf51f8ad\\\", \\\"schoolTypes\\\": [ \\\"Total\\\" ], \\\"startYear\\\": 201213}\"},\"Charts\":[{\"Indicators\":[\"sess_overall_percent\",\"sess_unauthorised_percent\",\"sess_authorised_percent\"],\"XAxis\":{\"title\":\"School Year\"},\"YAxis\":{\"title\":\"Absence Rate\"},\"Type\":\"line\"}],\"Summary\":{\"dataKeys\":[\"sess_overall_percent\",\"sess_authorised_percent\",\"sess_unauthorised_percent\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * pupils missed on average 8.2 school days \\n * overall and unauthorised absence rates up on previous year \\n * unauthorised rise due to higher rates of unauthorised holidays \\n * 10% of pupils persistently absent during 2016/17\"}}}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "KeyStatistics",
                value: "{\"Type\":\"DataBlock\",\"Heading\":\"Latest headline facts and figures - 2016 to 2017\",\"DataQuery\":{\"path\":\"/api/tablebuilder/characteristics/national\",\"method\":\"POST\",\"body\":\"{ \\\"indicators\\\": [\\\"enrolments\\\",\\\"sess_authorised\\\",\\\"sess_overall\\\",\\\"enrolments_PA_10_exact\\\",\\\"sess_unauthorised_percent\\\",\\\"enrolments_pa_10_exact_percent\\\",\\\"sess_authorised_percent\\\",\\\"sess_overall_percent\\\" ], \\\"characteristics\\\": [ \\\"Total\\\" ], \\\"endYear\\\": 201617, \\\"publicationId\\\": \\\"cbbd299f-8297-44bc-92ac-558bcf51f8ad\\\", \\\"schoolTypes\\\": [ \\\"Total\\\" ], \\\"startYear\\\": 201213}\"},\"Charts\":[{\"Indicators\":[\"sess_overall_percent\",\"sess_unauthorised_percent\",\"sess_authorised_percent\"],\"XAxis\":{\"title\":\"School Year\"},\"YAxis\":{\"title\":\"Absence Rate\"},\"Type\":\"line\"}],\"Summary\":{\"dataKeys\":[\"sess_overall_percent\",\"sess_authorised_percent\",\"sess_unauthorised_percent\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}}}");
        }
    }
}
