using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class DataSetVersionImportService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext) 
    : IDataSetVersionImportService
{
    public async Task<bool> IsPublicApiDataSetImportsInProgress(Guid releaseVersionId)
    {
        var dataFileIds = await contentDbContext
            .ReleaseFiles
            .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data)
            .Select(rf => rf.FileId)
            .ToListAsync();

        return await publicDataDbContext
            .DataSetVersionImports
            .AsNoTracking()
            .AnyAsync(import => 
                dataFileIds.Contains(import.DataSetVersion.CsvFileId) 
                && !DataSetVersionImportStatusConstants.TerminalStates.ToList().Contains(import.Status));
    }
}
