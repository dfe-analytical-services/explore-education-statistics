#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public record BoundaryLevelViewModel
    {
        public long Id { get; }
        public string Label { get; }

        public BoundaryLevelViewModel(long id, string label)
        {
            Id = id;
            Label = label;
        }
    }
}
