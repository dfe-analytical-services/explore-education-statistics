#nullable enable
using GeoJSON.Net.Feature;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class BoundaryData
{
    public int Id { get; set; }

    [Required]
    public string? Code { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public Feature? GeoJson { get; set; }

    [Required]
    public BoundaryLevel? BoundaryLevel { get; set; }
}
