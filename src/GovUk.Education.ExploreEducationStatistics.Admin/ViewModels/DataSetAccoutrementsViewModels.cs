using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DataSetAccoutrementsViewModel
{
    public List<DataBlockAccoutrementViewModel> DataBlocks { get; set; }
    public List<FootnoteAccoutrementViewModel> Footnotes { get; set; }
}

public record DataBlockAccoutrementViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public record FootnoteAccoutrementViewModel
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
}
