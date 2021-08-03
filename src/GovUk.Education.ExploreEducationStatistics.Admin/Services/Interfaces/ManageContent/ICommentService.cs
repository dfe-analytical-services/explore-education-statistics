#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface ICommentService
    {
        Task<Either<ActionResult, List<CommentViewModel>>> GetComments(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId);

        Task<Either<ActionResult, CommentViewModel>> AddComment(
            Guid releaseId, Guid contentSectionId, Guid contentBlockId, CommentSaveRequest saveRequest);

        Task<Either<ActionResult, CommentViewModel>> SetResolved(Guid commentId, bool resolve);

        Task<Either<ActionResult, CommentViewModel>> UpdateComment(Guid commentId,
            CommentSaveRequest saveRequest);

        Task<Either<ActionResult, bool>> DeleteComment(Guid commentId);
    }
}
