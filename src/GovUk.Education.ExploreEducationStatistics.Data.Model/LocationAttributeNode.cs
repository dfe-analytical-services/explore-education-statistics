#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public record LocationAttributeNode
    {
        public LocationAttribute Attribute { get; init; }

        public List<LocationAttributeNode> Children { get; init; } = new();

        public Guid? LocationId { get; set; }

        public bool IsLeaf => Children.Count == 0;

        public LocationAttributeNode(LocationAttribute attribute)
        {
            Attribute = attribute;
        }

        public List<LocationAttribute> GetLeafAttributes()
        {
            if (IsLeaf)
            {
                return ListOf(Attribute);
            }

            return Children
                .SelectMany(child => child.GetLeafAttributes())
                .ToList();
        }
    }
}
