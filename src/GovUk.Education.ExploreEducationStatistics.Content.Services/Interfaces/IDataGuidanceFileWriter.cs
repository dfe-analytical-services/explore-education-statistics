#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IDataGuidanceFileWriter
{
    Task<Stream> WriteToStream(Stream stream,
        ReleaseVersion releaseVersion,
        IList<Guid>? dataFileIds = null);
}
