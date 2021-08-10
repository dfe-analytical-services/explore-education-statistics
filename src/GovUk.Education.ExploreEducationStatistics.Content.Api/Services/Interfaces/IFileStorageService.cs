#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<Either<ActionResult, T?>> GetDeserialized<T>(string path)
            where T : class;
    }
}
