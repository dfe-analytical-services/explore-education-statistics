#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;

public interface IDataFilesPathResolver
{
    string ParquetV1Path(File file);
}
