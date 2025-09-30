#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public class FilterHierarchiesOptionsUtils
{
    public static IDictionary<
        Guid,
        List<FilterHierarchyOption>
    >? FilterHierarchiesOptionsAsDictionary(List<FilterHierarchyOptions>? hierarchiesOptions)
    {
        if (hierarchiesOptions is null)
        {
            return null;
        }

        var result = new Dictionary<Guid, List<FilterHierarchyOption>>();

        foreach (var hierarchyOptions in hierarchiesOptions)
        {
            var leafFilterId = hierarchyOptions.LeafFilterId;
            result.Add(leafFilterId, hierarchyOptions.Options);
        }

        return result;
    }

    public static List<FilterHierarchyOptions>? CreateFilterHierarchiesOptionsFromJson(string json)
    {
        var hierarchiesOptionsDict = JsonConvert.DeserializeObject<
            IDictionary<Guid, List<FilterHierarchyOption>>
        >(json);

        return CreateFilterHierarchiesOptionsFromDictionary(hierarchiesOptionsDict);
    }

    public static List<FilterHierarchyOptions>? CreateFilterHierarchiesOptionsFromDictionary(
        IDictionary<Guid, List<FilterHierarchyOption>>? hierarchiesOptionsDict
    )
    {
        if (hierarchiesOptionsDict == null || !hierarchiesOptionsDict.Any())
        {
            return null;
        }

        List<FilterHierarchyOptions> result = [];

        foreach (var key in hierarchiesOptionsDict.Keys)
        {
            var hierarchyOptions = hierarchiesOptionsDict[key];
            result.Add(
                new FilterHierarchyOptions { LeafFilterId = key, Options = hierarchyOptions }
            );
        }

        return result;
    }
}
