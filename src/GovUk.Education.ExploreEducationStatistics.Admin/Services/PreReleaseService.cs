﻿using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PreReleaseService : IPreReleaseService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly AccessWindowOptions _preReleaseOptions;

        public PreReleaseService(IOptions<PreReleaseOptions> config,
            IPersistenceHelper<ContentDbContext> persistenceHelper)
        {
            _preReleaseOptions = config.Value.PreReleaseAccess.AccessWindow;
            _persistenceHelper = persistenceHelper;
        }

        public PreReleaseWindowStatus GetPreReleaseWindowStatus(Release release, DateTime referenceTime)
        {
            if (!release.PublishScheduled.HasValue)
            {
                return new PreReleaseWindowStatus
                {
                    PreReleaseAccess = PreReleaseAccess.NoneSet
                };
            }

            var publishDate = release.PublishScheduled.Value;
            var accessWindowStart = publishDate.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeStart);
            var accessWindowEnd = publishDate.AddMinutes(-_preReleaseOptions.MinutesBeforeReleaseTimeEnd);

            return new PreReleaseWindowStatus
            {
                PreReleaseWindowStartTime = DateTime.SpecifyKind(accessWindowStart, DateTimeKind.Utc),
                PreReleaseWindowEndTime = DateTime.SpecifyKind(accessWindowEnd, DateTimeKind.Utc),
                PreReleaseAccess = GetPreReleaseAccess(release, accessWindowStart, accessWindowEnd, referenceTime)
            };
        }

        private static PreReleaseAccess GetPreReleaseAccess(
            Release release,
            DateTime accessWindowStart,
            DateTime accessWindowEnd,
            DateTime referenceTime)
        {
            if (!release.PublishScheduled.HasValue || release.Status != ReleaseStatus.Approved)
            {
                return PreReleaseAccess.NoneSet;
            }

            if (referenceTime.CompareTo(accessWindowStart) < 0)
            {
                return PreReleaseAccess.Before;
            }

            if (referenceTime.CompareTo(accessWindowEnd) >= 0)
            {
                return PreReleaseAccess.After;
            }

            return PreReleaseAccess.Within;
        }

        public async Task<Either<ActionResult, PreReleaseSummaryViewModel>> GetPreReleaseSummaryViewModelAsync(
            Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId, queryable =>
                    queryable.Include(r => r.Publication)
                        .ThenInclude(publication => publication.Contact))
                .OnSuccess(release => new PreReleaseSummaryViewModel(
                    release.Publication.Slug,
                    release.Publication.Title,
                    release.Title,
                    release.Publication.Contact.TeamEmail));
        }
    }

    public class PreReleaseOptions
    {
        public PreReleaseAccessOptions PreReleaseAccess { get; set; }
    }

    public class AccessWindowOptions
    {
        public int MinutesBeforeReleaseTimeStart { get; set; }

        public int MinutesBeforeReleaseTimeEnd { get; set; }
    }

    public class PreReleaseAccessOptions
    {
        public AccessWindowOptions AccessWindow { get; set; }
    }
}