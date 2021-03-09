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
    public class ReleaseDataFileRepository : IReleaseDataFileRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public static readonly List<FileType> SupportedFileTypes = new List<FileType>
        {
            FileType.Data,
            Metadata
        };

        public ReleaseDataFileRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<File> Create(
            Guid releaseId,
            Guid subjectId,
            string filename,
            FileType type,
            File replacingFile = null,
            File source = null)
        {
            if (!SupportedFileTypes.Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Cannot create file for file type");
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
                    // Mark any new files as already migrated while these flags temporarily exist
                    PrivateBlobPathMigrated = true,
                    PublicBlobPathMigrated = true,
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
                // Mark any new files as already migrated while these flags temporarily exist
                PrivateBlobPathMigrated = true,
                PublicBlobPathMigrated = true,
                RootPath = releaseId,
                Filename = filename,
                Type = DataZip
            })).Entity;

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

        public async Task<IList<File>> ListReplacementDataFiles(Guid releaseId)
        {
            return await _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(
                    rf => rf.ReleaseId == releaseId
                          && rf.File.Type == FileType.Data
                          && rf.File.ReplacingId != null
                          && rf.File.SubjectId.HasValue
                )
                .Select(rf => rf.File)
                .ToListAsync();
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
    }
}
