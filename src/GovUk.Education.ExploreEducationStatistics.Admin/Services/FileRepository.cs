using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileRepository : IFileRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public FileRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<ReleaseFileReference> Create(Guid releaseId,
            string filename,
            FileType type,
            ReleaseFileReference replacingFile = null,
            ReleaseFileReference source = null)
        {
            if (type == DataZip)
            {
                throw new ArgumentException($"Cannot use generic Create method for type {DataZip}",
                    nameof(type));
            }

            var releaseFile = new ReleaseFile
            {
                ReleaseId = releaseId,
                ReleaseFileReference = new ReleaseFileReference
                {
                    ReleaseId = releaseId,
                    Filename = filename,
                    // Mark any new ancillary or chart files as already migrated while this flag temporarily exists 
                    FilenameMigrated = type == Ancillary || type == Chart,
                    ReleaseFileType = type,
                    Replacing = replacingFile,
                    Source = source
                }
            };

            var created = (await _contentDbContext.ReleaseFiles.AddAsync(releaseFile)).Entity;

            if (replacingFile != null)
            {
                _contentDbContext.Update(replacingFile);
                replacingFile.ReplacedBy = releaseFile.ReleaseFileReference;
            }

            await _contentDbContext.SaveChangesAsync();

            return created.ReleaseFileReference;
        }

        public async Task<ReleaseFileReference> CreateZip(Guid releaseId, string filename)
        {
            var file = (await _contentDbContext.ReleaseFileReferences.AddAsync(new ReleaseFileReference
            {
                ReleaseId = releaseId,
                Filename = filename,
                ReleaseFileType = DataZip
            })).Entity;

            await _contentDbContext.SaveChangesAsync();

            return file;
        }

        public async Task Delete(Guid id)
        {
            var file = await Get(id);
            _contentDbContext.ReleaseFileReferences.Remove(file);

            await _contentDbContext.SaveChangesAsync();
        }

        public async Task<ReleaseFileReference> Get(Guid id)
        {
            return await _contentDbContext.ReleaseFileReferences
                .SingleAsync(f => f.Id == id);
        }

        public async Task<ReleaseFileReference> UpdateFilename(Guid releaseId,
            Guid fileId,
            string filename)
        {
            // Ensure file is linked to the Release by getting the ReleaseFile first
            var releaseFile = await _contentDbContext.ReleaseFiles
                .Include(rf => rf.ReleaseFileReference)
                .SingleAsync(rf =>
                    rf.ReleaseId == releaseId
                    && rf.ReleaseFileReferenceId == fileId);

            var file = releaseFile.ReleaseFileReference;
            _contentDbContext.Update(file);
            file.Filename = filename;

            await _contentDbContext.SaveChangesAsync();

            return file;
        }

        public async Task<IList<ReleaseFileReference>> ListDataFiles(Guid releaseId)
        {
            return await ListDataFilesQuery(releaseId).ToListAsync();
        }

        public async Task<bool> HasAnyDataFiles(Guid releaseId)
        {
            return await ListDataFilesQuery(releaseId).AnyAsync();
        }

        private IQueryable<ReleaseFileReference> ListDataFilesQuery(Guid releaseId)
        {
            return _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.ReleaseFileReference)
                .Where(
                    rf => rf.ReleaseId == releaseId
                          && rf.ReleaseFileReference.ReleaseFileType == ReleaseFileTypes.Data
                          && rf.ReleaseFileReference.ReplacingId == null
                          && rf.ReleaseFileReference.SubjectId.HasValue
                )
                .Select(rf => rf.ReleaseFileReference);
        }

        public async Task<IList<ReleaseFileReference>> ListReplacementDataFiles(Guid releaseId)
        {
            return await _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.ReleaseFileReference)
                .Where(
                    rf => rf.ReleaseId == releaseId
                          && rf.ReleaseFileReference.ReleaseFileType == ReleaseFileTypes.Data
                          && rf.ReleaseFileReference.ReplacingId != null
                )
                .Select(rf => rf.ReleaseFileReference)
                .ToListAsync();
        }
    }
}