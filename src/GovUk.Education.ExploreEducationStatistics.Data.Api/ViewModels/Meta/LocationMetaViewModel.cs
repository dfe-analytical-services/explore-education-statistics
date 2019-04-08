namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class LocationMetaViewModel
    {
        public LabelOptionsMetaValueModel<LabelValueViewModel> LocalAuthority { get; set; }
        public LabelOptionsMetaValueModel<LabelValueViewModel> National { get; set; }
        public LabelOptionsMetaValueModel<LabelValueViewModel> Region { get; set; }
        public LabelOptionsMetaValueModel<LabelValueViewModel> School { get; set; }
    }
}