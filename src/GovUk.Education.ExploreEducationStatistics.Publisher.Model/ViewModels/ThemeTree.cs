using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class ThemeTree<TreeNode> where TreeNode : IPublicationTreeNode
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public List<TopicTree<TreeNode>> Topics { get; set; }
    }
}