using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class AddReleaseIdToFootnotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReleaseId",
                table: "Footnote",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"
                UPDATE Footnote 
                SET Footnote.ReleaseId = Subject.ReleaseId 
                FROM Footnote
                JOIN FilterFootnote ON Footnote.Id = FilterFootnote.FootnoteId
                JOIN Filter ON Filter.Id = FilterFootnote.FilterId
                JOIN Subject ON Subject.Id = Filter.SubjectId");
            
            migrationBuilder.Sql(
                @"
                UPDATE Footnote 
                SET Footnote.ReleaseId = Subject.ReleaseId 
                FROM Footnote
                JOIN FilterGroupFootnote ON Footnote.Id = FilterGroupFootnote.FootnoteId
                JOIN FilterGroup ON FilterGroup.Id = FilterGroupFootnote.FilterGroupId
                JOIN Filter ON Filter.Id = FilterGroup.FilterId
                JOIN Subject ON Subject.Id = Filter.SubjectId");
            
            migrationBuilder.Sql(
                @"
                UPDATE Footnote 
                SET Footnote.ReleaseId = Subject.ReleaseId 
                FROM Footnote
                JOIN FilterItemFootnote ON Footnote.Id = FilterItemFootnote.FootnoteId
                JOIN FilterItem ON FilterItem.Id = FilterItemFootnote.FilterItemId
                JOIN FilterGroup ON FilterGroup.Id = FilterItem.FilterGroupId
                JOIN Filter ON Filter.Id = FilterGroup.FilterId
                JOIN Subject ON Subject.Id = Filter.SubjectId");
            
            migrationBuilder.Sql(
                @"
                UPDATE Footnote 
                SET Footnote.ReleaseId = Subject.ReleaseId 
                FROM Footnote
                JOIN IndicatorFootnote ON Footnote.Id = IndicatorFootnote.FootnoteId
                JOIN Indicator ON Indicator.Id = IndicatorFootnote.IndicatorId
                JOIN IndicatorGroup ON IndicatorGroup.Id = Indicator.IndicatorGroupId
                JOIN Subject ON Subject.Id = IndicatorGroup.SubjectId");
            
            migrationBuilder.Sql(
                @"
                UPDATE Footnote 
                SET Footnote.ReleaseId = Subject.ReleaseId 
                FROM Footnote
                JOIN SubjectFootnote ON Footnote.Id = SubjectFootnote.FootnoteId
                JOIN Subject ON Subject.Id = SubjectFootnote.SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Footnote_ReleaseId",
                table: "Footnote",
                column: "ReleaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Footnote_Release_ReleaseId",
                table: "Footnote",
                column: "ReleaseId",
                principalTable: "Release",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Footnote_Release_ReleaseId",
                table: "Footnote");

            migrationBuilder.DropIndex(
                name: "IX_Footnote_ReleaseId",
                table: "Footnote");

            migrationBuilder.DropColumn(
                name: "ReleaseId",
                table: "Footnote");
        }
    }
}
