using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class RenameTimePeriodsToTimePeriodInDataBlocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"183_461_____\":{\"Label\":\"Pupils with one or more exclusion\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"183\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                column: "DataBlock_Charts",
                value: "[{\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null}},\"Type\":\"map\",\"Title\":null,\"Width\":0,\"Height\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"179_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":[{\"Indicator\":\"179\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Exclusion Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"181_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"183_461_____\":{\"Label\":\"Pupils with one or more exclusion\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"181\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"183\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                column: "DataBlock_Charts",
                value: "[{\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null}},\"Type\":\"map\",\"Title\":null,\"Width\":0,\"Height\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"23_1_58_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"26_1_58_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"28_1_58_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"23\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"26\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null},{\"Indicator\":\"28\",\"Filters\":[\"1\",\"58\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Absence Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":\"top\",\"Labels\":{\"179_461_____\":{\"Label\":\"Fixed period exclusion rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":[{\"Indicator\":\"179\",\"Filters\":[\"461\"],\"Location\":null,\"TimePeriod\":null}],\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"School Year\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":null,\"Max\":null,\"Size\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriods\",\"DataSets\":null,\"ReferenceLines\":null,\"Visible\":true,\"Title\":\"Exclusion Rate\",\"ShowGrid\":true,\"LabelPosition\":\"axis\",\"Min\":0,\"Max\":null,\"Size\":null}},\"Type\":\"line\",\"Title\":null,\"Width\":0,\"Height\":0}]");
        }
    }
}
