using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class CreatePublicationViewModel
    {
        public string title { get; set; }

        public Guid TopicId { get; set; }

        public Guid? MethodologyId { get; set; }

        public Guid Contact { get; set; }
    }
}