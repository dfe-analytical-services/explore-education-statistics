using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetVersionImport : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; init; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required Guid InstanceId { get; set; }

    public required DataSetVersionImportStage Stage { get; set; }

    public DateTimeOffset? Completed { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    public Guid? DataSetVersionToReplaceId { get; init; }

    public DataSetVersion? DataSetVersionToReplace { get; init; }

    internal class Config : IEntityTypeConfiguration<DataSetVersionImport>
    {
        public void Configure(EntityTypeBuilder<DataSetVersionImport> builder)
        {
            builder.Property(i => i.Id)
                .HasValueGenerator<UuidV7ValueGenerator>();

            builder.Property(i => i.Stage)
                .HasConversion<string>();

            builder.HasIndex(i => i.InstanceId)
                .IsUnique();

            builder.HasOne(i => i.DataSetVersionToReplace)
                .WithMany()
                .HasForeignKey(i => i.DataSetVersionToReplaceId);
        }
    }
}
