using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1319UpdateDataBlockChartLegend : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                UPDATE ContentBlock
                SET DataBlock_Charts = JSON_MODIFY(
                   DataBlock_Charts,
                   '$[0].Legend',
                   JSON_QUERY(
                           FORMATMESSAGE(
                                   '{""Position"":%s,""Items"":[]}',
                                    IIF(
                                        JSON_VALUE(DataBlock_Charts, '$[0].Legend') IS NULL,
                                        'null',
                                        FORMATMESSAGE('""%s""', JSON_VALUE(DataBlock_Charts, '$[0].Legend'))
                                    )
                                ),
                        '$')
                    )
            ");

            migrationBuilder.Sql(
                @"
                UPDATE ContentBlock
                SET DataBlock_Charts = JSON_MODIFY(DataBlock_Charts, '$[0].LegendHeight', null);    
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                UPDATE ContentBlock
                SET DataBlock_Charts = JSON_MODIFY(DataBlock_Charts, '$[0].Legend', JSON_VALUE(DataBlock_Charts, '$[0].Legend.Position'));    
            ");

            migrationBuilder.Sql(
                @"
                UPDATE ContentBlock
                SET DataBlock_Charts = JSON_MODIFY(DataBlock_Charts, '$[0].LegendHeight', 0);    
            ");
        }
    }
}
