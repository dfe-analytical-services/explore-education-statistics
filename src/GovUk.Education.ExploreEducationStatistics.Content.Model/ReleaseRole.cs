using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using Newtonsoft.Json;
using static System.DateTime;
using static System.String;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PartialDate;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public enum ReleaseRole
    {
        View,
        Edit,
        Review,
    }
}