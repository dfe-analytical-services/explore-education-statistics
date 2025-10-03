#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public record FreeTextRank(Guid Id, int Rank)
{
    public class Config : IEntityTypeConfiguration<FreeTextRank>
    {
        public void Configure(EntityTypeBuilder<FreeTextRank> builder)
        {
            builder.HasNoKey().ToTable((string?)null);
        }
    }
}

public record FreeTextValueResult<TValue>
{
    public TValue Value { get; init; }
    public int Rank { get; init; }
}
