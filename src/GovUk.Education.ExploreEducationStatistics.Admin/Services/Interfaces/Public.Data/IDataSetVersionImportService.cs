using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetVersionImportService
{
    Task<bool> IsPublicApiDataSetImportsInProgress(Guid releaseVersionId);
}
