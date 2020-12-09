using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ImportStatusBauService : IImportStatusBauService
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger _logger;
        private readonly IUserService _userService;

        public ImportStatusBauService(ITableStorageService tableStorageService,
            ILogger<ImportStatusBauService> logger,
            IUserService userService)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
            _userService = userService;
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

                    return queryResults.Where(result =>
                    {
                        if (result.Message == "")
                        {
                            _logger.LogWarning(
                                $"{MethodBase.GetCurrentMethod()} received Imports row with empty Message. " +
                                $"PartitionKey: \"{result.PartitionKey}\", RowKey: \"{result.RowKey}\"");
                            return false;
                        }

                        return true;
                    }).Select(result =>
                    {
                        var m = JsonConvert.DeserializeObject<ImportMessage>(result.Message);

                        return new ImportStatusBau()
                        {
                            SubjectTitle = null,  // EES-1655
                            SubjectId = m.SubjectId,
                            PublicationId = m.Release.Publication.Id,
                            ReleaseId = m.Release.Id,
                            DataFileName = result.RowKey,
                            MetaFileName = m.MetaFileName,
                            NumberOfRows = result.NumberOfRows,
                            Status = result.Status,
                            StagePercentageComplete = result.PercentageComplete,
                        };
                    }).ToList();
                });
        }
    }
}
