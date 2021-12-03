#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record ThemeTree<TNode> where TNode : BasePublicationTreeNode
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Summary { get; init; } = string.Empty;

        public List<TopicTree<TNode>> Topics { get; init; } = new();

        public virtual bool Equals(ThemeTree<TNode>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Title == other.Title && Summary == other.Summary 
                   && Topics.SequenceEqual(other.Topics);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Title, Summary, Topics);
        }
    }
}
