using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ImportRepository : IImportRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public ImportRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<Import> Add(Import import)
        {
            var added = await _contentDbContext.Imports.AddAsync(import);
            await _contentDbContext.SaveChangesAsync();
            return added.Entity;
        }

        public async Task DeleteByFileId(Guid fileId)
        {
            var import = await GetByFileId(fileId);

            if (import != null)
            {
                _contentDbContext.Imports.Remove(import);
                await _contentDbContext.SaveChangesAsync();
            }
        }

        public async Task<Import> GetByFileId(Guid fileId)
        {
            return await _contentDbContext.Imports
                .Include(i => i.Errors)
                .SingleOrDefaultAsync(i => i.FileId == fileId);
        }

        public async Task<ImportStatus> GetStatusByFileId(Guid fileId)
        {
            var import = await _contentDbContext.Imports
                .SingleOrDefaultAsync(i => i.FileId == fileId);

            return import?.Status ?? ImportStatus.NOT_FOUND;
        }
    }
}