﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserReleaseRoleRepository : IUserReleaseRoleRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public UserReleaseRoleRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<UserReleaseRole> Create(Guid userId, Guid releaseId, ReleaseRole role, Guid createdById)
        {
            var userReleaseRole = new UserReleaseRole
            {
                UserId = userId,
                ReleaseId = releaseId,
                Role = role,
                Created = DateTime.UtcNow,
                CreatedById = createdById,
            };

            var created =
                (await _contentDbContext.UserReleaseRoles.AddAsync(userReleaseRole)).Entity;
            await _contentDbContext.SaveChangesAsync();
            return created;
        }

        public async Task<Unit> CreateAll(List<Guid> userIds, Guid releaseId, ReleaseRole role,
            Guid createdById)
        {
            var userIdsAlreadyHaveRole = await _contentDbContext.UserReleaseRoles
                .AsAsyncEnumerable()
                .Where(urr =>
                    urr.ReleaseId == releaseId
                    && urr.Role == role
                    && userIds.Contains(urr.UserId))
                .Select(urr => urr.UserId)
                .ToListAsync();

            var userIdsToAdd = userIds.Except(userIdsAlreadyHaveRole).ToList();

            var newUserReleaseRoles = userIdsToAdd
                .Select(userId =>
                    new UserReleaseRole
                    {
                        UserId = userId,
                        ReleaseId = releaseId,
                        Role = role,
                        Created = DateTime.UtcNow,
                        CreatedById = createdById,
                    }
                ).ToList();

            await _contentDbContext.UserReleaseRoles.AddRangeAsync(newUserReleaseRoles);
            await _contentDbContext.SaveChangesAsync();
            return Unit.Instance;
        }

        public async Task Remove(UserReleaseRole userReleaseRole, Guid deletedById)
        {
            userReleaseRole.Deleted = DateTime.UtcNow;
            userReleaseRole.DeletedById = deletedById;
            _contentDbContext.Update(userReleaseRole);
            await _contentDbContext.SaveChangesAsync();
        }

        public async Task RemoveAll(List<UserReleaseRole> userReleaseRoles, Guid deletedById)
        {
            userReleaseRoles.ForEach(userReleaseRole =>
            {
                userReleaseRole.Deleted = DateTime.UtcNow;
                userReleaseRole.DeletedById = deletedById;
            });
            _contentDbContext.UpdateRange(userReleaseRoles);
            await _contentDbContext.SaveChangesAsync();
        }

        public async Task RemoveAllUserReleaseRolesForPublication(Guid userId, Publication publication, ReleaseRole role, Guid deletedById)
        {
            _contentDbContext.Update(publication);
            await _contentDbContext
                .Entry(publication)
                .Collection(p => p.Releases)
                .LoadAsync();
            var allLatestReleaseVersionIds = publication
                .GetLatestVersionsOfAllReleases()
                .Select(r => r.Id)
                .ToList();
            var userReleaseRoles = _contentDbContext.UserReleaseRoles
                .AsQueryable()
                .Where(urr =>
                    urr.UserId == userId
                    && allLatestReleaseVersionIds.Contains(urr.ReleaseId)
                    && urr.Role == role)
                .ToList();
            await RemoveAll(userReleaseRoles, deletedById);
        }

        public async Task<List<ReleaseRole>> GetAllRolesByUser(Guid userId, Guid releaseId)
        {
            return await _contentDbContext.UserReleaseRoles
                .AsQueryable()
                .Where(r =>
                    r.UserId == userId &&
                    r.ReleaseId == releaseId)
                .Select(r => r.Role)
                .ToListAsync();
        }

        public async Task<bool> IsUserApproverOnLatestRelease(Guid userId, Guid publicationId)
        {
            return await UserHasAnyOfRolesOnLatestRelease(
                userId,
                publicationId,
                ApproverRoles);
        }

        public async Task<bool> IsUserEditorOrApproverOnLatestRelease(Guid userId, Guid publicationId)
        {
            return await UserHasAnyOfRolesOnLatestRelease(
                userId,
                publicationId,
                EditorAndApproverRoles);
        }

        public async Task<bool> UserHasRoleOnRelease(Guid userId, Guid releaseId, ReleaseRole role)
        {
            return await _contentDbContext.UserReleaseRoles
                .AsQueryable()
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.ReleaseId == releaseId &&
                    r.Role == role);
        }

        public async Task<bool> UserHasAnyOfRolesOnLatestRelease(Guid userId,
            Guid publicationId,
            IEnumerable<ReleaseRole> roles)
        {
            var publication = await _contentDbContext.Publications
                .Include(p => p.Releases)
                .SingleAsync(p => p.Id == publicationId);

            var latestRelease = publication.LatestRelease();

            // Publication may have no releases
            if (latestRelease == null)
            {
                return false;
            }

            return await _contentDbContext.UserReleaseRoles
                .AsQueryable()
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.ReleaseId == latestRelease.Id &&
                    roles.Contains(r.Role));
        }
    }
}
