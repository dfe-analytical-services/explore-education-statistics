using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class LocationMetaViewModel
    {
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> LocalAuthority { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> LocalAuthorityDistrict { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> National { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> Region { get; set; }
    }
}