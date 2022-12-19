#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<Either<ActionResult, UserViewModel>> GetUser(Guid id);

        Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers();

        Task<Either<ActionResult, List<IdTitleViewModel>>> ListReleases();

        Task<List<UserViewModel>> ListPreReleaseUsersAsync();

        Task<Either<ActionResult, List<PendingInviteViewModel>>> ListPendingInvites();

        Task<Either<ActionResult, UserInvite>> InviteUser(UserInviteCreateRequest request);

        Task<Either<ActionResult, Unit>> CancelInvite(string email);

        Task<Either<ActionResult, Unit>> UpdateUser(string userId, string roleId);
    }
}
