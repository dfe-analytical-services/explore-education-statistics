using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class ReleaseContentOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Content",
                value: "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\"},{\"Order\":2,\"Heading\":\"Absence rates\",\"Caption\":\"\"},{\"Order\":3,\"Heading\":\"Persistent absence\",\"Caption\":\"\"},{\"Order\":4,\"Heading\":\"Distribution of absence\",\"Caption\":\"\"},{\"Order\":5,\"Heading\":\"Absence for four year olds\",\"Caption\":\"\"},{\"Order\":6,\"Heading\":\"Pupil referral unit absence\",\"Caption\":\"\"},{\"Order\":7,\"Heading\":\"Pupil absence by local authority\",\"Caption\":\"\"}]");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                column: "Content",
                value: "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\"}]");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "Content",
                value: "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\"},{\"Order\":2,\"Heading\":\"Permanent exclusions\",\"Caption\":\"\"},{\"Order\":3,\"Heading\":\"Fixed-period exclusions\",\"Caption\":\"\"},{\"Order\":4,\"Heading\":\"Number and length of fixed-period exclusions\",\"Caption\":\"\"},{\"Order\":5,\"Heading\":\"Reasons for exclusions\",\"Caption\":\"\"},{\"Order\":6,\"Heading\":\"Exclusions by pupil; characteristics\",\"Caption\":\"\"},{\"Order\":7,\"Heading\":\"Independent exclusion reviews\",\"Caption\":\"\"},{\"Order\":8,\"Heading\":\"Exclusions from pupil referral units\",\"Caption\":\"\"},{\"Order\":9,\"Heading\":\"Exclusions by local authority\",\"Caption\":\"\"}]");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "Content",
                value: "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\"},{\"Order\":2,\"Heading\":\"Absence rates\",\"Caption\":\"\"},{\"Order\":3,\"Heading\":\"Persistent absence\",\"Caption\":\"\"},{\"Order\":4,\"Heading\":\"Distribution of absence\",\"Caption\":\"\"},{\"Order\":5,\"Heading\":\"Absence for four year olds\",\"Caption\":\"\"},{\"Order\":6,\"Heading\":\"Pupil referral unit absence\",\"Caption\":\"\"},{\"Order\":7,\"Heading\":\"Pupil absence by local authority\",\"Caption\":\"\"}]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                column: "Content",
                value: "[{\"Heading\":\"About this release\",\"Caption\":\"\"},{\"Heading\":\"Absence rates\",\"Caption\":\"\"},{\"Heading\":\"Persistent absence\",\"Caption\":\"\"},{\"Heading\":\"Distribution of absence\",\"Caption\":\"\"},{\"Heading\":\"Absence for four year olds\",\"Caption\":\"\"},{\"Heading\":\"Pupil referral unit absence\",\"Caption\":\"\"},{\"Heading\":\"Pupil absence by local authority\",\"Caption\":\"\"}]");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                column: "Content",
                value: "[{\"Heading\":\"About this release\",\"Caption\":\"\"},{\"Heading\":\"Permanent exclusions\",\"Caption\":\"\"},{\"Heading\":\"Fixed-period exclusions\",\"Caption\":\"\"},{\"Heading\":\"Number and length of fixed-period exclusions\",\"Caption\":\"\"},{\"Heading\":\"Reasons for exclusions\",\"Caption\":\"\"},{\"Heading\":\"Exclusions by pupil; characteristics\",\"Caption\":\"\"},{\"Heading\":\"Independent exclusion reviews\",\"Caption\":\"\"},{\"Heading\":\"Exclusions from pupil referral units\",\"Caption\":\"\"},{\"Heading\":\"Exclusions by local authority\",\"Caption\":\"\"}]");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                column: "Content",
                value: "[{\"Heading\":\"About this release\",\"Caption\":\"\"},{\"Heading\":\"Permanent exclusions\",\"Caption\":\"\"},{\"Heading\":\"Fixed-period exclusions\",\"Caption\":\"\"},{\"Heading\":\"Number and length of fixed-period exclusions\",\"Caption\":\"\"},{\"Heading\":\"Reasons for exclusions\",\"Caption\":\"\"},{\"Heading\":\"Exclusions by pupil; characteristics\",\"Caption\":\"\"},{\"Heading\":\"Independent exclusion reviews\",\"Caption\":\"\"},{\"Heading\":\"Exclusions from pupil referral units\",\"Caption\":\"\"},{\"Heading\":\"Exclusions by local authority\",\"Caption\":\"\"}]");

            migrationBuilder.UpdateData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                column: "Content",
                value: "[{\"Heading\":\"About this release\",\"Caption\":\"\"},{\"Heading\":\"Absence rates\",\"Caption\":\"\"},{\"Heading\":\"Persistent absence\",\"Caption\":\"\"},{\"Heading\":\"Distribution of absence\",\"Caption\":\"\"},{\"Heading\":\"Absence for four year olds\",\"Caption\":\"\"},{\"Heading\":\"Pupil referral unit absence\",\"Caption\":\"\"},{\"Heading\":\"Pupil absence by local authority\",\"Caption\":\"\"}]");
        }
    }
}
