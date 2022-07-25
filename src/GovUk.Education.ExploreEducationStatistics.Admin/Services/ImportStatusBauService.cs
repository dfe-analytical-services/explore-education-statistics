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
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

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
                    var releaseFilesQueryable = _contentDbContext.ReleaseFiles
                        .Include(rf => rf.Release)
                        .ThenInclude(r => r.Publication);

                    return await _contentDbContext.DataImports
                        .Include(dataImport => dataImport.File)
                        .Join(releaseFilesQueryable,
                            dataImport => dataImport.FileId,
                            releaseFile => releaseFile.FileId,
                            (dataImport, releaseFile) => new
                            {
                                DataImport = dataImport,
                                Release = releaseFile.Release
                            })
                        .Where(join => join.DataImport.Status != COMPLETE)
                        .OrderByDescending(join => join.DataImport.Created)
                        .Select(join => BuildViewModel(join.DataImport, join.Release))
                        .ToListAsync();
                });
        }

        private static ImportStatusBauViewModel BuildViewModel(DataImport dataImport, Release release)
        {
            var file = dataImport.File;
            var publication = release.Publication;
            return new ImportStatusBauViewModel
            {
                SubjectTitle = null, // EES-1655
                SubjectId = dataImport.SubjectId,
                PublicationId = publication.Id,
                PublicationTitle = publication.Title,
                ReleaseId = release.Id,
                ReleaseTitle = release.Title,
                FileId = file.Id,
                DataFileName = file.Filename,
                TotalRows = dataImport.TotalRows,
                Batches = dataImport.NumBatches,
                Status = dataImport.Status,
                StagePercentageComplete = dataImport.StagePercentageComplete,
                PercentageComplete = dataImport.PercentageComplete()
            };
        }
    }
}
