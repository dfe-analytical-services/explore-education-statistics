#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class EducationInNumbersContentViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; } = string.Empty;

    public DateTimeOffset? Published { get; set; }

    public List<EinContentSectionViewModel> Content { get; set; } = []; // @MarkFix rename to Sections?
}

public class EinContentSectionViewModel
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    public string Heading { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public List<EinContentBlockViewModel> Content { get; set; } = new(); // @MarkFix rename to Blocks?
}

public enum EinBlockType
{
    HtmlBlock,
}

public class EinContentBlockViewModel
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    public EinBlockType Type { get; set; }
}

public class EinHtmlBlockViewModel : EinContentBlockViewModel
{
    public string Body { get; set; } = string.Empty;
}
