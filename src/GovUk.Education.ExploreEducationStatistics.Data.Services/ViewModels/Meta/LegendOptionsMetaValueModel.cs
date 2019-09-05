namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class LegendOptionsMetaValueModel<OptionsType>
    {
        public string Hint { get; set; }
        public string Legend { get; set; }
        public OptionsType Options { get; set; }
    }
}