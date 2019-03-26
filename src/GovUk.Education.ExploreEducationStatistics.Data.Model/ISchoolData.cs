namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public interface ISchoolData
    {
        School School { get; set; }
        
        string SchoolLaEstab { get; set; }
    }
}