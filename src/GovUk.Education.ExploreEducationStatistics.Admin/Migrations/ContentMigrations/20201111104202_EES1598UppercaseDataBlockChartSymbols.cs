using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1598UppercaseDataBlockChartSymbols : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                UPDATE ContentBlock
                SET DataBlock_Charts = REPLACE(DataBlock_Charts, '""symbol"":', '""Symbol"":');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                UPDATE ContentBlock
                SET DataBlock_Charts = REPLACE(DataBlock_Charts, '""Symbol"":', '""symbol"":');
            ");
        }
    }
}
