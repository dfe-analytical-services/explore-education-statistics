#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IFilterRepository
{
    Task<List<Filter>> GetFiltersIncludingItems(Guid subjectId);
}
