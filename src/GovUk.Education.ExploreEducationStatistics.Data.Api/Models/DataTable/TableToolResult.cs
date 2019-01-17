using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class TableToolResult
    {
        public Guid Publication { get; set; }
        public Guid Release { get; set; }
        public IEnumerable<TableToolData> Result { get; set; }
    }
}