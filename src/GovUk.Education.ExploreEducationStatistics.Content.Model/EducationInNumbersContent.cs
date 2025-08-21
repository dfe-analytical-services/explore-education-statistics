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
    public string? Caption { get; set; }

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

public class EinHtmlBlock : EinContentBlock
{
    public string Body { get; set; } = string.Empty; // @MarkFix rename to HtmlContent?
}
