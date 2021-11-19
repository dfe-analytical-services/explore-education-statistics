#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public record LocationAttributeNode
    {
        public ILocationAttribute? Attribute { get; init; }

        public List<LocationAttributeNode> Children { get; init; } = new();

        public bool IsLeaf => Children.Count == 0;
    }
}
