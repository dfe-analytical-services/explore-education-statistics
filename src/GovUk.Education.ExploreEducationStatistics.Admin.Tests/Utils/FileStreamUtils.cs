using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using System.IO;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils;

public static class FileStreamUtils
{
    public static async Task<MemoryStream> CreateMemoryStreamFromLocalResource(string fileName)
    {
        var memoryStream = new MemoryStream();
        using var fileStream = File.OpenRead(MockFormTestUtils.GetPathForFile(fileName));
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.SeekToBeginning();

        return memoryStream;
    }
}
