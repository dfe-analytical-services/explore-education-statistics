using Asp.Versioning;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Controllers;

[ApiVersion(1.0)]
[ApiVersion(2.0)]
[ApiController]
[Route("api/v{version:apiVersion}/HelloWorld")]
public class HelloWorldController(PublicDataDbContext publicDataDbContext) : ControllerBase
{
    /// <summary>
    /// List v1
    /// </summary>
    /// <remarks>
    /// Test the API can list with `GET` in v1.
    /// </remarks>
    [HttpGet]
    [SwaggerResponse(200, "The list of things", typeof(string))]
    public async Task<ActionResult<string>> List()
    {
        return await Task.FromResult("Hello World");
    }

    /// <summary>
    /// List v2
    /// </summary>
    /// <remarks>
    /// Tests the API can list with `GET` in v2.
    /// </remarks>
    [MapToApiVersion(2.0)]
    [HttpGet]
    [SwaggerResponse(200, "The list of things", typeof(string))]
    public async Task<ActionResult<string>> ListV2()
    {
        return await Task.FromResult("Hello World v2");
    }

    /// <summary>
    /// Get by ID
    /// </summary>
    /// <remarks>
    /// Tests the API can get by ID with `GET` v1 and v2.
    /// </remarks>
    [HttpGet("{id}", Name = "GetById")]
    [SwaggerResponse(200, "The specific thing", typeof(string))]
    [SwaggerResponse(404, "Thing could not be found")]
    public async Task<ActionResult<string>> Get(
        [SwaggerParameter("The ID of the thing")] string id)
    {
        if (id.IsNullOrEmpty())
        {
            return NotFound();
        }

        return await Task.FromResult("Hello World v1 and v2");
    }

    /// <summary>
    /// Create
    /// </summary>
    /// <remarks>
    /// Tests the API can create with `POST` in v1 and v2.
    /// </remarks>
    [HttpPost(Name = "CreateCustomId")]
    [SwaggerResponse(201, "Thing was created")]
    public async Task<DataSet> Create()
    {
        var dataSetId = Guid.NewGuid();
        var dataSetVersionId = Guid.NewGuid();

        var dataSet = new DataSet
        {
            Id = dataSetId,
            Status = DataSetStatus.Published,
            Title = "My first dataset",
            Summary = "My first summary",
            PublicationId = Guid.NewGuid(),
            Versions = new List<DataSetVersion>
            {
                new()
                {
                    DataSetId = dataSetId,
                    Status = DataSetVersionStatus.Staged,
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
                        },
                        GeographicLevels = new List<GeographicLevel>
                        {
                            GeographicLevel.Country,
                            GeographicLevel.LocalAuthority,
                        },
                        TimePeriodRange = new TimePeriodRange
                        {
                            Start = new TimePeriodMeta
                            {
                                Code = TimeIdentifier.AcademicYear,
                                Year = 2018
                            },
                            End = new TimePeriodMeta
                            {
                                Code = TimeIdentifier.AcademicYear,
                                Year = 2023
                            }
                        }
                    },
                    Meta = new DataSetMeta
                    {
                        DataSetVersionId = dataSetVersionId,
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
                                        PublicId = "123",
                                        PrivateId = 123,
                                        Label = "Primary"
                                    },
                                    new()
                                    {
                                        PublicId = "345",
                                        PrivateId = 345,
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
                                Unit = IndicatorUnit.Pound
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
                                        PublicId = "1",
                                        PrivateId = 1,
                                        Label = "Barnsley",
                                        Code = "E00001"
                                    },
                                    new()
                                    {
                                        PublicId = "2",
                                        PrivateId = 2,
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
                            DataSetVersionId = dataSetVersionId,
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
                            DataSetVersionId = dataSetVersionId,
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
                                        Unit = IndicatorUnit.Percent
                                    },
                                    PreviousState = new IndicatorChangeState
                                    {
                                        Label = "aaa",
                                        Id = "d",
                                        DecimalPlaces = 2,
                                        Unit = IndicatorUnit.Pound
                                    }
                                }
                            }
                        }
                    },
                    LocationChanges = new List<ChangeSetLocations>()
                    {
                        new()
                        {
                            DataSetVersionId = dataSetVersionId,
                            Changes = new List<Change<LocationChangeState>>
                            {
                                new()
                                {
                                    Type = ChangeType.Add,
                                    CurrentState = new LocationChangeState
                                    {
                                        Id = "123",
                                        Label = "Barnsley",
                                        Level = GeographicLevel.LocalAuthority,
                                        Code = "E123"
                                    },
                                }
                            }
                        }
                    },
                    TimePeriodChanges = new List<ChangeSetTimePeriods>
                    {
                        new()
                        {
                            DataSetVersionId = dataSetVersionId,
                            Changes = new List<Change<TimePeriodChangeState>>
                            {
                                new()
                                {
                                    Type = ChangeType.Update,
                                    CurrentState = new TimePeriodChangeState
                                    {
                                        Code = TimeIdentifier.FinancialYear,
                                        Year = 2020,
                                    },
                                    PreviousState = new TimePeriodChangeState
                                    {
                                        Code = TimeIdentifier.FinancialYear,
                                        Year = 2023,
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
