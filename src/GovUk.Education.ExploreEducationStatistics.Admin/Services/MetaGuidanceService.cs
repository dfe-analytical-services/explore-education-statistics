using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class MetaGuidanceService : IMetaGuidanceService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IUserService _userService;

        public MetaGuidanceService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            StatisticsDbContext statisticsDbContext,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _statisticsDbContext = statisticsDbContext;
            _userService = userService;
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Get(Guid releaseId)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release => BuildViewModel(release, await GetSubjects(release.Id)));
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Update(Guid releaseId,
            MetaGuidanceUpdateViewModel request)
        {
            return await _persistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanUpdateRelease(release))
                .OnSuccess(async release =>
                {
                    _contentDbContext.Update(release);
                    release.MetaGuidance = request.Content;
                    await _contentDbContext.SaveChangesAsync();

                    return BuildViewModel(release, await GetSubjects(release.Id));
                });
        }

        private async Task<List<MetaGuidanceSubjectViewModel>> GetSubjects(Guid releaseId)
        {
            var dataFiles = await _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.ReleaseFileReference)
                .Where(rf => rf.ReleaseId == releaseId
                             && rf.ReleaseFileReference.ReleaseFileType == ReleaseFileTypes.Data
                             && rf.ReleaseFileReference.SubjectId.HasValue)
                .Select(rf => rf.ReleaseFileReference)
                .ToDictionaryAsync(f => f.SubjectId.Value, f => f);

            var subjects = await _statisticsDbContext
                .ReleaseSubject
                .Include(s => s.Subject)
                .Where(s => s.ReleaseId == releaseId)
                .Select(s => s.Subject)
                .ToListAsync();

            return (await Task.WhenAll(
                subjects
                    .OrderBy(subject => subject.Name)
                    .Select(async subject =>
                    {
                        var geographicLevels = await GetGeographicLevels(subject.Id);
                        var (start, end) = await GetSubjectTimePeriods(subject.Id);
                        return new MetaGuidanceSubjectViewModel
                        {
                            Content = subject.MetaGuidance,
                            Filename = dataFiles[subject.Id].Filename,
                            Name = subject.Name,
                            GeographicLevels = geographicLevels,
                            Start = start,
                            End = end
                        };
                    })
            )).ToList();
        }

        private async Task<(string start, string end)> GetSubjectTimePeriods(Guid subjectId)
        {
            var orderedObservations = _statisticsDbContext
                .Observation
                .Where(observation => observation.SubjectId == subjectId)
                .OrderBy(observation => observation.Year)
                .ThenBy(observation => observation.TimeIdentifier);

            var first = await orderedObservations.FirstAsync();
            var last = await orderedObservations.LastAsync();
            return (first.GetTimePeriod(), last.GetTimePeriod());
        }

        private async Task<List<GeographicLevel>> GetGeographicLevels(Guid subjectId)
        {
            return await _statisticsDbContext
                .Observation
                .Where(observation => observation.SubjectId == subjectId)
                .Select(observation => observation.GeographicLevel)
                .Distinct()
                .ToListAsync();
        }

        private static MetaGuidanceViewModel BuildViewModel(Release release,
            List<MetaGuidanceSubjectViewModel> subjects)
        {
            return new MetaGuidanceViewModel
            {
                Content = release.MetaGuidance,
                Subjects = subjects
            };
        }
    }
}