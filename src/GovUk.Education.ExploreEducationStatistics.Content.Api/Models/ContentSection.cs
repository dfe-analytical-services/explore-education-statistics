using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Converters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Models
{
    public class ContentSection
    {
        public int Order { get; set; }
        
        public string Heading { get; set; }
        
        public string Caption { get; set; }
        
        public List<IContentBlock> Content { get; set; }
    }

    [JsonConverter(typeof(ContentBlockConverter))]
    public interface IContentBlock
    {
         string Type { get; }
    }

    public class MarkDownBlock : IContentBlock
    {
        public string Type => "MarkDownBlock";

        public string Body { get; set; }
    }
    
    public class InsetTextBlock : IContentBlock
    {
        public string Type => "InsetTextBlock";

        public string Heading { get; set; }
        
        public string Body { get; set; }
    }
}
