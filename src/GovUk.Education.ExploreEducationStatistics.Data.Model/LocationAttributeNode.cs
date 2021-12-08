#nullable enable
using System.Collections.Generic;
using System.Linq;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public record LocationAttributeNode
    {
        public ILocationAttribute Attribute { get; init; }

        public List<LocationAttributeNode> Children { get; init; } = new();

        public bool IsLeaf => Children.Count == 0;

        public LocationAttributeNode(ILocationAttribute attribute)
        {
            Attribute = attribute;
        }

        public List<ILocationAttribute> GetLeafAttributes()
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
