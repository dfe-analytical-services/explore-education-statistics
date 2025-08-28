#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EmbedBlockService : IEmbedBlockService
{
    /// <summary>
    /// Whitelist of permitted domains from which to embed URLs in Release content.
    /// </summary>
    /// <remarks>
    /// Note that if amending this list, the corresponding changes must also be made to the public site's CSP
    /// directive configuration in explore-education-statistics-frontend/server.js.
    /// </remarks>
    public static readonly string[] PermittedDomains =
    {
        "https://department-for-education.shinyapps.io",
        "https://dfe-analytical-services.github.io"
    };

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

    public async Task<Either<ActionResult, EmbedBlockLinkViewModel>> Create(
        Guid releaseVersionId,
        EmbedBlockCreateRequest request)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ => _persistenceHelper.CheckEntityExists<ContentSection>(
                request.ContentSectionId,
                query => query.Include(contentSection => contentSection.Content)))
            .OnSuccessDo(_ => ValidateContentSectionAttachedToReleaseVersion(releaseVersionId: releaseVersionId,
                contentSectionId: request.ContentSectionId))
            .OnSuccessDo(_ => ValidateEmbedUrl(request.Url))
            .OnSuccess(async contentSection =>
            {
                var embedBlock = new EmbedBlock
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Url = request.Url
                };

                var order = contentSection.Content.Any()
                    ? contentSection.Content.Max(contentBlock => contentBlock.Order) + 1
                    : 1;

                var contentBlock = new EmbedBlockLink
                {
                    ContentSectionId = request.ContentSectionId,
                    Order = order,
                    EmbedBlock = embedBlock,
                    ReleaseVersionId = releaseVersionId
                };

                _contentDbContext.EmbedBlocks.Add(embedBlock);
                _contentDbContext.EmbedBlockLinks.Add(contentBlock);

                await _contentDbContext.SaveChangesAsync();

                return _mapper.Map<EmbedBlockLinkViewModel>(contentBlock);
            });
    }

    public async Task<Either<ActionResult, EmbedBlockLinkViewModel>> Update(Guid releaseVersionId,
        Guid contentBlockId,
        EmbedBlockUpdateRequest request)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ => _persistenceHelper.CheckEntityExists<EmbedBlockLink>(
                contentBlockId,
                query => query.Include(cb => cb.EmbedBlock)))
            .OnSuccessDo(contentBlock => ValidateContentSectionAttachedToReleaseVersion(
                releaseVersionId: releaseVersionId,
                contentSectionId: contentBlock.ContentSectionId!.Value))
            .OnSuccessDo(_ => ValidateEmbedUrl(request.Url))
            .OnSuccess(async contentBlock =>
            {
                var embedBlock = contentBlock.EmbedBlock;

                embedBlock.Title = request.Title;
                embedBlock.Url = request.Url;

                _contentDbContext.Update(embedBlock);
                await _contentDbContext.SaveChangesAsync();

                return _mapper.Map<EmbedBlockLinkViewModel>(contentBlock);
            });
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId, Guid contentBlockId)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccessDo(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccessVoid(async releaseVersion =>
            {
                return await _persistenceHelper
                    .CheckEntityExists<EmbedBlockLink>(contentBlockId, query
                        => query.Include(block => block.EmbedBlock))
                    .OnSuccess<ActionResult, EmbedBlockLink, Unit>(async contentBlock =>
                    {
                        if (!_contentDbContext.ContentSections
                                .Any(section => section.ReleaseVersionId == releaseVersion.Id
                                                && section.Id == contentBlock.ContentSectionId))
                        {
                            return ValidationActionResult(
                                ContentBlockNotAttachedToRelease);
                        }

                        await _contentBlockService.DeleteContentBlockAndReorder(contentBlock.Id);

                        return Unit.Instance;
                    });
            });
    }

    private Either<ActionResult, Unit> ValidateEmbedUrl(string url)
    {
        return PermittedDomains.Any(url.StartsWith)
            ? Unit.Instance
            : ValidationActionResult(EmbedBlockUrlDomainNotPermitted);
    }

    private Either<ActionResult, Unit> ValidateContentSectionAttachedToReleaseVersion(
        Guid releaseVersionId,
        Guid contentSectionId)
    {
        return _contentDbContext
            .ContentSections
            .Any(section => section.ReleaseVersionId == releaseVersionId && section.Id == contentSectionId)
            ? Unit.Instance
            : ValidationActionResult(ContentSectionNotAttachedToRelease);
    }
}
