using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Chart
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum ChartLineStyle
    {
        solid,
        dashed,
        dotted
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum ChartLineSymbol
    {
        circle,
        cross,
        diamond,
        square,
        star,
        triangle,
        wye,
        none
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum ChartInlinePosition
    {
        above, below
    }
}
