using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class PublicationMetaViewModel
    {
        public Guid PublicationId { get; set; }
        public Dictionary<string, List<NameLabelViewModel>> Attributes { get; set; }
        public Dictionary<string, List<NameLabelViewModel>> Characteristics { get; set; }
    }
}