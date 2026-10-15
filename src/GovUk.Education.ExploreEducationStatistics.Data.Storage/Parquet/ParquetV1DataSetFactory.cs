#nullable enable
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;

public class ParquetV1DataSetFactory(IServiceProvider serviceProvider)
{
    public ParquetV1DataSet Create(Guid subjectId)
    {
        return ActivatorUtilities.CreateInstance<ParquetV1DataSet>(serviceProvider, subjectId);
    }
}
