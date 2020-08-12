using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUpdateChartFilesService
    {
        Task<Either<ActionResult, Unit>> UpdateChartFiles();
    }
}