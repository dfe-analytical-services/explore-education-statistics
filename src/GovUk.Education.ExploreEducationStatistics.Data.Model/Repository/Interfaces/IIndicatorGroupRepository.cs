#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IIndicatorGroupRepository
{
    Task<List<IndicatorGroup>> GetIndicatorGroups(Guid subjectId);
}
