#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DataSetUploadViewModel
{
    public Guid Id { get; init; }

    public required string DataSetTitle { get; init; }

    public required string DataFileName { get; init; }

    public required string MetaFileName { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetUploadStatus Status { get; set; }

    public DataSetScreenerResponse? ScreenerResult { get; set; }
}
