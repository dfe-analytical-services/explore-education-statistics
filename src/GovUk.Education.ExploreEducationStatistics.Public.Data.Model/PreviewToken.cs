using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class PreviewToken : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; set; } = UuidUtils.UuidV7();

    public required string Label { get; set; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required Guid CreatedByUserId { get; set; }

    public DateTimeOffset Created { get; set; }

    public required DateTimeOffset Activates { get; set; }

    public required DateTimeOffset Expires { get; set; }

    public DateTimeOffset? Updated { get; set; }

    public PreviewTokenStatus Status => GetPreviewTokenStatus();

    private PreviewTokenStatus GetPreviewTokenStatus(DateTimeOffset? now = null)
    {
        now??= DateTimeOffset.UtcNow;
        return now >= Expires ? PreviewTokenStatus.Expired :
            now < Activates ? PreviewTokenStatus.Pending : PreviewTokenStatus.Active;
    }

    internal class Config : IEntityTypeConfiguration<PreviewToken>
    {
        public void Configure(EntityTypeBuilder<PreviewToken> builder)
        {
            builder.Property(pt => pt.Id)
                .HasValueGenerator<UuidV7ValueGenerator>();

            builder.Property(pt => pt.Label)
                .HasMaxLength(100);
        }
    }
}

public enum PreviewTokenStatus
{
    Active,
    Pending,
    Expired
}
