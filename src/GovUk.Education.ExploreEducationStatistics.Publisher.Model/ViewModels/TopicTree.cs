using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class TopicTree<TNode> where TNode : IPublicationTreeNode
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public List<TNode> Publications { get; set; }
    }
}
