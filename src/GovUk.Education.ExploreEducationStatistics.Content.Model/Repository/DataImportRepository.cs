#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository
{
    public class DataImportRepository : IDataImportRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public DataImportRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<DataImport> Add(DataImport dataImport)
        {
            var added = await _contentDbContext.DataImports.AddAsync(dataImport);
            await _contentDbContext.SaveChangesAsync();
            return added.Entity;
        }

        public async Task DeleteByFileId(Guid fileId)
        {
            var import = await GetByFileId(fileId);

            if (import != null)
            {
                _contentDbContext.DataImports.Remove(import);
                await _contentDbContext.SaveChangesAsync();
            }
        }

        public async Task<DataImport?> GetByFileId(Guid fileId)
        {
            return await _contentDbContext.DataImports
                .SingleOrDefaultAsync(i => i.FileId == fileId);
        }

        public async Task<DataImportStatus> GetStatusByFileId(Guid fileId)
        {
            var import = await GetByFileId(fileId);
            return import?.Status ?? DataImportStatus.NOT_FOUND;
        }
    }
}
