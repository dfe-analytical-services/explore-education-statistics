#nullable enable
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

public interface IStorageDataSetWriterResolver
{
    IStorageDataSetWriter Resolve(File dataFile);
}
