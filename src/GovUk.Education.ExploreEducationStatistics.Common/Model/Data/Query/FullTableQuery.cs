#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query
{
    public record FilterHierarchyOptions
    {
        public Guid LeafFilterId { get; set; }
        public List<FilterHierarchyOption> Options { get; set; } = [];
    }

    public record FullTableQuery
    {
        public Guid SubjectId { get; set; }

        public List<Guid> LocationIds { get; set; } = new();

        public TimePeriodQuery? TimePeriod { get; set; }

        public IEnumerable<Guid> Filters
        {
            [Obsolete("Use GetFilterItemIds() or GetNonHierarchicalFilterItemIds")]
            get; 
            set;
        } = new List<Guid>();

        public IEnumerable<Guid> Indicators { get; set; } = new List<Guid>();

        // List<FilterHierarchyOption> is all the options selected for a specific hierarchy.
        // The Dictionary potentially stores FilterHierarchyOptions for multiple hierarchies
        // and each list of Guids associated with a particular filter hierarchy option/checkbox.
        // FilterHierarchiesOptions Key is the leaf FilterId for the hierarchy the Value's FilterItemIds are associated with.
        [JsonConverter(typeof(FilterHierarchiesOptionsConverter))]
        public List<FilterHierarchyOptions>? FilterHierarchiesOptions { get; set; } = null;

        public IEnumerable<Guid> GetNonHierarchicalFilterItemIds()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return Filters;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public List<Guid> GetFilterItemIds()
        {
            var filterItemIds = GetNonHierarchicalFilterItemIds().ToList();
            if (FilterHierarchiesOptions != null)
            {
                filterItemIds.AddRange(FilterHierarchiesOptions
                    .SelectMany(filterHierarchyOptions => filterHierarchyOptions.Options
                            .SelectMany(hierarchyOption => hierarchyOption)));
            }

            // NOTE: We don't include FilterHierarchiesOptions.LeafFilterIds as they are filterIds, not filterItemIds

            return filterItemIds.Distinct().ToList();
        }

        public static IDictionary<Guid, List<FilterHierarchyOption>>? FilterHierarchiesOptionsAsDictionary(List<FilterHierarchyOptions>? hierarchiesOptions) // @MarkFix remove when Json output formatter is done
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

        public static List<FilterHierarchyOptions>? CreateFilterHierarchiesOptionsFromJson(string json) // @MarkFix should live somewhere else?
        {
            var hierarchiesOptionsDict =
                JsonConvert.DeserializeObject<IDictionary<Guid, List<FilterHierarchyOption>>>(json);

            return CreateFilterHierarchiesOptionsFromDictionary(hierarchiesOptionsDict);
        }

        public static List<FilterHierarchyOptions>? CreateFilterHierarchiesOptionsFromDictionary(IDictionary<Guid, List<List<Guid>>>? hierarchiesOptionsDict) // @MarkFix should live somewhere else?
        {
            if (hierarchiesOptionsDict == null || !hierarchiesOptionsDict.Any())
            {
                return null;
            }

            List<FilterHierarchyOptions> result = [];

            foreach (var key in hierarchiesOptionsDict.Keys)
            {
                var hierarchyOptions = hierarchiesOptionsDict[key];
                result.Add(new FilterHierarchyOptions
                {
                    LeafFilterId = key,
                    Options = hierarchyOptions
                });
            }

            return result;
        }
    }
}
