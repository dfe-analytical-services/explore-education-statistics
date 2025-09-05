#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class EinContentSection
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    [MaxLength(255)]
    public string Heading { get; set; } = string.Empty;
    [MaxLength(2048)]
    public string? Caption { get; set; } // NOTE: This column is currently unused, but keeping it to be aligned with releases/methodologies ContentSections
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
}

public class EinHtmlBlock : EinContentBlock
{
    public string Body { get; set; } = string.Empty;
}
