using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public interface ITableBuilderData
    {
        int Year { get; set; }
        TimePeriod TimePeriod { get; set; }
    }
}