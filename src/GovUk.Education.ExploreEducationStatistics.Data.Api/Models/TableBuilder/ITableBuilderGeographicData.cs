using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public interface ITableBuilderGeographicData : ITableBuilderData
    {
        Country Country { get; set; }
        Region Region { get; set; }
        LocalAuthority LocalAuthority { get; set; }
    }
}