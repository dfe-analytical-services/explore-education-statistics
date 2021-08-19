#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces
{
    public interface IMetaGuidanceService
    {
        public Task<Either<ActionResult, MetaGuidanceViewModel>> Get(string publicationPath, string releasePath);
    }
}