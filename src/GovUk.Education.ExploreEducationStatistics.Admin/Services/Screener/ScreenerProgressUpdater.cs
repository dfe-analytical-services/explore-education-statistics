using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Screener;

public class ScreenerProgressUpdater(ContentDbContext contentDbContext) : IScreenerProgressUpdater
{
    public async Task UpdateScreenerProgress()
    {
        var datasetsBeingScreened = await contentDbContext
            .DataSetUploads.Where(upload => upload.Status == DataSetUploadStatus.SCREENING)
            .ToListAsync();
    }
}
