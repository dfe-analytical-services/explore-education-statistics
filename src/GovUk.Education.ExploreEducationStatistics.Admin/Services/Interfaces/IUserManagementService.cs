using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using EnumExtensions = GovUk.Education.ExploreEducationStatistics.Common.Extensions.EnumExtensions;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<Either<ActionResult, UserViewModel>> GetUser(Guid id);

        Task<Either<ActionResult, Dictionary<string, List<string>>>> GetResourceRoles();

        Task<Either<ActionResult, List<UserViewModel>>> ListAllUsers();

        Task<Either<ActionResult, Unit>> AddPublicationRole(Guid userId, Guid publicationId, PublicationRole role);

        Task<Either<ActionResult, Unit>> AddReleaseRole(Guid userId, Guid releaseId, ReleaseRole role);

        Task<Either<ActionResult, List<TitleAndIdViewModel>>> ListPublications();

        Task<Either<ActionResult, List<TitleAndIdViewModel>>> ListReleases();

        Task<Either<ActionResult, List<RoleViewModel>>> ListRoles();

        Task<Either<ActionResult, List<EnumExtensions.EnumValue>>> ListReleaseRoles();
        
        Task<List<UserViewModel>> ListPreReleaseUsersAsync();
        
        Task<Either<ActionResult, List<UserViewModel>>> ListPendingInvites();

        Task<Either<ActionResult, UserInvite>> InviteUser(string email, string user, string roleId);
        
        Task<Either<ActionResult, Unit>> CancelInvite(string email);

        Task<Either<ActionResult, Unit>> RemoveUserPublicationRole(Guid id);

        Task<Either<ActionResult, Unit>> RemoveUserReleaseRole(Guid id);

        Task<Either<ActionResult, Unit>> UpdateUser(string userId, string roleId);
    }
}
