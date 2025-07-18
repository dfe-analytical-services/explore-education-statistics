#nullable enable
using System;
using Generator.Equals;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

[Equatable]
public partial class Organisation : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Url { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<Organisation>
    {
        public void Configure(EntityTypeBuilder<Organisation> builder)
        {
            builder.Property(o => o.Title)
                .HasMaxLength(100);

            builder.Property(o => o.Url)
                .HasMaxLength(1024);

            builder.HasIndex(o => o.Title)
                .IsUnique();
        }
    }
}
