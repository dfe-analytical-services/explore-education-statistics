#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class EinContentSection
{
    public Guid Id { get; set; }
    public int Order { get; set; }

    [MaxLength(255)]
    public string Heading { get; set; } = string.Empty;
    public Guid EducationInNumbersPageId { get; set; }
    public EducationInNumbersPage EducationInNumbersPage { get; set; } = null!;
    public List<EinContentBlock> Content { get; set; } = [];
}

public abstract class EinContentBlock
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public Guid EinContentSectionId { get; set; }
    public EinContentSection EinContentSection { get; set; } = null!;
}

public enum EinBlockType
{
    // NOTE: Update ContentDbContext.ConfigureEinContentBlock if you add a new type!
    HtmlBlock,
    TileGroupBlock,
}

public class EinHtmlBlock : EinContentBlock
{
    public string Body { get; set; } = string.Empty;
}

public class EinTileGroupBlock : EinContentBlock
{
    [MaxLength(1024)]
    public string? Title { get; set; }
    public List<EinTile> Tiles { get; set; } = [];
}

public class EinTile
{
    public Guid Id { get; set; } // @MarkFix Add required to all these?

    [MaxLength(2048)]
    public string? Title { get; set; }

    public int Order { get; set; }

    public Guid EinParentBlockId { get; set; }
    public EinTileGroupBlock EinParentBlock { get; set; } = null!;

    internal class EinTileConfig : IEntityTypeConfiguration<EinTile> // @MarkFix redo migration
    {
        public void Configure(EntityTypeBuilder<EinTile> builder)
        {
            builder
                .HasDiscriminator<string>("Type")
                .HasValue<EinFreeTextStatTile>("FreeTextStatTile")
                .HasValue<EinApiQueryStatTile>("ApiQueryStatTile");

            builder.Property(o => o.Title).HasMaxLength(2048);
        }
    }
}

public enum EinTileType
{
    // NOTE: Update ContentDbContext.ConfigureEinTile if you add a new type!
    FreeTextStatTile,
    ApiQueryStatTile,
}

public class EinFreeTextStatTile : EinTile
{
    public string? Statistic { get; set; }
    public string? Trend { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkText { get; set; }
}

public class EinApiQueryStatTile : EinTile
{
    public Guid? DataSetId { get; set; }
    public string? Version { get; set; }
    public string? LatestPublishedVersion { get; set; }
    public string? Query { get; set; }
    public string? Statistic { get; set; }
    public IndicatorUnit? IndicatorUnit { get; set; }
    public int? DecimalPlaces { get; set; }
    public string? QueryResult { get; set; }
    public Guid? ReleaseId { get; set; }
    public Release? Release { get; set; }

    internal class EinApiQueryStatTileConfig : IEntityTypeConfiguration<EinApiQueryStatTile>
    {
        public void Configure(EntityTypeBuilder<EinApiQueryStatTile> builder)
        {
            builder.Property(e => e.IndicatorUnit).HasConversion(new EnumToStringConverter<IndicatorUnit>());

            builder.Property(o => o.Version).HasMaxLength(32);
            builder.Property(o => o.LatestPublishedVersion).HasMaxLength(32);
            builder.Property(o => o.Statistic).HasMaxLength(64);
        }
    }
}
