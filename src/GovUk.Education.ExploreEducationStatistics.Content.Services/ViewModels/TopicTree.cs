#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record TopicTree<TNode> where TNode : BasePublicationTreeNode
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Summary { get; init; } = string.Empty;

        public List<TNode> Publications { get; init; } = new();

        public virtual bool Equals(TopicTree<TNode>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Title == other.Title && Summary == other.Summary 
                   && Publications.SequenceEqual(other.Publications);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Title, Summary, Publications);
        }
    }
}
