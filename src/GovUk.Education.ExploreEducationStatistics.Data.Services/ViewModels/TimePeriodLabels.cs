namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class TimePeriodLabels
    {
        public string From { get; }
        public string To { get; }

        public TimePeriodLabels()
        {
        }

        public TimePeriodLabels(string from, string to)
        {
            From = from;
            To = to;
        }
    }
}