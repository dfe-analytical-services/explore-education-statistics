#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface ITimePeriodService
{
    IList<(int Year, TimeIdentifier TimeIdentifier)> GetTimePeriodRange(IList<Observation> observations);

    Task<TimePeriodLabels> GetTimePeriodLabels(IStorageDataSet dataSet);
}
