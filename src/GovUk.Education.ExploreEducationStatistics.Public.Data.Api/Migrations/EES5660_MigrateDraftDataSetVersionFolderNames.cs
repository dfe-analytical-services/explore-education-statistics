using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Migrations;

// TODO EES-5660 - remove this migration after it has been run against each Public API-enabled environment.
public class EES5660_MigrateDraftDataSetVersionFolderNames(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver pathResolver) : ICustomMigration
{
    public void Apply()
    {
        var draftDataSetVersions = publicDataDbContext
            .DataSetVersions
            .WherePrivateStatus()
            .ToList();

        var failedDataSetVersionIds = new List<Guid>();
        
        draftDataSetVersions.ForEach(dataSetVersion =>
        {
            var versionedFolder = pathResolver.DirectoryPath(dataSetVersion, dataSetVersion.SemVersion());
            
            if (Directory.Exists(versionedFolder))
            {
                var newDraftFolder = pathResolver.DirectoryPath(dataSetVersion);

                if (Directory.Exists(newDraftFolder))
                {
                    failedDataSetVersionIds.Add(dataSetVersion.Id);
                }
                else
                {
                    Directory.Move(versionedFolder, newDraftFolder);
                }
            }
        });

        if (failedDataSetVersionIds.Count > 0)
        {
            throw new Exception($"The following DataSetVersions have both a versioned " +
                                $"and a draft folder: {failedDataSetVersionIds.JoinToString(",")}");
        }
    }
}
