#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository
{
    public class ReleaseDataFileRepository : IReleaseDataFileRepository
    {
        private readonly ContentDbContext _contentDbContext;

        private static readonly List<FileType> SupportedFileTypes = new ()
        {
            Data,
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
            long contentLength,
            FileType type,
            Guid createdById,
            string? name = null,
            File? replacingFile = null,
            File? source = null,
            int order = 0)
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
                Name = name,
                Order = order,
                File = new File
                {
                    CreatedById = createdById,
                    RootPath = releaseId,
                    SubjectId = subjectId,
                    Filename = filename,
                    ContentLength = contentLength,
                    ContentType = "text/csv",
                    Type = type,
                    Replacing = replacingFile,
                    Source = source
                },
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

        public async Task<File> CreateZip(Guid releaseId,
            string filename,
            long contentLength,
            string contentType,
            Guid createdById)
        {
            var file = (await _contentDbContext.Files.AddAsync(new File
            {
                CreatedById = createdById,
                RootPath = releaseId,
                Filename = filename,
                ContentLength = contentLength,
                ContentType = contentType,
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

        public async Task<ReleaseFile> GetBySubject(Guid releaseId, Guid subjectId)
        {
            return await _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.File)
                .SingleAsync(rf =>
                    rf.ReleaseId == releaseId
                    && rf.File.SubjectId == subjectId
                    && rf.File.Type == Data);
        }

        private IQueryable<File> ListDataFilesQuery(Guid releaseId)
        {
            return _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(
                    rf => rf.ReleaseId == releaseId
                          && rf.File.Type == Data
                          && rf.File.ReplacingId == null
                          && rf.File.SubjectId.HasValue
                )
                .OrderBy(rf => rf.Order)
                .ThenBy(rf => rf.Name) // For subjects existing before ordering was added
                .Select(rf => rf.File);
        }
    }
}
