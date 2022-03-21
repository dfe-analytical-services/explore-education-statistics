﻿#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IUserInviteRepository
    {
        Task<UserInvite> Create(string email, Role role, Guid createdById);

        Task<UserInvite> Create(string email, string roleId, Guid createdById);
    }
}
