#nullable enable
using System.IO;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public class JsonFileUtils
{
    public static TContent? ReadJsonFile<TContent>(string path, JsonSerializerSettings? settings = null)
    {
        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<TContent>(json, settings);
    }
}
