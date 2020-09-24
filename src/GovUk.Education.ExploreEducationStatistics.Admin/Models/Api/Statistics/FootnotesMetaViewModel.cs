using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
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

    public class FootnotesFilterItemsMetaViewModel : LabelOptionsMetaValueModel<Dictionary<Guid, LabelValue>>
    {
    }

    public class FootnotesFilterMetaViewModel : LegendOptionsMetaValueModel<Dictionary<Guid, FootnotesFilterItemsMetaViewModel>>
    {
    }

    public class FootnotesIndicatorsMetaViewModel : LabelOptionsMetaValueModel<Dictionary<Guid, IndicatorMetaViewModel>>
    {
    }
}