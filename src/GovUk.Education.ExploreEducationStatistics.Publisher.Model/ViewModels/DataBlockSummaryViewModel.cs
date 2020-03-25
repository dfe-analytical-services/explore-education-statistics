using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class DataBlockSummaryViewModel
    {
        public List<Guid> DataKeys { get; set; }

        public List<string> DataSummary { get; set; }

        public List<string> DataDefinition { get; set; }

        public List<string> DataDefinitionTitle { get; set; }
    }
}