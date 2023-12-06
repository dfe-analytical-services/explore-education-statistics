using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiController]
[Route("/HelloWorld")]
public class HelloWorldController(PublicDataDbContext publicDataDbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DataSet>>> Get()
    {
        return await publicDataDbContext.DataSets.ToListAsync();
    }

    [HttpPost]
    public async Task<DataSet> Create()
    {
        var dataSet = new DataSet
        {
            Status = DataSetStatus.Public,
            Title = "My first dataset",
            PublicationId = Guid.NewGuid(),
            Versions = new List<DataSetVersion>
            {
                new()
                {
                    CsvFileId = Guid.NewGuid(),
                    Notes = "fdshbab",
                    VersionMajor = 1,
                    VersionMinor = 0,
                    ParquetFilename = "thing.parquet",
                    MetaSummary = new DataSetVersionMetaSummary
                    {
                        Filters = new List<string>()
                        {
                            "school type",
                            "other type"
                        },
                        Indicators = new List<string>()
                        {
                            "thing"
                        }
                    },
                    Meta = new DataSetMeta
                    {
                        Filters = new List<FilterMeta>
                        {
                            new()
                            {
                                Identifier = "school_type",
                                Label = "School type",
                                Options = new List<FilterOptionMeta>
                                {
                                    new()
                                    {
                                        Identifier = "123",
                                        Label = "Primary"
                                    },
                                    new()
                                    {
                                        Identifier = "345",
                                        Label = "Secondary"
                                    }
                                },
                            }
                        },
                        Indicators = new List<IndicatorMeta>()
                        {
                            new()
                            {
                                Identifier = "number_pupils",
                                Label = "Number of pupils",
                            }
                        },
                        TimePeriods = new List<TimePeriodMeta>
                        {
                            new()
                            {
                                Year = 2020,
                                Code = TimeIdentifier.AcademicYear
                            }
                        },
                        Locations = new List<LocationMeta>
                        {
                            new()
                            {
                                Level = GeographicLevel.LocalAuthority,
                                Options = new List<LocationOptionMeta>
                                {
                                    new()
                                    {
                                        Identifier = "1",
                                        Label = "Barnsley",
                                        Code = "E00001"
                                    },
                                    new()
                                    {
                                        Identifier = "2",
                                        Label = "Sheffield",
                                        Code = "E00002"
                                    }
                                }
                            }
                        },
                        GeographicLevels = new List<GeographicLevel>
                        {
                            GeographicLevel.Country,
                            GeographicLevel.Region,
                            GeographicLevel.LocalAuthority
                        }
                    },
                    FilterChanges = new List<ChangeSetFilters>
                    {
                        new()
                        {
                            Changes = new List<Change<FilterChangeState>>
                            {
                                new()
                                {
                                    Type = ChangeType.Add,
                                    CurrentState = new FilterChangeState
                                    {
                                        Label = "dd",
                                        Hint = "dd",
                                        Id = "d"
                                    },
                                    PreviousState = new FilterChangeState
                                    {
                                        Label = "aaa",
                                        Hint = "dd",
                                        Id = "d"
                                    }
                                },
                                new()
                                {
                                    Type = ChangeType.Add,
                                    CurrentState = new FilterChangeState
                                    {
                                        Label = "aa",
                                        Hint = "aa",
                                        Id = "a"
                                    },
                                    PreviousState = new FilterChangeState
                                    {
                                        Label = "aaa",
                                        Hint = "ddc",
                                        Id = "d"
                                    }
                                }
                            }
                        }
                    },
                    IndicatorChanges = new List<ChangeSetIndicators>
                    {
                        new()
                        {
                            Changes = new List<Change<IndicatorChangeState>>
                            {
                                new()
                                {
                                    Type = ChangeType.Update,
                                    CurrentState = new IndicatorChangeState
                                    {
                                        Label = "aa",
                                        Id = "a",
                                        DecimalPlaces = 3,
                                        Unit = "%"
                                    },
                                    PreviousState = new IndicatorChangeState
                                    {
                                        Label = "aaa",
                                        Id = "d",
                                        DecimalPlaces = 2,
                                        Unit = "%"
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        publicDataDbContext.DataSets.Add(dataSet);

        await publicDataDbContext.SaveChangesAsync();

        return null!;
    }
}
