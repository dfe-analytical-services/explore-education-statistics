using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class PublicationMetaViewModel
    {
        public Guid PublicationId { get; set; }
        public Dictionary<string, List<IndicatorMetaViewModel>> Indicators { get; set; }
        public Dictionary<string, List<NameLabelViewModel>> Characteristics { get; set; }
        public ObservationalUnitsViewModel ObservationalUnits { get; set; }
    }
}