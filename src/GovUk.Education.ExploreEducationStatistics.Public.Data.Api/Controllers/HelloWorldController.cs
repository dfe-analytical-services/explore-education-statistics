using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiController]
[Route("/HelloWorld")]
public class HelloWorldController : ControllerBase
{
    private readonly PublicDataDbContext publicDataDbContext;

    public HelloWorldController(PublicDataDbContext publicDataDbContext)
    {
        this.publicDataDbContext=publicDataDbContext;
    }

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
            Versions = new List<DataSetVersion>
            {
                new DataSetVersion
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
                    FilterChanges = new List<DataSetChangeFilter>
                    {
                        new DataSetChangeFilter
                        {
                            Changes = new List<DataSetChange<FilterChangeState>>
                            {
                                new DataSetChange<FilterChangeState>
                                {
                                    Type = DataSetChangeType.Add,
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
                                new DataSetChange<FilterChangeState>
                                {
                                    Type = DataSetChangeType.Add,
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
                    IndicatorChanges = new List<DataSetChangeIndicator>
                    {
                        new DataSetChangeIndicator
                        {
                            Changes = new List<DataSetChange<IndicatorChangeState>>
                            {
                                new DataSetChange<IndicatorChangeState>
                                {
                                    Type = DataSetChangeType.Update,
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

        return null;
    }
}
