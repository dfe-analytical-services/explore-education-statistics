#nullable enable
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class DeleteDataBlockPlanViewModel
{
    [JsonIgnore] public Guid ReleaseId { get; set; }
    public List<DependentDataBlock> DependentDataBlocks { get; set; } = new();
}

public class DependentDataBlock
{
    [JsonIgnore] public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContentSectionHeading { get; set; }
    public List<InfographicFileInfo> InfographicFilesInfo { get; set; } = new();
    public bool IsKeyStatistic { get; set; }
    public FeaturedTableBasicViewModel? FeaturedTable { get; set; }
}

public class InfographicFileInfo
{
    public Guid Id { get; set; }
    public string Filename { get; set; } = "";
}
