namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public interface ISchoolData
    {
        School School { get; set; }
        
        string SchoolLaEstab { get; set; }
    }
}