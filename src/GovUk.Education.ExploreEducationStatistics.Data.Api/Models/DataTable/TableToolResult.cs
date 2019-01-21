using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.DataTable
{
    public class TableToolResult
    {
        public Guid PublicationId { get; set; }
        public Guid ReleaseId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public IEnumerable<TableToolData> Result { get; set; }
    }
}