using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderResult
    {
        public Guid PublicationId { get; set; }
        public long ReleaseId { get; set; }
        public DateTime ReleaseDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public GeographicLevel GeographicLevel { get; set; }

        public IEnumerable<ITableBuilderData> Result { get; set; }

        public TableBuilderResult()
        {
            Result = new List<ITableBuilderData>();
        }
    }
}