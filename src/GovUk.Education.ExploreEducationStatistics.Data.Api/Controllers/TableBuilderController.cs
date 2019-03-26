using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IReleaseService _releaseService;

        public TableBuilderController(
            ITableBuilderService tableBuilderService,
            IReleaseService releaseService)
        {
            _tableBuilderService = tableBuilderService;
            _releaseService = releaseService;
        }

        [HttpPost("geographic")]
        public ActionResult<TableBuilderResult> GetGeographic([FromBody] GeographicQueryContext query)
        {
            var result = _tableBuilderService.GetGeographic(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }

        [HttpPost("characteristics/local-authority")]
        public ActionResult<TableBuilderResult> GetLocalAuthority([FromBody] LaQueryContext query)
        {
            var result = _tableBuilderService.GetLocalAuthority(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }

        [HttpPost("characteristics/national")]
        public ActionResult<TableBuilderResult> GetNational(NationalQueryContext query)
        {
            var result = _tableBuilderService.GetNational(query);
            if (result.Result.Any())
            {
                return result;
            }

            return NotFound();
        }

        [HttpGet("meta/{typeName}/{publicationId}")]
        public ActionResult<PublicationMetaViewModel> GetMeta(string typeName, Guid publicationId)
        {
            var result = new PublicationMetaViewModel
            {
                PublicationId = publicationId,
                Indicators = _releaseService.GetIndicatorMetas(publicationId, typeName),
                Characteristics = _releaseService.GetCharacteristicMetas(publicationId, typeName)
            };

            if (result.Indicators != null && result.Indicators.Any() ||
                result.Characteristics != null && result.Characteristics.Any())
            {
                result.ObservationalUnits = new ObservationalUnitsViewModel
                {
                    Country = new List<NameLabelViewModel>
                    {
                        new NameLabelViewModel {Name = "E92000001", Label = "England"}
                    },
                    LocalAuthority = new List<NameLabelViewModel>
                    {
                        new NameLabelViewModel
                        {
                            Name = "E09000007",
                            Label = "Camden"
                        },
                        new NameLabelViewModel
                        {
                            Name = "E09000001",
                            Label = "City of London"
                        },
                        new NameLabelViewModel
                        {
                            Name = "E09000011",
                            Label = "Greenwich"
                        }
                    },
                    Region = new List<NameLabelViewModel>
                    {
                        new NameLabelViewModel
                        {
                            Name = "E13000001",
                            Label = "Inner London"
                        },
                        new NameLabelViewModel
                        {
                            Name = "E13000002",
                            Label = "Outer London"
                        }
                    },
                    TimePeriod = new TimePeriodMetaViewModel
                    {
                        Hint = "Filter statistics by a given start and end date",
                        Legend = "Academic Year",
                        Options = new List<NameLabelViewModel>
                        {
                            new NameLabelViewModel
                            {
                                Name = "201112",
                                Label = "2011/12"
                            },
                            new NameLabelViewModel
                            {
                                Name = "201213",
                                Label = "2012/13"
                            },
                            new NameLabelViewModel
                            {
                                Name = "201314",
                                Label = "2013/14"
                            },
                            new NameLabelViewModel
                            {
                                Name = "201415",
                                Label = "2014/15"
                            },
                            new NameLabelViewModel
                            {
                                Name = "201516",
                                Label = "2015/16"
                            },
                            new NameLabelViewModel
                            {
                                Name = "201617",
                                Label = "2016/17"
                            }
                        }
                    }
                };

                return result;
            }

            return NotFound();
        }
    }
}