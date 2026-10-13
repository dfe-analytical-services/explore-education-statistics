#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;

public interface IObservationBatchImporter
{
    Task ImportObservationBatch(StatisticsDbContext context, IEnumerable<Observation> observations);
}
