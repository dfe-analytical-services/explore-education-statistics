namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    public abstract class AbstractChart : IContentBlockChart
    {
        public string Title { get; set; }
        public string Alt { get; set; }
        public int Height { get; set; }
        public int? Width { get; set; }
        
        public abstract ChartType Type { get; }
    }
}