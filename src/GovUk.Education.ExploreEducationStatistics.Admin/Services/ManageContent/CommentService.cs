#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class CommentService : ICommentService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public CommentService(ContentDbContext context,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IMapper mapper)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, List<CommentViewModel>>> GetComments(Guid releaseId,
            Guid contentSectionId,
            Guid contentBlockId)
        {
            return await CheckContentBlockExists(releaseId, contentSectionId, contentBlockId)
                .OnSuccess(async () =>
                    {
                        var comments = await _context.Comment
                            .AsNoTracking()
                            .Include(comment => comment.CreatedBy)
                            .Include(comment => comment.ResolvedBy)
                            .Where(comment => comment.ContentBlockId == contentBlockId)
                            .ToListAsync();

                        return _mapper.Map<List<CommentViewModel>>(comments);
                    }
                );
        }

        public Task<Either<ActionResult, CommentViewModel>> AddComment(Guid releaseId,
            Guid contentSectionId,
            Guid contentBlockId,
            CommentSaveRequest saveRequest)
        {
            return _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(() => CheckContentBlockExists(releaseId, contentSectionId, contentBlockId))
                .OnSuccess(async () =>
                    {
                        var saved = await _context.Comment.AddAsync(new Comment
                        {
                            ContentBlockId = contentBlockId,
                            Content = saveRequest.Content,
                            CreatedById = _userService.GetUserId()
                        });
                        await _context.SaveChangesAsync();

                        return await GetComment(saved.Entity.Id);
                    }
                );
        }

        public Task<Either<ActionResult, CommentViewModel>> SetResolved(Guid commentId, bool resolve)
        {
            return _persistenceHelper.CheckEntityExists<Comment>(commentId)
                .OnSuccessDo(_userService.CheckCanResolveComment)
                .OnSuccess(async comment =>
                {
                    _context.Comment.Update(comment);
                    comment.Resolved = resolve ? DateTime.UtcNow : null;
                    comment.ResolvedById = resolve ? _userService.GetUserId() : null;
                    await _context.SaveChangesAsync();
                    return await GetComment(comment.Id);
                });
        }

        public Task<Either<ActionResult, CommentViewModel>> UpdateComment(Guid commentId,
            CommentSaveRequest saveRequest)
        {
            return _persistenceHelper.CheckEntityExists<Comment>(commentId)
                .OnSuccess(_userService.CheckCanUpdateComment)
                .OnSuccess(async comment =>
                    {
                        _context.Comment.Update(comment);
                        comment.Content = saveRequest.Content;
                        await _context.SaveChangesAsync();
                        return await GetComment(comment.Id);
                    }
                );
        }

        public Task<Either<ActionResult, bool>> DeleteComment(Guid commentId)
        {
            return _persistenceHelper.CheckEntityExists<Comment>(commentId)
                .OnSuccess(_userService.CheckCanDeleteComment)
                .OnSuccess(async comment =>
                    {
                        _context.Comment.Remove(comment);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                );
        }

        private async Task<Either<ActionResult, Unit>> CheckContentBlockExists(
            Guid releaseId,
            Guid contentSectionId,
            Guid contentBlockId)
        {
            if (await _context.ContentBlocks
                    .AnyAsync(cb =>
                        cb.Id == contentBlockId &&
                        cb.ContentSection!.Id == contentSectionId &&
                        cb.ContentSection!.ReleaseId == releaseId))
            {
                return Unit.Instance;
            }

            return new NotFoundResult();
        }

        private async Task<Either<ActionResult, CommentViewModel>> GetComment(Guid commentId)
        {
            return await _context.Comment
                .AsNoTracking()
                .Include(c => c.CreatedBy)
                .Include(c => c.ResolvedBy)
                .FirstOrNotFoundAsync(comment => comment.Id == commentId)
                .OnSuccess(comment => _mapper.Map<CommentViewModel>(comment));
        }
    }
}
