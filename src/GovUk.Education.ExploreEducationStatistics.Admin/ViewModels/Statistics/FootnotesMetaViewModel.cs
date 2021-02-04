using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics
{
    public class FootnotesMetaViewModel
    {
        public Dictionary<Guid, FootnotesSubjectMetaViewModel> Subjects { get; set; }
    }

    public class FootnotesSubjectMetaViewModel
    {
        public Dictionary<Guid, FootnotesFilterMetaViewModel> Filters { get; set; }

        public Dictionary<Guid, FootnotesIndicatorsMetaViewModel> Indicators { get; set; }

        public Guid SubjectId { get; set; }

        public string SubjectName { get; set; }
    }

    public class FootnotesFilterMetaViewModel : LegendOptionsMetaValueModel<Dictionary<Guid, FootnotesFilterGroupsMetaViewModel>>
    {
    }

    public class FootnotesFilterGroupsMetaViewModel : LabelOptionsMetaValueModel<List<LabelValue>>
    {
    }

    public class FootnotesIndicatorsMetaViewModel : LabelOptionsMetaValueModel<IList<IndicatorMetaViewModel>>
    {
    }
}