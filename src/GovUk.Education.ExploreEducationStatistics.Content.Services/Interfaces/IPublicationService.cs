#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IPublicationService
{
    public Task<Either<ActionResult, PublicationViewModel>> Get(string publicationSlug);

    public Task<Either<ActionResult, PublicationViewModel>> GetCachedPublication(string publicationSlug);

    public Task<Either<ActionResult, PublicationViewModel>> UpdateCachedPublication(string publicationSlug);
}
