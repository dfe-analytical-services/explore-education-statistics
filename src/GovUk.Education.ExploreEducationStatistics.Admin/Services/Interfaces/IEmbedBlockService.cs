#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using EmbedBlockLinkViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.EmbedBlockLinkViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEmbedBlockService
{
    Task<Either<ActionResult, EmbedBlockLinkViewModel>> Create(Guid releaseId, EmbedBlockCreateRequest request);

    Task<Either<ActionResult, EmbedBlockLinkViewModel>> Update(Guid releaseId, Guid contentBlockId, EmbedBlockUpdateRequest request);

    Task<Either<ActionResult, Unit>> Delete(Guid releaseId, Guid contentBlockId);
}
