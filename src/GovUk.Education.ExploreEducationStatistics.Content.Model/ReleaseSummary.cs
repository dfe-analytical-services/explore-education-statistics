using System;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseSummary : AbstractVersioned<ReleaseSummaryVersion>
    {
        public Guid Id { get; set; }

        public Guid ReleaseId { get; set; }

        public Release Release { get; set; }

        [NotMapped] public string Title => Current?.Title;

        [NotMapped] public string YearTitle => Current?.YearTitle;

        [NotMapped] public string CoverageTitle => Current?.CoverageTitle;

        [NotMapped] public string ReleaseName => Current?.ReleaseName;

        [NotMapped] public DateTime? PublishScheduled => Current?.PublishScheduled;

        [NotMapped] public string Slug => Current?.Slug;

        [NotMapped] public string Summary => Current?.Summary;

        [NotMapped] public ReleaseType Type => Current?.Type;

        [NotMapped] public Guid? TypeId => Current?.TypeId;

        [NotMapped] public TimeIdentifier? TimePeriodCoverage => Current?.TimePeriodCoverage;

        [NotMapped] public PartialDate NextReleaseDate => Current?.NextReleaseDate;

        [NotMapped] public DateTime? Created => Current?.Created;
    }
}