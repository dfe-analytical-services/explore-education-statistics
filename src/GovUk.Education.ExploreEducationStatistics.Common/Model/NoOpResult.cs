using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

/// <summary>
/// Action result that does nothing. Only required in situations
/// where the action has already been handled by the controller
/// and no more processing is required.
/// </summary>
public class NoOpResult : ActionResult;
