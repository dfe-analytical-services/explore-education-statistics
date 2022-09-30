#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPermalinkMigrationService
{
    Task<Permalink> AddPermalinkToDbFromStorage(Guid permalinkId);

    Task EnumerateAllPermalinksForMigration();
}
