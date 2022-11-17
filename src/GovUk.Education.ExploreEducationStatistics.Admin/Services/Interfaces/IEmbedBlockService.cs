#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using EmbedBlockLinkViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.EmbedBlockLinkViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEmbedBlockService
{
    Task<Either<ActionResult, EmbedBlockLinkViewModel>> Create(Guid releaseId, EmbedBlockCreateRequest request);

    Task<Either<ActionResult, EmbedBlockLinkViewModel>> Update(Guid releaseId, EmbedBlockUpdateRequest request);

    Task<Either<ActionResult, Unit>> Delete(Guid releaseId, Guid contentBlockId);
}
