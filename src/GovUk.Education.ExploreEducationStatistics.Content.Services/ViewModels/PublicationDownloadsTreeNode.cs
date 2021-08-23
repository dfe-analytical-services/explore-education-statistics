#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationDownloadsTreeNode : BasePublicationTreeNode
    {
        public string Summary { get; init; } = string.Empty;

        public List<FileInfo> DownloadFiles { get; init; } = new();

        public string? EarliestReleaseTime { get; init; }

        public string? LatestReleaseTime { get; init; }

        public Guid? LatestReleaseId { get; init; }
    }
}