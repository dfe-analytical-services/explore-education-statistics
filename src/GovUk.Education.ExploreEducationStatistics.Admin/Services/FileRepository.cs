using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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
    }
}
