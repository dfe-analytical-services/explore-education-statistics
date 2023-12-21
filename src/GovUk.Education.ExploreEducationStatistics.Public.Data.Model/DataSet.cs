using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSet : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; init; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public required Guid PublicationId { get; set; }

    public required DataSetStatus Status { get; set; }

    public Guid? SupersedingDataSetId { get; set; }

    public DataSet? SupersedingDataSet { get; set; }

    public Guid? LatestVersionId { get; set; }

    public DataSetVersion? LatestVersion { get; set; }

    public List<DataSetVersion> Versions { get; set; } = [];

    public DateTimeOffset? Published { get; set; }

    public DateTimeOffset? Unpublished { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<DataSet>
    {
        public void Configure(EntityTypeBuilder<DataSet> builder)
        {
            builder.Property(ds => ds.Status).HasConversion<string>();

            builder
                .HasOne(ds => ds.LatestVersion)
                .WithOne(dsv => dsv.DataSet)
                .HasForeignKey<DataSet>(ds => ds.LatestVersionId);
        }
    }
}
