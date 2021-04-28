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
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseFileRepository : IReleaseFileRepository
    {
        private readonly ContentDbContext _contentDbContext;

        private static readonly List<FileType> SupportedFileTypes = new List<FileType>
        {
            Ancillary,
            Chart,
            Image
        };

        public ReleaseFileRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<Either<ActionResult, File>> CheckFileExists(Guid releaseId, Guid id,
            params FileType[] allowedFileTypes)
        {
            // Ensure file is linked to the Release by getting the ReleaseFile first
            var releaseFile = await Get(releaseId, id);

            if (releaseFile == null)
            {
                return new NotFoundResult();
            }

            if (allowedFileTypes.Any() && !allowedFileTypes.Contains(releaseFile.File.Type))
            {
                return ValidationUtils.ValidationActionResult(FileTypeInvalid);
            }

            return releaseFile.File;
        }

        public async Task<ReleaseFile> Create(
            Guid releaseId,
            string filename,
            FileType type,
            Guid createdById,
            string name = null)
        {
            if (!SupportedFileTypes.Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Cannot create file for file type");
            }

            var releaseFile = new ReleaseFile
            {
                ReleaseId = releaseId,
                Name = name,
                File = new File
                {
                    Created = DateTime.UtcNow,
                    CreatedById = createdById,
                    RootPath = releaseId,
                    Filename = filename,
                    Type = type
                }
            };

            var created = (await _contentDbContext.ReleaseFiles.AddAsync(releaseFile)).Entity;
            await _contentDbContext.SaveChangesAsync();
            return created;
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
                .Include(releaseFile => releaseFile.File)
                .SingleOrDefaultAsync(releaseFile =>
                    releaseFile.ReleaseId == releaseId
                    && releaseFile.FileId == fileId);
        }

        public async Task<List<ReleaseFile>> GetByFileType(Guid releaseId, params FileType[] types)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(f => f.File)
                .Where(releaseFile =>
                    releaseFile.ReleaseId == releaseId
                    && types.Contains(releaseFile.File.Type))
                .ToListAsync();
        }

        public async Task<bool> FileIsLinkedToOtherReleases(Guid releaseId, Guid fileId)
        {
            return await _contentDbContext.ReleaseFiles
                .AnyAsync(releaseFile =>
                    releaseFile.ReleaseId != releaseId
                    && releaseFile.FileId == fileId);
        }

        public async Task<ReleaseFile> UpdateFilename(Guid releaseId,
            Guid fileId,
            string filename)
        {
            // Ensure file is linked to the Release by getting the ReleaseFile first
            var releaseFile = await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .SingleAsync(rf =>
                    rf.ReleaseId == releaseId
                    && rf.FileId == fileId);

            var file = releaseFile.File;
            _contentDbContext.Update(file);
            file.Filename = filename;

            await _contentDbContext.SaveChangesAsync();

            return releaseFile;
        }

        public async Task UpdateName(Guid releaseId,
            Guid fileId,
            string name)
        {
            var releaseFile = await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .SingleAsync(rf =>
                    rf.ReleaseId == releaseId
                    && rf.FileId == fileId
                    && (rf.File.Type == FileType.Data || rf.File.Type == Ancillary));

            _contentDbContext.Update(releaseFile);
            releaseFile.Name = name;

            await _contentDbContext.SaveChangesAsync();
        }
    }
}
