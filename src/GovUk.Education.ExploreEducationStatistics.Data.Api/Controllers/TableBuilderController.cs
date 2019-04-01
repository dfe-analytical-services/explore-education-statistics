using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableBuilderController : ControllerBase
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IReleaseService _releaseService;
        private readonly ISubjectService _subjectService;

        public TableBuilderController(
            ITableBuilderService tableBuilderService,
            IReleaseService releaseService,
            ISubjectService subjectService)
        {
            _tableBuilderService = tableBuilderService;
            _releaseService = releaseService;
            _subjectService = subjectService;
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

        [HttpGet("meta/{publicationId}")]
        public ActionResult<PublicationSubjectsMetaViewModel> GetMeta(Guid publicationId)
        {
            var subjectMetaViewModels = _subjectService.GetSubjectMetas(publicationId);

            if (subjectMetaViewModels == null)
            {
                return NotFound();
            }
            
            return new PublicationSubjectsMetaViewModel
            {
                PublicationId = publicationId,
                Subjects = subjectMetaViewModels
            };
        }

        [HttpGet("meta/{typeName}/{publicationId}")]
        [Obsolete("Use subject instead")]
        // TODO Remove me - Get meta by subject instead
        public ActionResult<PublicationMetaViewModel> GetSubjectMeta(string typeName, Guid publicationId)
        {
            // TODO Remove me once UI updated to get Meta by Subject.
            // TODO Currently the UI only requests meta data for type "CharacteristicDataNational"
            if (typeName == "CharacteristicDataNational")
            {
                var latestRelease = _releaseService.GetLatestRelease(publicationId);

                var subjectForPublication = _subjectService.FindMany(subject =>
                    subject.Release.Id == latestRelease &&
                    subject.Name == "National characteristics"
                ).FirstOrDefault();

                return GetSubjectMeta(subjectForPublication.Id);
            }

            return NotFound();
        }

        [HttpGet("meta/subject/{subjectId}")]
        public ActionResult<PublicationMetaViewModel> GetSubjectMeta(long subjectId)
        {
            var subject = _subjectService.Find(subjectId, new List<Expression<Func<Subject, object>>> {s => s.Release});

            return new PublicationMetaViewModel
            {
                PublicationId = subject.Release.PublicationId,
                Characteristics = _subjectService.GetCharacteristicMetas(subject),
                Indicators = _subjectService.GetIndicatorMetas(subject),
                ObservationalUnits = new ObservationalUnitsMetaViewModel
                {
                    Location = new LocationMetaViewModel
                    {
                        LocalAuthority = new List<LabelValueViewModel>
                        {
                            new LabelValueViewModel
                            {
                                Label = "Camden",
                                Value = "E09000007"
                            },
                            new LabelValueViewModel
                            {
                                Label = "City of London",
                                Value = "E09000001"
                            },
                            new LabelValueViewModel
                            {
                                Label = "Greenwich",
                                Value = "E09000011"
                            }
                        },
                        National = new List<LabelValueViewModel>
                        {
                            new LabelValueViewModel {Label = "England", Value = "E92000001"}
                        },
                        Region = new List<LabelValueViewModel>
                        {
                            new LabelValueViewModel
                            {
                                Label = "Inner London",
                                Value = "E13000001"
                            },
                            new LabelValueViewModel
                            {
                                Label = "Outer London",
                                Value = "E13000002"
                            }
                        },
                        School = new List<LabelValueViewModel>()
                    },
                    TimePeriod = new TimePeriodMetaViewModel
                    {
                        Hint = "Filter statistics by a given start and end date",
                        Legend = "Academic Year",
                        Options = new List<LabelValueViewModel>
                        {
                            new LabelValueViewModel
                            {
                                Label = "2011/12",
                                Value = "201112"
                            },
                            new LabelValueViewModel
                            {
                                Label = "2012/13",
                                Value = "201213"
                            },
                            new LabelValueViewModel
                            {
                                Label = "2013/14",
                                Value = "201314"
                            },
                            new LabelValueViewModel
                            {
                                Label = "2014/15",
                                Value = "201415"
                            },
                            new LabelValueViewModel
                            {
                                Label = "2015/16",
                                Value = "201516"
                            },
                            new LabelValueViewModel
                            {
                                Label = "2016/17",
                                Value = "201617"
                            }
                        }
                    }
                }
            };
        }
    }
}