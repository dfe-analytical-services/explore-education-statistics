#nullable enable
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

    /// <summary>
    /// Indicates whether the organisation is a government body that should use the Government Information Service (GIS) logo format.
    /// </summary>
    /// <remarks>
    /// GIS logos use a common format which should be constructed from markup, whereas non-GIS logos may use an image file.
    /// </remarks>
    public bool UseGISLogo { get; set; }

    /// <summary>
    /// GIS logos contain a vertical line which may vary in colour depending on the department or organisation. This property identifies the colour of the line as a hex code (e.g. the DfE uses #003764).
    /// </summary>
    public string? GISLogoHexCode { get; set; }

    /// <summary>
    /// The name of the image which should be used for the organisation's logo.
    /// <remarks>
    /// Government Information Service (GIS) organisations require an official crest, whereas non-GIS organisations may use any image file for their logo.
    /// </remarks>
    /// </summary>
    public string LogoFileName { get; set; } = string.Empty;

    internal class Config : IEntityTypeConfiguration<Organisation>
    {
        public void Configure(EntityTypeBuilder<Organisation> builder)
        {
            builder.Property(o => o.Title).HasMaxLength(100);
            builder.Property(o => o.Url).HasMaxLength(1024);
            builder.Property(o => o.GISLogoHexCode).HasMaxLength(7);
            builder.Property(o => o.LogoFileName).HasMaxLength(255);

            builder.HasIndex(o => o.Title).IsUnique();

            builder.ToTable(t =>
                t.HasCheckConstraint(
                    "CK_Organisations_GISLogoHexCode",
                    "[UseGISLogo] = CASE WHEN [GISLogoHexCode] IS NULL THEN 0 ELSE 1 END"
                )
            );
        }
    }
}
