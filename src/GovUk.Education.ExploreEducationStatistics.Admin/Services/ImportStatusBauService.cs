using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ImportStatusBauService : IImportStatusBauService
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IUserService _userService;
        private readonly ContentDbContext _contentDbContext;
        private readonly ILogger<ImportStatusBauService> _logger;

        public ImportStatusBauService(ITableStorageService tableStorageService,
            IUserService userService,
            ContentDbContext contentDbContext,
            ILogger<ImportStatusBauService> logger)
        {
            _tableStorageService = tableStorageService;
            _userService = userService;
            _contentDbContext = contentDbContext;
            _logger = logger;
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

                            var releaseFileRef = await _contentDbContext.Files.Where(
                                    r =>
                                        r.ReleaseId == releaseId &&
                                        r.Filename == dataFileName &&
                                        r.Type == FileType.Data)
                                .Include(f => f.Release)
                                .ThenInclude(r => r.Publication)
                                .FirstOrDefaultAsync();

                            if (releaseFileRef == null)
                            {
                                _logger.LogWarning(
                                    "No ReleaseFileReference found! " +
                                    $"releaseId: \"{releaseId}\" " +
                                    $"dataFileName: \"{dataFileName}\"");
                                return null;
                            }

                            return new ImportStatusBau()
                            {
                                SubjectTitle = null, // EES-1655
                                SubjectId = releaseFileRef.SubjectId,
                                PublicationId = releaseFileRef.Release.PublicationId,
                                PublicationTitle = releaseFileRef.Release.Publication.Title,
                                ReleaseId = releaseFileRef.Release.Id,
                                ReleaseTitle = releaseFileRef.Release.Title,
                                DataFileName = result.RowKey,
                                NumberOfRows = result.NumberOfRows,
                                Status = result.Status,
                                StagePercentageComplete = result.PercentageComplete,
                            };
                        })
                        .Select(t => t?.Result)
                        .Where(item => item != null)
                        .ToList();
                });
        }
    }
}
