using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1816MoveSubjectNameFromSubjectTableToReleaseSubjectTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubjectName",
                table: "ReleaseSubject",
                defaultValue: "");
            
            migrationBuilder.Sql(
                "UPDATE ReleaseSubject " +
                "SET ReleaseSubject.SubjectName = Subject.Name " +
                "FROM ReleaseSubject JOIN Subject ON Subject.Id = ReleaseSubject.SubjectId " +
                "WHERE ReleaseSubject.SubjectId = Subject.Id");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Subject");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Subject",
                type: "nvarchar(max)",
                nullable: true);
            
            migrationBuilder.Sql(
                "UPDATE Subject " +
                "SET Subject.Name = ReleaseSubject.SubjectName " +
                "FROM Subject JOIN ReleaseSubject ON Subject.Id = ReleaseSubject.SubjectId " +
                "WHERE ReleaseSubject.SubjectId = Subject.Id");

            migrationBuilder.DropColumn(
                name: "SubjectName",
                table: "ReleaseSubject");
        }
    }
}
