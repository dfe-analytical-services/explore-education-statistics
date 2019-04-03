namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class GeographicData : TidyData, ISchoolData
    {
        public School School { get; set; }
        
        public string SchoolLaEstab { get; set; }
    }
}