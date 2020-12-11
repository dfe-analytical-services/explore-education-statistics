using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ImportStatusBauService : IImportStatusBauService
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IUserService _userService;
        private readonly ContentDbContext _contentDbContext;

        public ImportStatusBauService(ITableStorageService tableStorageService,
            IUserService userService,
            ContentDbContext contentDbContext)
        {
            _tableStorageService = tableStorageService;
            _userService = userService;
            _contentDbContext = contentDbContext;
        }

        public async Task<Either<ActionResult, List<ImportStatusBau>>> GetAllIncompleteImports()
        {
            return await _userService
                .CheckCanViewAllImports()
                .OnSuccess(async () =>
                {
                    var queryFilterCondition = TableQuery.GenerateFilterCondition("Status",
                        QueryComparisons.NotEqual,
                        IStatus.COMPLETE.ToString());
                    var tableQuery = new TableQuery<DatafileImport>().Where(queryFilterCondition);
                    var queryResults = await _tableStorageService.ExecuteQueryAsync(
                        DatafileImportsTableName, tableQuery);

                    return queryResults
                        .OrderByDescending(result => result.Timestamp)
                        .Select(async result =>
                    {
                        var releaseId = new Guid(result.PartitionKey);
                        var dataFileName = result.RowKey;
                        var release =
                            await _contentDbContext.Releases
                                .Where(r => r.Id == releaseId)
                                .Include(r => r.Publication)
                                .FirstAsync();

                        var releaseFileRef = await _contentDbContext.ReleaseFileReferences.FirstAsync(r =>
                            r.ReleaseId == releaseId &&
                            r.Filename == dataFileName &&
                            r.ReleaseFileType == ReleaseFileTypes.Data);

                        return new ImportStatusBau()
                        {
                            SubjectTitle = null, // EES-1655
                            SubjectId = releaseFileRef.SubjectId,
                            PublicationId = release.PublicationId,
                            PublicationTitle = release.Publication.Title,
                            ReleaseId = release.Id,
                            ReleaseTitle = release.Title,
                            DataFileName = result.RowKey,
                            NumberOfRows = result.NumberOfRows,
                            Status = result.Status,
                            StagePercentageComplete = result.PercentageComplete,
                        };
                    }).Select(t => t.Result).ToList();
                });
        }
    }
}
