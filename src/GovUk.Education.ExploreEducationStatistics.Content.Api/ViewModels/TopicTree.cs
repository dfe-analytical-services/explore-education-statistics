#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels
{
    public record TopicTree<TNode> where TNode : BasePublicationTreeNode
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Summary { get; init; } = string.Empty;

        public List<TNode> Publications { get; init; } = new();
    }
}
