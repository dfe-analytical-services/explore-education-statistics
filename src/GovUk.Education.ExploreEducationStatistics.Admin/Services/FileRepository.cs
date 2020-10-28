using System;
using System.Linq;
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

        public async Task<ReleaseFileReference> CreateOrUpdate(string filename,
            Guid releaseId,
            ReleaseFileTypes type,
            Guid? id = null,
            ReleaseFileReference replacingFile = null,
            ReleaseFileReference source = null)
        {
            // If updating existing then check if existing reference is for this release - if not then create new ref
            if (id != null)
            {
                var existing = await _contentDbContext.ReleaseFileReferences
                    .Where(rfr => rfr.Id == id).FirstAsync();

                if (existing.ReleaseId == releaseId)
                {
                    _contentDbContext.Update(existing);
                    existing.Filename = filename;
                    return existing;
                }
            }

            var created = (await _contentDbContext.ReleaseFileReferences.AddAsync(new ReleaseFileReference
            {
                ReleaseId = releaseId,
                Filename = filename,
                ReleaseFileType = type,
                Replacing = replacingFile,
                Source = source
            })).Entity;

            if (replacingFile != null)
            {
                _contentDbContext.Update(replacingFile);
                replacingFile.ReplacedBy = created;
            }

            // No ReleaseFileLink required for the zip file source reference
            if (type != ReleaseFileTypes.DataZip)
            {
                var fileLink = new ReleaseFile
                {
                    ReleaseId = releaseId,
                    ReleaseFileReference = created
                };

                await _contentDbContext.ReleaseFiles.AddAsync(fileLink);
            }

            return created;
        }

        public async Task<ReleaseFileReference> Get(Guid id)
        {
            return await _contentDbContext.ReleaseFileReferences
                .SingleAsync(f => f.Id == id);
        }
    }
}