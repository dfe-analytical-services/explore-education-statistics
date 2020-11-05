using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

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
            ReleaseFileTypes type,
            ReleaseFileReference replacingFile = null,
            ReleaseFileReference source = null)
        {
            if (type == ReleaseFileTypes.DataZip)
            {
                throw new ArgumentException($"Cannot use generic Create method for type {ReleaseFileTypes.DataZip}",
                    nameof(type));
            }

            var releaseFile = new ReleaseFile
            {
                ReleaseId = releaseId,
                ReleaseFileReference = new ReleaseFileReference
                {
                    ReleaseId = releaseId,
                    Filename = filename,
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

            return created.ReleaseFileReference;
        }

        public async Task<ReleaseFileReference> CreateZip(Guid releaseId, string filename)
        {
            return (await _contentDbContext.ReleaseFileReferences.AddAsync(new ReleaseFileReference
            {
                ReleaseId = releaseId,
                Filename = filename,
                ReleaseFileType = ReleaseFileTypes.DataZip
            })).Entity;
        }

        public async Task Delete(Guid id)
        {
            var file = await Get(id);
            _contentDbContext.ReleaseFileReferences.Remove(file);
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

            return file;
        }
    }
}