#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IRepository<TEntity, in TKey> where TEntity : class
    {
        Task<Either<ActionResult, TEntity>> FindOrNotFoundAsync(TKey id);
    }
}
