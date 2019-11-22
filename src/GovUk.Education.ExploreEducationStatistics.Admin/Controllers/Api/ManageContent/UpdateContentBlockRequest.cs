using System.Runtime.Serialization;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent
{
    public class UpdateTextBasedContentBlockRequest
    {
        public string? Heading { get; set; }

        public string Body { get; set; }
    }
}