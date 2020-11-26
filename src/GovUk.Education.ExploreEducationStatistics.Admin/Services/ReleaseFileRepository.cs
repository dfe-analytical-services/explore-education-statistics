using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseFileRepository : IReleaseFileRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public ReleaseFileRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<Either<ActionResult, ReleaseFileReference>> CheckFileExists(Guid releaseId, Guid id,
            params ReleaseFileTypes[] allowedFileTypes)
        {
            // Ensure file is linked to the Release by getting the ReleaseFile first
            var releaseFile = await Get(releaseId, id);

            if (releaseFile == null)
            {
                return new NotFoundResult();
            }

            if (allowedFileTypes.Any() && !allowedFileTypes.Contains(releaseFile.ReleaseFileReference.ReleaseFileType))
            {
                return ValidationUtils.ValidationActionResult(FileTypeInvalid);
            }

            return releaseFile.ReleaseFileReference;
        }

        public async Task Delete(Guid releaseId, Guid fileId)
        {
            var releaseFile = await Get(releaseId, fileId);
            if (releaseFile != null)
            {
                _contentDbContext.ReleaseFiles.Remove(releaseFile);
                await _contentDbContext.SaveChangesAsync();
            }
        }

        public async Task<ReleaseFile> Get(Guid releaseId, Guid fileId)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(releaseFile => releaseFile.ReleaseFileReference)
                .SingleOrDefaultAsync(releaseFile =>
                    releaseFile.ReleaseId == releaseId
                    && releaseFile.ReleaseFileReferenceId == fileId);
        }

        public async Task<List<ReleaseFile>> GetByFileType(Guid releaseId, params ReleaseFileTypes[] types)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(releaseFile =>
                    releaseFile.ReleaseId == releaseId
                    && types.Contains(releaseFile.ReleaseFileReference.ReleaseFileType))
                .ToListAsync();
        }

        public async Task<bool> FileIsLinkedToOtherReleases(Guid releaseId, Guid fileId)
        {
            return await _contentDbContext.ReleaseFiles
                .AnyAsync(releaseFile =>
                    releaseFile.ReleaseId != releaseId
                    && releaseFile.ReleaseFileReferenceId == fileId);
        }
    }
}