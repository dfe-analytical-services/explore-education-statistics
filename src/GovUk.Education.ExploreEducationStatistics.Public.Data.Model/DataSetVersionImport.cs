using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Semver;

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

    public SemVersion? DataSetVersionToReplace { get; set; }
    
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

            builder.Property(i => i.DataSetVersionToReplace)
                .HasColumnType("varchar(50)")
                .HasConversion(
                    value => value == null ? null : value.ToString(),
                    value => ConvertStringToSemVersion(value)
                );
        }

        private static SemVersion? ConvertStringToSemVersion(string? value)
        {
            return value == null ? null : SemVersion.Parse(
                value,
                SemVersionStyles.OptionalMinorPatch
                | SemVersionStyles.AllowWhitespace
                | SemVersionStyles.AllowLowerV);
        }
    }
}
