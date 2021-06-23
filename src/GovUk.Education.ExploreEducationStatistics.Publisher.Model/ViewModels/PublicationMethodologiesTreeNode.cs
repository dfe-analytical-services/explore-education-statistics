using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class PublicationMethodologiesTreeNode : AbstractPublicationTreeNode
    {
        public List<MethodologySummaryViewModel> Methodologies { get; set; }
    }
}
