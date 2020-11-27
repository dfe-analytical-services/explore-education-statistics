using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1600RemoveDataBlockChartLabelsField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                UPDATE ContentBlock
                SET DataBlock_Charts = JSON_MODIFY(
                    DataBlock_Charts,
                    '$[0].Labels',
                    null
                );    
            ");
            
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Charts",
                value: "[{\"Type\":\"line\",\"Title\":null,\"Alt\":null,\"Height\":0,\"Width\":null,\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[{\"Indicator\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null}],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"School Year\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":null,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"Absence Rate\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":0,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null}},\"Legend\":{\"Position\":\"top\",\"Items\":[{\"DataSet\":{\"Indicator\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},\"Label\":\"Unauthorised absence rate\",\"Colour\":\"#4763a5\",\"Symbol\":\"circle\",\"LineStyle\":\"solid\"},{\"DataSet\":{\"Indicator\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},\"Label\":\"Overall absence rate\",\"Colour\":\"#f5a450\",\"Symbol\":\"cross\",\"LineStyle\":\"solid\"},{\"DataSet\":{\"Indicator\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},\"Label\":\"Authorised absence rate\",\"Colour\":\"#005ea5\",\"Symbol\":\"diamond\",\"LineStyle\":\"solid\"}]}}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                column: "DataBlock_Charts",
                value: "[{\"Type\":\"line\",\"Title\":null,\"Alt\":null,\"Height\":0,\"Width\":null,\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[{\"Indicator\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null}],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"School Year\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":null,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"Absence Rate\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":0,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null}},\"Legend\":{\"Position\":\"top\",\"Items\":[{\"DataSet\":{\"Indicator\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},\"Label\":\"Unauthorised absence rate\",\"Colour\":\"#4763a5\",\"Symbol\":\"circle\",\"LineStyle\":\"solid\"},{\"DataSet\":{\"Indicator\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},\"Label\":\"Overall absence rate\",\"Colour\":\"#f5a450\",\"Symbol\":\"cross\",\"LineStyle\":\"solid\"},{\"DataSet\":{\"Indicator\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},\"Label\":\"Authorised absence rate\",\"Colour\":\"#005ea5\",\"Symbol\":\"diamond\",\"LineStyle\":\"solid\"}]}}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                column: "DataBlock_Charts",
                value: "[]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":{\"Position\":\"top\",\"Items\":[]},\"Type\":\"line\",\"Title\":null,\"Alt\":null,\"Height\":0,\"Width\":null,\"Labels\":{\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"Symbol\":\"circle\",\"LineStyle\":\"solid\"},\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"Symbol\":\"cross\",\"LineStyle\":\"solid\"},\"f9ae4976-7cd3-4718-834a-09349b6eb377_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"Symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[{\"Indicator\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null}],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"School Year\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":null,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"Absence Rate\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":0,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null}}}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":{\"Position\":\"top\",\"Items\":[]},\"Type\":\"line\",\"Title\":null,\"Alt\":null,\"Height\":0,\"Width\":null,\"Labels\":{\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"Symbol\":\"circle\",\"LineStyle\":\"solid\"},\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"Symbol\":\"cross\",\"LineStyle\":\"solid\"},\"f9ae4976-7cd3-4718-834a-09349b6eb377_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"Symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[{\"Indicator\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null}],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"School Year\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":null,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"Absence Rate\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":0,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null}}}]");

            migrationBuilder.UpdateData(
                table: "ContentBlock",
                keyColumn: "Id",
                keyValue: new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                column: "DataBlock_Charts",
                value: "[{\"Legend\":{\"Position\":\"top\",\"Items\":[]},\"Type\":\"line\",\"Title\":null,\"Alt\":null,\"Height\":0,\"Width\":null,\"Labels\":{\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"Symbol\":\"cross\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[{\"Indicator\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null}],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"School Year\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":null,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"Absence Rate\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":0,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null}}}]");
        }
    }
}
