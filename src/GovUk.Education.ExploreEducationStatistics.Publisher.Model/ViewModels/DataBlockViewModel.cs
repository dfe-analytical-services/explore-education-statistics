﻿using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class DataBlockViewModel : IContentBlockViewModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Heading { get; set; }

        public string Name { get; set; }

        public string Source { get; set; }

        public ObservationQueryContext Query { get; set; }

        public List<IContentBlockChart> Charts { get; set; }

        public DataBlockSummaryViewModel Summary { get; set; }

        public TableBuilderConfiguration Table { get; set; }

        [Obsolete("Maintain compatibility with existing cached DataBlocks")]
        public List<TableBuilderConfiguration> Tables
        {
            set
            {
                if (value != null && value.Count > 0)
                {
                    Table = value[0];
                }
            }
        }

        public string Type => "DataBlock";
    }
}