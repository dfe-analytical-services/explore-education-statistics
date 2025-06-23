using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Common;

public enum PageFeedbackResponse
{
    [EnumLabelValue("Useful")]
    Useful,

    [EnumLabelValue("Not useful")]
    NotUseful,

    [EnumLabelValue("Problem encountered")]
    ProblemEncountered,
}
