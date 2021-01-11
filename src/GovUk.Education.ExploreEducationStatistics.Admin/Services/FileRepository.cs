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

        public async Task<File> Create(Guid releaseId,
            string filename,
            FileType type,
            File replacingFile = null,
            File source = null,
            Guid? subjectId = null)
        {
            if (type == DataZip)
            {
                throw new ArgumentException($"Cannot use generic Create method for type {DataZip}",
                    nameof(type));
            }

            if ((type == FileType.Data || type == Metadata) && subjectId == null)
            {
                throw new ArgumentException("If creating data or metadata File, subjectId must not be null.");
            }

            var releaseFile = new ReleaseFile
            {
                ReleaseId = releaseId,
                File = new File
                {
                    ReleaseId = releaseId,
                    SubjectId = subjectId,
                    Filename = filename,
                    // Mark any new ancillary or chart files as already migrated while this flag temporarily exists 
                    FilenameMigrated = type == Ancillary || type == Chart,
                    Type = type,
                    Replacing = replacingFile,
                    Source = source
                }
            };

            var created = (await _contentDbContext.ReleaseFiles.AddAsync(releaseFile)).Entity;

            if (replacingFile != null)
            {
                _contentDbContext.Update(replacingFile);
                replacingFile.ReplacedBy = releaseFile.File;
            }

            await _contentDbContext.SaveChangesAsync();

            return created.File;
        }

        public async Task<File> CreateZip(Guid releaseId, string filename)
        {
            var file = (await _contentDbContext.Files.AddAsync(new File
            {
                ReleaseId = releaseId,
                Filename = filename,
                Type = DataZip
            })).Entity;

            await _contentDbContext.SaveChangesAsync();

            return file;
        }

        public async Task Delete(Guid id)
        {
            var file = await Get(id);
            _contentDbContext.Files.Remove(file);

            await _contentDbContext.SaveChangesAsync();
        }

        public async Task<File> Get(Guid id)
        {
            return await _contentDbContext.Files
                .SingleAsync(f => f.Id == id);
        }

        public async Task<File> UpdateFilename(Guid releaseId,
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

            return file;
        }

        public async Task<IList<File>> ListDataFiles(Guid releaseId)
        {
            return await ListDataFilesQuery(releaseId).ToListAsync();
        }

        public async Task<bool> HasAnyDataFiles(Guid releaseId)
        {
            return await ListDataFilesQuery(releaseId).AnyAsync();
        }

        private IQueryable<File> ListDataFilesQuery(Guid releaseId)
        {
            return _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(
                    rf => rf.ReleaseId == releaseId
                          && rf.File.Type == FileType.Data
                          && rf.File.ReplacingId == null
                          && rf.File.SubjectId.HasValue
                )
                .Select(rf => rf.File);
        }

        public async Task<IList<File>> ListReplacementDataFiles(Guid releaseId)
        {
            return await _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(
                    rf => rf.ReleaseId == releaseId
                          && rf.File.Type == FileType.Data
                          && rf.File.ReplacingId != null
                )
                .Select(rf => rf.File)
                .ToListAsync();
        }
    }
}
