#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;

public class MethodologyAmendmentService : IMethodologyAmendmentService
{
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IUserService _userService;
    private readonly IMethodologyService _methodologyService;
    private readonly ContentDbContext _context;

    public MethodologyAmendmentService(
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IUserService userService,
        IMethodologyService methodologyService,
        ContentDbContext context)
    {
        _persistenceHelper = persistenceHelper;
        _userService = userService;
        _methodologyService = methodologyService;
        _context = context;
    }

    public async Task<Either<ActionResult, MethodologyVersionViewModel>> CreateMethodologyAmendment(
        Guid originalMethodologyVersionId)
    {
        return await _persistenceHelper.CheckEntityExists<MethodologyVersion>(originalMethodologyVersionId)
            .OnSuccess(_userService.CheckCanMakeAmendmentOfMethodology)
            .OnSuccess(HydrateMethodologyVersionForAmendment)
            .OnSuccess(CreateAndSaveAmendment)
            .OnSuccessDo(LinkOriginalMethodologyFilesToAmendment)
            .OnSuccess(amendment =>
                _methodologyService.BuildMethodologyVersionViewModel(amendment));
    }

    private async Task<Either<ActionResult, MethodologyVersion>> CreateAndSaveAmendment(
        MethodologyVersion originalMethodologyVersion)
    {
        var amendment = CreateMethodologyAmendment(originalMethodologyVersion, _userService.GetUserId());
        var savedAmendment = (await _context.MethodologyVersions.AddAsync(amendment)).Entity;
        await _context.SaveChangesAsync();
        return savedAmendment;
    }

    public MethodologyVersion CreateMethodologyAmendment(
        MethodologyVersion originalMethodologyVersion,
        Guid createdByUserId)
    {
        var methodologyVersionAmendmentId = Guid.NewGuid();

        var amendment = new MethodologyVersion
        {
            // Assign a new Id.
            Id = methodologyVersionAmendmentId,

            // Assign this new MethodologyVersion to the same parent as the original.
            MethodologyId = originalMethodologyVersion.MethodologyId,

            // Copy certain fields from the original.
            AlternativeTitle = originalMethodologyVersion.AlternativeTitle,
            AlternativeSlug = originalMethodologyVersion.AlternativeSlug,

            // Assign new values to fields specifically for this amendment.
            Status = MethodologyApprovalStatus.Draft,
            Version = originalMethodologyVersion.Version + 1,
            PreviousVersionId = originalMethodologyVersion.Id,
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,

            MethodologyContent = CopyMethodologyContent(originalMethodologyVersion, createdByUserId),
            Notes = CopyNotes(originalMethodologyVersion, methodologyVersionAmendmentId, createdByUserId),
            CreatedById = createdByUserId
        };

        return amendment;
    }

    private List<MethodologyNote> CopyNotes(
        MethodologyVersion originalMethodologyVersion,
        Guid methodologyVersionAmendmentId,
        Guid createdByUserId)
    {
        return originalMethodologyVersion
            .Notes
            .Select(originalNote => new MethodologyNote
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Copy certain fields from the original.
                Content = originalNote.Content,
                DisplayDate = originalNote.DisplayDate,

                // Link it to the new MethodologyVersion amendment.
                MethodologyVersionId = methodologyVersionAmendmentId,

                CreatedById = createdByUserId,
            })
            .ToList();
    }

    private MethodologyVersionContent CopyMethodologyContent(
        MethodologyVersion originalMethodologyVersion,
        Guid methodologyVersionAmendmentId)
    {
        return new MethodologyVersionContent
        {
            MethodologyVersionId = methodologyVersionAmendmentId,
            Annexes = CopyContentSections(originalMethodologyVersion.MethodologyContent.Annexes),
            Content = CopyContentSections(originalMethodologyVersion.MethodologyContent.Content),
        };
    }

    private List<ContentSection> CopyContentSections(List<ContentSection> originalContentSections)
    {
        return originalContentSections
            .Select(originalContentSection => new ContentSection
            {
                // Assign a new Id.
                Id = Guid.NewGuid(),

                // Copy certain fields from the original.
                Heading = originalContentSection.Heading,
                Order = originalContentSection.Order,
                Type = originalContentSection.Type,

                // Copy the ContentBlocks.
                Content = CopyContentBlocks(originalContentSection.Content),
            })
            .ToList();
    }

    private List<ContentBlock> CopyContentBlocks(List<ContentBlock> originalContentBlocks)
    {
        return originalContentBlocks
            .Select<ContentBlock, ContentBlock>(originalContentBlock =>
            {
                if (originalContentBlock is HtmlBlock originalHtmlBlock)
                {
                    return new HtmlBlock
                    {
                        // Assign a new Id.
                        Id = Guid.NewGuid(),

                        // Copy certain fields from the original.
                        Body = originalHtmlBlock.Body,
                        Order = originalHtmlBlock.Order,

                        // Manually set the created date, as Methodology content is currently saved in JSON and
                        // therefore not able to make use of the ICreated interface's automatic setting of
                        // created dates.
                        Created = DateTime.UtcNow
                    };
                }

                throw new ArgumentException(
                    $"Unknown {nameof(ContentBlockType)} value {originalContentBlock.GetType()} during amendment");
            })
            .ToList();
    }

    private async Task<Either<ActionResult, Unit>> LinkOriginalMethodologyFilesToAmendment(
        MethodologyVersion methodologyVersion)
    {
        var originalFiles = await _context
            .MethodologyFiles
            .AsQueryable()
            .Where(f => f.MethodologyVersionId == methodologyVersion.PreviousVersionId)
            .ToListAsync();

        var fileCopies = originalFiles
            .Select(f => new MethodologyFile
            {
                FileId = f.FileId,
                MethodologyVersionId = methodologyVersion.Id
            });

        await _context.AddRangeAsync(fileCopies);
        await _context.SaveChangesAsync();
        return Unit.Instance;
    }

    private async Task<Either<ActionResult, MethodologyVersion>> HydrateMethodologyVersionForAmendment(
        MethodologyVersion methodologyVersion)
    {
        await _context
            .Entry(methodologyVersion)
            .Collection(m => m.Notes)
            .LoadAsync();

        await _context
            .Entry(methodologyVersion)
            .Reference(m => m.Methodology)
            .LoadAsync();

        await _context
            .Entry(methodologyVersion)
            .Reference(m => m.MethodologyContent)
            .LoadAsync();

        return methodologyVersion;
    }
}
