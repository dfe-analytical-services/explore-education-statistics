namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public interface IGeographicData : ITidyData
    {
        Region Region { get; set; }
        LocalAuthority LocalAuthority { get; set; }
        School School { get; set; }
    }
}