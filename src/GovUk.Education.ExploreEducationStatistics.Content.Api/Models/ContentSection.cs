using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
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
    [KnownType(typeof(MarkDownBlock))]
    [KnownType(typeof(InsetTextBlock))]
    [KnownType(typeof(DataBlock))]
    public abstract class IContentBlock {
        
        public abstract string Type { get;  }
    }

    public class MarkDownBlock : IContentBlock
    {
        public override string Type => "MarkDownBlock";

        public string Body { get; set; }
    }
    
    public class InsetTextBlock : IContentBlock
    {
        public override string Type => "InsetTextBlock";

        public string Heading { get; set; }
        
        public string Body { get; set; }
    }

    public class DataBlock : IContentBlock 
    {
        public override string Type => "DataBlock";

        public string Heading { get; set; }
        
        public DataBlockRequest DataBlockRequest { get; set; }
    
        public List<IContentBlockChart> Charts { get; set; }

        public Summary Summary { get; set; }
        
        public List<Table> Tables { get; set; }

    }

    public class Summary
    {
        public List<string> dataKeys { get; set; }

        public MarkDownBlock description { get; set; }
        
    }

    public class Table 
    {
        public List<string> indicators { get; set; }    
    }
}
