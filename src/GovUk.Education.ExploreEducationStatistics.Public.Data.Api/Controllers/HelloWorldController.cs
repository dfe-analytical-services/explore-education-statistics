using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiController]
[Route("/HelloWorld")]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public string Get()
    {
        return "Hello world";
    }
}
