#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Update : ICreatedTimestamp<DateTime?>
{
    public Guid Id { get; set; }

    public Guid ReleaseVersionId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    // Nullable to support older records missing Created timestamps
    public DateTime? Created { get; set; }

    // Nullable to support older records missing CreatedBy references
    public User? CreatedBy { get; set; }

    // Nullable to support older records missing CreatedBy references
    public Guid? CreatedById { get; set; }

    // TODO EES-6490 Convert 'On' from DateTime to DateTimeOffset
    public DateTime On { get; set; }

    public string Reason { get; set; } = null!;

    internal class Config : IEntityTypeConfiguration<Update>
    {
        public void Configure(EntityTypeBuilder<Update> builder)
        {
            // Values of 'On' are stored in the db in a datetime2 column - a raw value without timezone offset or kind.
            // Reapply a DateTimeKind when reading from database results to avoid values with DateTimeKind.Unspecified,
            // which are ambiguous in API responses due to being serialised to JSON without an offset.
            // TODO EES-6490 Convert 'On' from DateTime to DateTimeOffset
            builder.Property(u => u.On)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v,
                        // Updates are created by ReleaseNoteService in local time using DateTime.Now,
                        // so reapply DateTimeKind.Local here rather than DateTimeKind.Utc.
                        // It's different to the rest of the service where new date values use DateTime.UtcNow,
                        // where the kind would be reapplied with DateTimeKind.Utc here.
                        DateTimeKind.Local));
        }
    }
}
