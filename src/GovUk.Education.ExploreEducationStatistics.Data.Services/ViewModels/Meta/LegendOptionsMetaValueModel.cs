namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class LegendOptionsMetaValueModel<TOptionsType>
    {
        public string Hint { get; set; }
        public string Legend { get; set; }
        public TOptionsType Options { get; set; }
        public string Name { get; set; }
    }
}