using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    [Table("ObservationRowFilterItem")]
    public class ObservationFilterItem
    {
        public Observation Observation { get; set; }
        public long ObservationId { get; set; }
        public Guid OldObservationId { get; set; }
        public FilterItem FilterItem { get; set; }
        public Guid FilterItemId { get; set; }
    }
}