using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ImportStatusBauService : IImportStatusBauService
    {
        private readonly IUserService _userService;
        private readonly ContentDbContext _contentDbContext;

        public ImportStatusBauService(IUserService userService,
            ContentDbContext contentDbContext)
        {
            _userService = userService;
            _contentDbContext = contentDbContext;
        }

        public async Task<Either<ActionResult, List<ImportStatusBauViewModel>>> GetAllIncompleteImports()
        {
            return await _userService
                .CheckCanViewAllImports()
                .OnSuccess(async () =>
                {
                    var imports = await _contentDbContext.Imports
                        .Include(import => import.File)
                        .ThenInclude(file => file.Release)
                        .ThenInclude(release => release.Publication)
                        .Where(import => import.Status != ImportStatus.COMPLETE)
                        .OrderByDescending(import => import.Created)
                        .ToListAsync();

                    return imports.Select(BuildViewModel).ToList();
                });
        }

        private static ImportStatusBauViewModel BuildViewModel(Import import)
        {
            var file = import.File;
            var release = file.Release;
            var publication = release.Publication;
            return new ImportStatusBauViewModel
            {
                SubjectTitle = null, // EES-1655
                SubjectId = import.SubjectId,
                PublicationId = publication.Id,
                PublicationTitle = publication.Title,
                ReleaseId = release.Id,
                ReleaseTitle = release.Title,
                FileId = file.Id,
                DataFileName = file.Filename,
                Rows = import.Rows,
                Batches = import.NumBatches,
                Status = import.Status,
                StagePercentageComplete = import.StagePercentageComplete,
                PercentageComplete = import.PercentageComplete()
            };
        }
    }
}
