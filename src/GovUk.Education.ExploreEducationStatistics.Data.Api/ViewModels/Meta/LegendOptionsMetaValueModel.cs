namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class LegendOptionsMetaValueModel<OptionsType>
    {
        public string Hint { get; set; }
        public string Legend { get; set; }
        public OptionsType Options { get; set; }
    }
}