#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ReleaseChecklistViewModel
    {
        public bool Valid => !Errors.Any();
        public List<ReleaseChecklistIssue> Errors { get; }
        public List<ReleaseChecklistIssue> Warnings { get; }

        public ReleaseChecklistViewModel(List<ReleaseChecklistIssue> errors, List<ReleaseChecklistIssue> warnings)
        {
            Errors = errors;
            Warnings = warnings;
        }
    }

    public class ReleaseChecklistIssue
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ValidationErrorMessages Code { get; }

        public ReleaseChecklistIssue(ValidationErrorMessages code)
        {
            Code = code;
        }
    }

    public class MethodologyNotApprovedWarning : ReleaseChecklistIssue
    {
        public Guid MethodologyId { get; }

        public MethodologyNotApprovedWarning(Guid methodologyId) : base(ValidationErrorMessages.MethodologyNotApproved)
        {
            MethodologyId = methodologyId;
        }
    }

    public class NoFootnotesOnSubjectsWarning : ReleaseChecklistIssue
    {
        public int TotalSubjects { get; }

        public NoFootnotesOnSubjectsWarning(int totalSubjects) : base(ValidationErrorMessages.NoFootnotesOnSubjects)
        {
            TotalSubjects = totalSubjects;
        }
    }
}