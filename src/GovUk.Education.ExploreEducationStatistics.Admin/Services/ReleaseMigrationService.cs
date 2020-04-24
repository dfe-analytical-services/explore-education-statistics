﻿using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    
    // TODO BAU-405 - temporary code to help seed the Release-File tables from Blob storage 
    public class ReleaseMigrationService : IReleaseMigrationService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IFileStorageService _fileStorageService;

        public ReleaseMigrationService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IFileStorageService fileStorageService)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _fileStorageService = fileStorageService;
        }

        public async Task<Either<ActionResult, bool>> PopulateReleaseAmendmentTables()
        {
            var releaseIds = _contentDbContext
                .Releases
                .Select(r => r.Id)
                .ToList();
            
            foreach (var releaseId in releaseIds)
            {
                var result = await PopulateReleaseAmendmentTables(releaseId);

                if (result.IsLeft)
                {
                    return false;
                }
            }

            return true;
        }

        public Task<Either<ActionResult, Release>> PopulateReleaseAmendmentTables(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanRunReleaseMigrations)
                .OnSuccess(PopulateFileTables)
                .OnSuccess(PopulateReleaseSubjectTable);
        }

        private async Task<Either<ActionResult, Release>> PopulateFileTables(Release release)
        {
            var dataFiles = (await _fileStorageService
                    .ListFilesFromBlobStorage(release.Id, ReleaseFileTypes.Data))
                .Where(f => f.MetaFileName?.Length > 0)
                .Select(f => (f, ReleaseFileTypes.Data));

            var metadataFiles = (await _fileStorageService
                    .ListFilesFromBlobStorage(release.Id, ReleaseFileTypes.Data))
                .Where(f => f.MetaFileName?.Length == 0)
                .Select(f => (f, ReleaseFileTypes.Metadata));

            var ancillaryFiles = (await _fileStorageService
                    .ListFilesFromBlobStorage(release.Id, ReleaseFileTypes.Ancillary))
                .Select(f => (f, ReleaseFileTypes.Ancillary));

            var chartFiles = (await _fileStorageService
                    .ListFilesFromBlobStorage(release.Id, ReleaseFileTypes.Chart))
                .Select(f => (f, ReleaseFileTypes.Chart));

            var allFiles = dataFiles
                .Concat(metadataFiles)
                .Concat(ancillaryFiles)
                .Concat(chartFiles);

            var existingLinks = _contentDbContext
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == release.Id)
                .ToList();

            allFiles
                .Where(file => !existingLinks
                    .Select(e => e.ReleaseFileReference.Filename)
                    .Contains(file.Item1.FileName))
                .ToList()
                .ForEach(fileAndType =>
                {
                    var subject = _statisticsDbContext
                        .Subject
                        .FirstOrDefault(s => s.Name == fileAndType.Item1.Name);

                    _contentDbContext.Add(new ReleaseFile
                    {
                        ReleaseId = release.Id,
                        ReleaseFileReference = new ReleaseFileReference
                        {
                            ReleaseId = release.Id,
                            Filename = fileAndType.Item1.FileName,
                            SubjectId = subject?.Id,
                            ReleaseFileType = fileAndType.Item2
                        }
                    });
                });

            await _contentDbContext.SaveChangesAsync();

            return release;
        }

        private async Task<Either<ActionResult, Release>> PopulateReleaseSubjectTable(Release release)
        {
            var subjectLinks = _statisticsDbContext
                .Subject
                .Where(s => s.ReleaseId == release.Id);

            var releaseSubjectLinks = subjectLinks
                .Select(s => new ReleaseSubject
                {
                    SubjectId = s.Id,
                    ReleaseId = s.ReleaseId
                })
                .ToList();

            var existingSubjectLinks = _statisticsDbContext
                .ReleaseSubject
                .Where(r => r.ReleaseId == release.Id)
                .ToList();

            var newSubjectLinks = releaseSubjectLinks
                .Where(r => existingSubjectLinks.Find(e => e.SubjectId == r.SubjectId) == null);
            
            _statisticsDbContext.AddRange(newSubjectLinks);
            await _statisticsDbContext.SaveChangesAsync();

            return release;
        }
    }
}