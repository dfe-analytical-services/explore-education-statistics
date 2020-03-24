using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    
    // TODO BAU-405 - temporary code to help seed the Release-File tables from Blob storage 
    public class ReleaseMigrationService : IReleaseMigrationService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly ISubjectService _subjectService;
        private readonly IFileStorageService _fileStorageService;

        public ReleaseMigrationService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            ISubjectService subjectService,
            IFileStorageService fileStorageService)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _subjectService = subjectService;
            _persistenceHelper = persistenceHelper;
            _fileStorageService = fileStorageService;
        }

        public Task<Either<ActionResult, List<ReleaseFile>>> PopulateReleaseFileTables(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
//                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async _ =>
                {
                    var dataFiles = (await _fileStorageService
                        .ListFilesFromBlobStorage(releaseId, ReleaseFileTypes.Data))
                        .Where(f => f.MetaFileName?.Length > 0)
                        .Select(f => (f, ReleaseFileTypes.Data));

                    var metadataFiles = (await _fileStorageService
                            .ListFilesFromBlobStorage(releaseId, ReleaseFileTypes.Data))
                        .Where(f => f.MetaFileName?.Length == 0)
                        .Select(f => (f, ReleaseFileTypes.Metadata));

                    var ancillaryFiles = (await _fileStorageService
                        .ListFilesFromBlobStorage(releaseId, ReleaseFileTypes.Ancillary)) 
                        .Select(f => (f, ReleaseFileTypes.Ancillary));
                    
                    var chartFiles = (await _fileStorageService
                        .ListFilesFromBlobStorage(releaseId, ReleaseFileTypes.Chart))
                        .Select(f => (f, ReleaseFileTypes.Chart));

                    return dataFiles
                        .Concat(metadataFiles)
                        .Concat(ancillaryFiles)
                        .Concat(chartFiles);
                })
                .OnSuccess(async filesAndTypes =>
                {
                    var existingLinks = _contentDbContext
                        .ReleaseFiles
                        .Include(f => f.ReleaseFileReference)
                        .Where(f => f.ReleaseId == releaseId)
                        .ToList();
                        
                    filesAndTypes
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
                                ReleaseId = releaseId,
                                ReleaseFileReference = new ReleaseFileReference
                                {
                                    ReleaseId = releaseId,
                                    Filename = fileAndType.Item1.FileName,
                                    SubjectId = subject?.Id,
                                    ReleaseFileType = fileAndType.Item2
                                }
                            });
                        });

                    await _contentDbContext.SaveChangesAsync();

                    return await _contentDbContext
                        .ReleaseFiles
                        .Where(f => f.ReleaseId == releaseId)
                        .ToListAsync();
                });
        }
    }
}