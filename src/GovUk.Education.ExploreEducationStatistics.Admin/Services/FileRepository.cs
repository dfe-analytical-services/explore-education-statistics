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

        public async Task<File> CreateAncillaryOrChart(
            Guid releaseId,
            string filename,
            FileType type)
        {
            if (type != Ancillary && type != Chart)
            {
                throw new ArgumentException(
                    "CreateAncillaryOrChart only used to create Files of type Ancillary or Chart");
            }

            var releaseFile = new ReleaseFile
            {
                ReleaseId = releaseId,
                File = new File
                {
                    RootPath = releaseId,
                    Filename = filename,
                    Type = type
                }
            };
            var created = (await _contentDbContext.ReleaseFiles.AddAsync(releaseFile)).Entity;
            await _contentDbContext.SaveChangesAsync();
            return created.File;
        }

        public async Task<File> CreateDataOrMetadata(
            Guid releaseId,
            Guid subjectId,
            string filename,
            FileType type,
            File replacingFile = null,
            File source = null)
        {
            if (type != FileType.Data && type != Metadata)
            {
                throw new ArgumentException(
                    "CreateDataOrMetadata only used to create Files of type Data and Metadata");
            }

            if (type == Metadata && replacingFile != null)
            {
                throw new ArgumentException("replacingFile only used with Files of type Data, not Metadata.");
            }

            var releaseFile = new ReleaseFile
            {
                ReleaseId = releaseId,
                File = new File
                {
                    RootPath = releaseId,
                    SubjectId = subjectId,
                    Filename = filename,
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
                RootPath = releaseId,
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
