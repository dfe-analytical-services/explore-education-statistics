#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository
{
    public class ReleaseFileRepository : IReleaseFileRepository
    {
        private readonly ContentDbContext _contentDbContext;

        private static readonly List<FileType> SupportedFileTypes = new()
        {
            Ancillary,
            Chart,
            Image
        };

        public ReleaseFileRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<ReleaseFile> Create(
            Guid releaseId,
            string filename,
            long contentLength,
            string contentType,
            FileType type,
            Guid createdById,
            string? name = null,
            string? summary = null)
        {
            if (!SupportedFileTypes.Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Cannot create file for file type");
            }

            var releaseFile = new ReleaseFile
            {
                ReleaseId = releaseId,
                Name = name,
                Summary = summary,
                File = new File
                {
                    CreatedById = createdById,
                    RootPath = releaseId,
                    Filename = filename,
                    ContentLength = contentLength,
                    ContentType = contentType,
                    Type = type
                }
            };

            var created = (await _contentDbContext.ReleaseFiles.AddAsync(releaseFile)).Entity;
            await _contentDbContext.SaveChangesAsync();
            return created;
        }

        public async Task Delete(Guid releaseId, Guid fileId)
        {
            var releaseFileToRemove = await Find(releaseId, fileId);

            if (releaseFileToRemove != null)
            {
                _contentDbContext.ReleaseFiles.Remove(releaseFileToRemove);

                await _contentDbContext.SaveChangesAsync();
            }
        }

        public async Task<ReleaseFile?> Find(Guid releaseId, Guid fileId)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(releaseFile => releaseFile.File)
                .SingleOrDefaultAsync(releaseFile =>
                    releaseFile.ReleaseId == releaseId
                    && releaseFile.FileId == fileId);
        }

        public async Task<Either<ActionResult, ReleaseFile>> FindOrNotFound(Guid releaseId, Guid fileId)
        {
            return await Find(releaseId, fileId) ?? new Either<ActionResult, ReleaseFile>(new NotFoundResult());
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
                .AsQueryable()
                .AnyAsync(releaseFile =>
                    releaseFile.ReleaseId != releaseId
                    && releaseFile.FileId == fileId);
        }

        public async Task<ReleaseFile> Update(Guid releaseId,
            Guid fileId,
            string? name = null,
            string? fileName = null,
            string? summary = null)
        {
            // Ensure file is linked to the Release by getting the ReleaseFile first
            var releaseFile = await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .SingleAsync(rf =>
                    rf.ReleaseId == releaseId
                    && rf.FileId == fileId);

            _contentDbContext.Update(releaseFile);

            releaseFile.Name = name ?? releaseFile.Name;
            releaseFile.File.Filename = fileName ?? releaseFile.File.Filename;
            releaseFile.Summary = summary ?? releaseFile.Summary;

            await _contentDbContext.SaveChangesAsync();

            return releaseFile;
        }
    }
}
