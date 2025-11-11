#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;

public class ReleaseDataFileRepository : IReleaseDataFileRepository
{
    private readonly ContentDbContext _contentDbContext;

    private static readonly List<FileType> SupportedFileTypes = new() { Data, Metadata };

    public ReleaseDataFileRepository(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<File> Create(
        Guid releaseVersionId,
        Guid subjectId,
        string filename,
        long contentLength,
        FileType type,
        Guid createdById,
        bool? apiCompatible = null,
        string? name = null,
        File? replacingDataFile = null,
        int order = 0
    )
    {
        if (!SupportedFileTypes.Contains(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Cannot create file for file type");
        }

        if (type == Metadata && replacingDataFile != null)
        {
            throw new ArgumentException("replacingDataFile only used with Files of type Data, not Metadata.");
        }

        var releaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersionId,
            Name = name,
            Order = order,
            File = new File
            {
                CreatedById = createdById,
                RootPath = releaseVersionId,
                SubjectId = subjectId,
                DataSetFileId = type != Data ? null : replacingDataFile?.DataSetFileId ?? Guid.NewGuid(),
                DataSetFileVersion = type != Data ? null : replacingDataFile?.DataSetFileVersion + 1 ?? 0,
                DataSetFileMeta = null, // If FileType.Data, this is set by Data.Processor when import is complete
                Filename = filename,
                ContentLength = contentLength,
                ContentType = "text/csv",
                Type = type,
                Replacing = replacingDataFile,
            },
            PublicApiCompatible = apiCompatible,
        };
        var created = (await _contentDbContext.ReleaseFiles.AddAsync(releaseFile)).Entity;
        if (replacingDataFile != null)
        {
            _contentDbContext.Update(replacingDataFile);
            replacingDataFile.ReplacedBy = releaseFile.File;
        }

        await _contentDbContext.SaveChangesAsync();
        return created.File;
    }

    public async Task<IList<File>> ListDataFiles(Guid releaseVersionId)
    {
        return await ListDataFilesQuery(releaseVersionId).ToListAsync();
    }

    public async Task<bool> HasAnyDataFiles(Guid releaseVersionId)
    {
        return await ListDataFilesQuery(releaseVersionId).AnyAsync();
    }

    public async Task<IList<File>> ListReplacementDataFiles(Guid releaseVersionId)
    {
        return await _contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .Where(rf =>
                rf.ReleaseVersionId == releaseVersionId
                && rf.File.Type == Data
                && rf.File.ReplacingId != null
                && rf.File.SubjectId.HasValue
            )
            .Select(rf => rf.File)
            .ToListAsync();
    }

    public async Task<ReleaseFile> GetBySubject(Guid releaseVersionId, Guid subjectId)
    {
        return await _contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .SingleAsync(rf =>
                rf.ReleaseVersionId == releaseVersionId && rf.File.SubjectId == subjectId && rf.File.Type == Data
            );
    }

    private IQueryable<File> ListDataFilesQuery(Guid releaseVersionId)
    {
        return _contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == Data && rf.File.ReplacingId == null)
            .OrderBy(rf => rf.Order)
            .ThenBy(rf => rf.Name) // For data sets existing before ordering was added
            .Select(rf => rf.File);
    }
}
