namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class GeographicData : TidyData, IGeographicSchoolData
    {
        public Region Region { get; set; }

        public string RegionCode { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public string LocalAuthorityCode { get; set; }
        
        public School School { get; set; }
        
        public string SchoolLaEstab { get; set; }
    }
}