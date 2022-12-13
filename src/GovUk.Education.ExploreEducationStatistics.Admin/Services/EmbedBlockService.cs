#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EmbedBlockService : IEmbedBlockService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IContentBlockService _contentBlockService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public EmbedBlockService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IContentBlockService contentBlockService,
        IUserService userService,
        IMapper mapper)
    {
        _contentDbContext = contentDbContext;
        _persistenceHelper = persistenceHelper;
        _contentBlockService = contentBlockService;
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<Either<ActionResult, EmbedBlockLinkViewModel>> Create(Guid releaseId,
        EmbedBlockCreateRequest request)
    {
        return await _persistenceHelper
            .CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanUpdateRelease)
            .OnSuccess(async _ =>
            {
                return await _persistenceHelper
                    .CheckEntityExists<ContentSection>(request.ContentSectionId, query =>
                        query.Include(contentSection => contentSection.Content))
                    .OnSuccess<ActionResult, ContentSection, EmbedBlockLinkViewModel>(async contentSection =>
                    {
                        if (!_contentDbContext.ReleaseContentSections
                              .Any(rcs => rcs.ReleaseId == releaseId
                                          && rcs.ContentSectionId == request.ContentSectionId))
                        {
                            return ValidationUtils.ValidationActionResult(
                                ValidationErrorMessages.ContentSectionNotAttachedToRelease);
                        }

                        var embedBlock = new EmbedBlock
                        {
                            Id = Guid.NewGuid(),
                            Title = request.Title,
                            Url = request.Url,
                        };

                        var order = contentSection.Content.Any()
                            ? contentSection.Content.Max(contentBlock => contentBlock.Order) + 1
                            : 1;

                        var contentBlock = new EmbedBlockLink
                        {
                            ContentSectionId = request.ContentSectionId,
                            Order = order,
                            EmbedBlock = embedBlock,
                        };

                        // NOTE: No need to create a ReleaseContentBlock here - see EES-1568

                        _contentDbContext.EmbedBlocks.Add(embedBlock);
                        _contentDbContext.EmbedBlockLinks.Add(contentBlock);

                        await _contentDbContext.SaveChangesAsync();

                        return _mapper.Map<EmbedBlockLinkViewModel>(contentBlock);
                    });
            });
    }

    public async Task<Either<ActionResult, EmbedBlockLinkViewModel>> Update(Guid releaseId,
        Guid contentBlockId,
        EmbedBlockUpdateRequest request)
    {
        return await _persistenceHelper
            .CheckEntityExists<Release>(releaseId)
            .OnSuccess(_userService.CheckCanUpdateRelease)
            .OnSuccess(async release =>
            {
                return await _persistenceHelper
                    .CheckEntityExists<EmbedBlockLink>(contentBlockId, query =>
                        query.Include(cb => cb.EmbedBlock))
                    .OnSuccess<ActionResult, EmbedBlockLink, EmbedBlockLinkViewModel>(async contentBlock =>
                    {
                        if (!_contentDbContext.ReleaseContentSections
                            .Any(rcs => rcs.ReleaseId == release.Id
                            && rcs.ContentSectionId == contentBlock.ContentSectionId))
                        {
                            return ValidationUtils.ValidationActionResult(
                                ValidationErrorMessages.ContentSectionNotAttachedToRelease);
                        }

                        var embedBlock = contentBlock.EmbedBlock;

                        embedBlock.Title = request.Title;
                        embedBlock.Url = request.Url;

                        _contentDbContext.Update(embedBlock);
                        await _contentDbContext.SaveChangesAsync();

                        return _mapper.Map<EmbedBlockLinkViewModel>(contentBlock);
                    });
            });
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId, Guid contentBlockId)
    {
        return await _persistenceHelper
            .CheckEntityExists<Release>(releaseId)
            .OnSuccessDo(_userService.CheckCanUpdateRelease)
            .OnSuccessVoid(async release =>
            {
                return await _persistenceHelper
                    .CheckEntityExists<EmbedBlockLink>(contentBlockId, query
                        => query.Include(cb => cb.EmbedBlock))
                    .OnSuccess<ActionResult, EmbedBlockLink, Unit>(async contentBlock =>
                    {
                        if (!_contentDbContext.ReleaseContentSections
                            .Any(rcs => rcs.ReleaseId == release.Id
                            && rcs.ContentSectionId == contentBlock.ContentSectionId))
                        {
                            return ValidationUtils.ValidationActionResult(
                                ValidationErrorMessages.ContentBlockNotAttachedToRelease);
                        }

                        await _contentBlockService.DeleteContentBlockAndReorder(contentBlock.Id);

                        return Unit.Instance;
                    });
            });
    }
}
