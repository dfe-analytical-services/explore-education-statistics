using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class LocationViewModel
    {
        public Country Country { get; set; }
        public Region Region { get; set; }
        public LocalAuthority LocalAuthority { get; set; }
        public LocalAuthorityDistrict LocalAuthorityDistrict { get; set; }
    }
}