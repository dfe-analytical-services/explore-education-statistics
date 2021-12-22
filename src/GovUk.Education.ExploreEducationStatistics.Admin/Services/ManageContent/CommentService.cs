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
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

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

        public Task<Either<ActionResult, List<CommentViewModel>>> GetComments(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId
        )
        {
            return CheckContentSectionExists(releaseId, contentSectionId)
                .OnSuccess(tuple =>
                {
                    var (_, section) = tuple;

                    var contentBlock = section.Content.Find(block => block.Id == contentBlockId);

                    if (contentBlock == null)
                    {
                        return NotFound<List<CommentViewModel>>();
                    }

                    return _mapper.Map<List<CommentViewModel>>(contentBlock.Comments);
                }
            );
        }

        public Task<Either<ActionResult, CommentViewModel>> AddComment(Guid releaseId,
            Guid contentSectionId,
            Guid contentBlockId,
            CommentSaveRequest saveRequest)
        {
            return CheckContentSectionExists(releaseId, contentSectionId)
                .OnSuccess(CheckCanUpdateRelease)
                .OnSuccess(async tuple =>
                {
                    var (_, section) = tuple;

                    var contentBlock = section.Content.Find(block => block.Id == contentBlockId);

                    if (contentBlock == null)
                    {
                        return ValidationResult<CommentViewModel>(ContentBlockNotFound);
                    }

                    var comment = new Comment
                    {
                        Id = new Guid(),
                        ContentBlockId = contentBlockId,
                        Content = saveRequest.Content,
                        Created = DateTime.UtcNow,
                        CreatedById = _userService.GetUserId()
                    };

                    await _context.Comment.AddAsync(comment);
                    await _context.SaveChangesAsync();
                    return await GetCommentAsync(comment.Id);
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
                    comment.Resolved = resolve ? DateTime.UtcNow : (DateTime?)null;
                    comment.ResolvedById = resolve ? _userService.GetUserId() : (Guid?)null;
                    await _context.SaveChangesAsync();
                    return await GetCommentAsync(comment.Id);
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
                        comment.Updated = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        return await GetCommentAsync(comment.Id);
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

        private Task<Either<ActionResult, CommentViewModel>> GetCommentAsync(Guid commentId)
        {
            return _persistenceHelper.CheckEntityExists<Comment>(commentId, queryable =>
                    queryable
                        .Include(comment => comment.CreatedBy)
                        .Include(comment => comment.ResolvedBy))
                .OnSuccess(comment => _mapper.Map<CommentViewModel>(comment));
        }

        private static IQueryable<Release> HydrateContentSectionsAndBlocks(IQueryable<Release> releases)
        {
            return releases
                .Include(r => r.Content)
                    .ThenInclude(join => join.ContentSection)
                    .ThenInclude(section => section.Content)
                    .ThenInclude(content => content.Comments)
                    .ThenInclude(comment => comment.CreatedBy)
                .Include(r => r.Content)
                    .ThenInclude(join => join.ContentSection)
                    .ThenInclude(section => section.Content)
                    .ThenInclude(content => content.Comments)
                    .ThenInclude(comment => comment.ResolvedBy);
        }

        private Task<Either<ActionResult, Tuple<Release, ContentSection>>> CheckContentSectionExists(
            Guid releaseId, Guid contentSectionId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId, HydrateContentSectionsAndBlocks)
                .OnSuccess(release =>
                {
                    var section = release
                        .Content
                        .Select(join => join.ContentSection)
                        .ToList()
                        .Find(contentSection => contentSection.Id == contentSectionId);

                    if (section == null)
                    {
                        return new NotFoundResult();
                    }

                    return new Either<ActionResult, Tuple<Release, ContentSection>>(
                        TupleOf(release, section));
                });
        }

        private Task<Either<ActionResult, Tuple<Release, ContentSection>>> CheckCanUpdateRelease(
            Tuple<Release, ContentSection> tuple)
        {
            return _userService
                .CheckCanUpdateRelease(tuple.Item1)
                .OnSuccess(_ => tuple);
        }
    }
}
