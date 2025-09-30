#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Release : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; }

    public required Guid PublicationId { get; set; }

    public Publication Publication { get; set; } = null!;

    public required string Slug { get; set; } = string.Empty;

    public required TimeIdentifier TimePeriodCoverage { get; set; }

    public required int Year { get; set; }

    public string? Label { get; set; }

    public List<ReleaseVersion> Versions { get; set; } = [];

    public List<ReleaseRedirect> ReleaseRedirects { get; set; } = [];

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public string TimePeriod =>
        TimePeriodLabelFormatter.Format(
            Year,
            TimePeriodCoverage,
            TimePeriodLabelFormat.FullLabelBeforeYear
        );

    public string Title => string.IsNullOrEmpty(Label) ? TimePeriod : $"{TimePeriod} {Label}";

    public string YearTitle => TimePeriodLabelFormatter.FormatYear(Year, TimePeriodCoverage);

    internal class Config : IEntityTypeConfiguration<Release>
    {
        public void Configure(EntityTypeBuilder<Release> builder)
        {
            builder.Property(m => m.Slug).HasMaxLength(51);

            builder.Property(m => m.Label).HasMaxLength(20);

            builder
                .Property(m => m.TimePeriodCoverage)
                .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>())
                .HasMaxLength(5);

            builder
                .HasIndex(dsv => new
                {
                    dsv.PublicationId,
                    dsv.Year,
                    dsv.TimePeriodCoverage,
                    dsv.Label,
                })
                .IsUnique()
                .HasFilter(null);
        }
    }
}
