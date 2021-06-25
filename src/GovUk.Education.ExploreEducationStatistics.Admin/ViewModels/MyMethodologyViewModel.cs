using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class MyMethodologyViewModel : MethodologyTitleViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }

        public PermissionsSet Permissions { get; set; }

        public class PermissionsSet
        {
            public bool CanUpdateMethodology { get; set; }
            public bool CanCancelMethodologyAmendment { get; set; }
            public bool CanMakeAmendmentOfMethodology { get; set; }
        }
    }
}
