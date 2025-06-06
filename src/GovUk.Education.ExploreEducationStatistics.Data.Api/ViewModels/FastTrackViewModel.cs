using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

public record FastTrackViewModel
{
    /// <summary>
    /// This "DataBlockParentId" field is the Id that is unchanging between versions of the same DataBlock
    /// throughout Release Amendments. We will use [JsonPropertyName] to ensure that existing JSON is unchanged
    /// for now, whilst being able to be explicit about which Id is being referred to in the back-end code.
    /// </summary>
    [JsonProperty("id")]
    public Guid DataBlockParentId { get; set; }

    public TableBuilderConfiguration Configuration { get; set; }

    public TableBuilderResultViewModel FullTable { get; set; }

    public TableBuilderQueryViewModel Query { get; set; }

    public Guid ReleaseId { get; set; }

    public string ReleaseSlug { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType ReleaseType { get; set; }

    public bool LatestData { get; set; }

    public string LatestReleaseTitle { get; set; }
 
    public string LatestReleaseSlug { get; set; }
}
