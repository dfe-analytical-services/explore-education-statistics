#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IIndicatorRepository
{
    IEnumerable<Indicator> GetIndicators(Guid subjectId, IEnumerable<Guid>? indicatorIds = null);
}
