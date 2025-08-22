#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface ILocationRepository
{
    Task<IList<Location>> GetDistinctForSubject(Guid subjectId);
}
