using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using Publication = GovUk.Education.ExploreEducationStatistics.Content.Model.Publication;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
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
            _userService = userService;
            _subjectService = subjectService;
            _fileStorageService = fileStorageService;
        }

        public Task<Either<ActionResult, List<ReleaseFile>>> PopulateReleaseFileTables(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
//                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(_ => _fileStorageService.ListFilesFromBlobStorage(releaseId, ReleaseFileTypes.Data))
                .OnSuccess(files => files.Where(f => f.MetaFileName?.Length > 0))
                .OnSuccess(async files =>
                {
                    var existingLinks = _contentDbContext
                        .ReleaseFiles
                        .Where(f => f.ReleaseId == releaseId)
                        .ToList();
                        
                    files
                        .Where(file => !existingLinks.Select(e => e.ReleaseFileReference.Filename).Contains(file.FileName))
                        .ToList()
                        .ForEach(file =>
                        {
                            var subject = _statisticsDbContext
                                .Subject
                                .FirstOrDefault(s => s.Name == file.Name);

                            _contentDbContext.Add(new ReleaseFile
                            {
                                ReleaseId = releaseId,
                                ReleaseFileReference = new ReleaseFileReference
                                {
                                    ReleaseId = releaseId,
                                    Filename = file.FileName,
                                    SubjectId = subject?.Id
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