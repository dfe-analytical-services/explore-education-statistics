﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserRoleService
    {
        Task<Either<ActionResult, Unit>> AddGlobalRole(string userId, string roleId);

        Task<Either<ActionResult, Unit>> AddPublicationRole(Guid userId, Guid publicationId, PublicationRole role);

        Task<Either<ActionResult, Unit>> AddReleaseRole(Guid userId, Guid releaseId, ReleaseRole role);

        Task<Either<ActionResult, List<RoleViewModel>>> GetAllGlobalRoles();

        Task<Either<ActionResult, Dictionary<string, List<string>>>> GetAllResourceRoles();

        Task<Either<ActionResult, List<RoleViewModel>>> GetGlobalRoles(string userId);

        Task<Either<ActionResult, List<UserPublicationRoleViewModel>>> GetPublicationRoles(Guid userId);

        Task<Either<ActionResult, List<UserReleaseRoleViewModel>>> GetReleaseRoles(Guid userId);

        Task<Either<ActionResult, Unit>> RemoveGlobalRole(string userId, string roleId);

        Task<Either<ActionResult, Unit>> RemoveUserPublicationRole(Guid id);

        Task<Either<ActionResult, Unit>> RemoveUserReleaseRole(Guid userReleaseRoleId);
    }
}
