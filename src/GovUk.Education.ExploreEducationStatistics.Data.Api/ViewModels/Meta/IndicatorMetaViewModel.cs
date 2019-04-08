namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class IndicatorMetaViewModel
    {
        public string Label { get; set; }
        // TODO DFE-412 Remove Name which was used by the original table tool
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Value { get; set; }
    }
}