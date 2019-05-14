using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class LocationMetaViewModel
    {
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> LocalAuthority { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> LocalAuthorityDistrict { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> LocalEnterprisePartnership { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> Institution { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> Mat { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> MayoralCombinedAuthority { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> OpportunityArea { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> ParliamentaryConstituency { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> Provider { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> Ward { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> National { get; set; }
        public LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>> Region { get; set; }
    }
}