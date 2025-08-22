#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class EinContentSection
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    [MaxLength(255)] // @MarkFix check this
    public string Heading { get; set; } = string.Empty;
    [MaxLength(2048)] // @MarkFix check this
    public string? Caption { get; set; } // NOTE: This column is currently unused, but keeping it to be aligned with releases/methodologies ContentSections
    public Guid EducationInNumbersPageId { get; set; } // @MarkFix rename PageId (and below to just Page)?
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
    HtmlBlock,
}

public class EinHtmlBlock : EinContentBlock
{
    public string Body { get; set; } = string.Empty; // @MarkFix rename to HtmlContent?
}
