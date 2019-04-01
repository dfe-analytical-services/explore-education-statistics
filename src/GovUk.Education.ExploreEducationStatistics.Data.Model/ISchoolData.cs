namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public interface ISchoolData : ITidyData
    {
        School School { get; set; }
        
        string SchoolLaEstab { get; set; }
    }
}