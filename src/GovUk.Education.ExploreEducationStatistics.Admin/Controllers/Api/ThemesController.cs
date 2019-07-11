
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    [Route("themes")]
    [ApiController]
    [Authorize]
    public class ThemeController : ControllerBase
    {

        private readonly IThemeService _themeService;
        
        public ThemeController(IThemeService themeService)
        {
            _themeService = themeService;
        }
        
        // GET api/themes?userId={guid}
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<List<Theme>> GetThemes([Required][FromQuery(Name ="userId")]Guid guid)
        {
            var result =  _themeService.GetThemes(guid);
            
            if (result.Any())
            {
                return result;
            }

            return NoContent();
        }
    }
}