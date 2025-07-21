#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

    public class ReleaseFileRepository : IReleaseFileRepository
    {
        private readonly ContentDbContext _contentDbContext;

        private static readonly List<FileType> SupportedFileTypes = new()
        {
            Ancillary,
            Chart,
            Image
        };

        public ReleaseFileRepository(
            ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<ReleaseFile> Create(
            Guid releaseVersionId,
            string filename,
            long contentLength,
            string contentType,
            FileType type,
            Guid createdById,
            string? name = null,
            string? summary = null,
            Guid? newFileId = null)
        {
            if (!SupportedFileTypes.Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Cannot create file for file type");
            }

            var releaseFile = new ReleaseFile
            {
                ReleaseVersionId = releaseVersionId,
                Name = name,
                Summary = summary,
                File = new File
                {
                    Id = newFileId ?? Guid.NewGuid(),
                    CreatedById = createdById,
                    RootPath = releaseVersionId,
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

        public async Task Delete(Guid releaseVersionId,
            Guid fileId)
        {
            var releaseFileToRemove = await Find(releaseVersionId, fileId);

            if (releaseFileToRemove != null)
            {
                _contentDbContext.ReleaseFiles.Remove(releaseFileToRemove);

                await _contentDbContext.SaveChangesAsync();
            }
        }

        public async Task<ReleaseFile?> Find(Guid releaseVersionId,
            Guid fileId)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(releaseFile => releaseFile.File.CreatedBy)
                .SingleOrDefaultAsync(releaseFile =>
                    releaseFile.ReleaseVersionId == releaseVersionId
                    && releaseFile.FileId == fileId);
        }

        public async Task<Either<ActionResult, ReleaseFile>> FindOrNotFound(Guid releaseVersionId,
            Guid fileId)
        {
            return await Find(releaseVersionId, fileId) ?? new Either<ActionResult, ReleaseFile>(new NotFoundResult());
        }

        public async Task<List<ReleaseFile>> GetByFileType(Guid releaseVersionId,
            CancellationToken cancellationToken = default,
            params FileType[] types)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(f => f.File.CreatedBy)
                .Where(releaseFile =>
                    releaseFile.ReleaseVersionId == releaseVersionId
                    && types.Contains(releaseFile.File.Type))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> FileIsLinkedToOtherReleases(Guid releaseVersionId,
            Guid fileId)
        {
            return await _contentDbContext.ReleaseFiles
                .AsQueryable()
                .AnyAsync(releaseFile =>
                    releaseFile.ReleaseVersionId != releaseVersionId
                    && releaseFile.FileId == fileId);
        }

        public async Task<ReleaseFile> Update(Guid releaseVersionId,
            Guid fileId,
            Guid? newFileId = null,
            string? name = null,
            string? fileName = null,
            string? summary = null)
        {
            // Ensure file is linked to the ReleaseVersion by getting the ReleaseFile first
            var releaseFile = await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .SingleAsync(rf =>
                    rf.ReleaseVersionId == releaseVersionId
                    && rf.FileId == fileId);

            _contentDbContext.Update(releaseFile);

            releaseFile.FileId = newFileId ?? releaseFile.FileId;
            releaseFile.Name = name ?? releaseFile.Name;
            releaseFile.File.Filename = fileName ?? releaseFile.File.Filename;
            releaseFile.Summary = summary ?? releaseFile.Summary;

            await _contentDbContext.SaveChangesAsync();

            return releaseFile;
        }

        public async Task<Either<ActionResult, (ReleaseFile originalReleaseFile, ReleaseFile replacementReleaseFile)>>
            CheckLinkedOriginalAndReplacementReleaseFilesExist(Guid releaseVersionId,
                Guid originalFileId)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId
                             && rf.FileId == originalFileId
                             && rf.File.Type == Data
                             && rf.File.ReplacedById != null)
                .Join(
                    _contentDbContext.ReleaseFiles.Include(rf => rf.File),
                    original => original.File.ReplacedById,
                    replacement => replacement.FileId,
                    (original, replacement) => new
                    {
                        Original = original,
                        Replacement = replacement
                    })
                .FirstOrNotFoundAsync(joined =>
                    joined.Replacement.ReleaseVersionId == releaseVersionId
                    && joined.Replacement.File.Type == Data
                    && joined.Original.FileId == joined.Replacement.File.ReplacingId)
                .OnSuccess(releaseFiles =>
                    new Tuple<ReleaseFile, ReleaseFile>(
                            releaseFiles.Original,
                            releaseFiles.Replacement)
                        .ToValueTuple());
        }
    }
