#nullable enable
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;

public class ParquetV1DataSetWriterFactory(IServiceProvider serviceProvider)
{
    public ParquetV1DataSetWriter Create(Guid subjectId)
    {
        return ActivatorUtilities.CreateInstance<ParquetV1DataSetWriter>(serviceProvider, subjectId);
    }
}
