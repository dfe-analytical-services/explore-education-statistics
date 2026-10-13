#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFile
{
    public Guid Id { get; set; }

    public List<DataSetFileVersion> Versions { get; set; } = [];

    // TODO EES-5105 Implement ICreatedTimestamp once DataSetFileDualWriteInterceptor is removed.
    // Until then, Created is copied from the earliest data File so must not be stamped by DbContextUtils.
    public DateTime? Created { get; set; }

    internal class Config : IEntityTypeConfiguration<DataSetFile>
    {
        public void Configure(EntityTypeBuilder<DataSetFile> builder)
        {
            builder
                .Property(dsf => dsf.Created)
                .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
        }
    }
}
