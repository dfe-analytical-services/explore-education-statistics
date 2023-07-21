using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

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

    public async Task<File> Create(
        Guid releaseId,
        string filename,
        long contentLength,
        string contentType,
        FileType type,
        Guid createdById,
        Guid? newFileId = null)
    {
        var file = new File
        {
            Id = newFileId ?? Guid.NewGuid(),
            RootPath = releaseId,
            Filename = filename,
            ContentLength = contentLength,
            ContentType = contentType,
            Type = type,
            CreatedById = createdById,
        };

        await _contentDbContext.Files.AddAsync(file);
        await _contentDbContext.SaveChangesAsync();

        return file;
    }
}
