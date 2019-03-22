namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public interface IGeographicData : ITidyData
    {
        Region Region { get; set; }
        
        string RegionCode { get; set; } 
        
        LocalAuthority LocalAuthority { get; set; }
        
        string LocalAuthorityCode { get; set; }
    }
}