using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderResult
    {
        public Guid PublicationId { get; set; }
        public Guid ReleaseId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public IEnumerable<ITableBuilderData> Result { get; set; }
    }
}