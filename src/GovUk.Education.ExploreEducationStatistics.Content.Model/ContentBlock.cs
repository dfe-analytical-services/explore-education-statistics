﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    [JsonConverter(typeof(JsonKnownTypesConverter<ContentBlock>))]
    [JsonDiscriminator(Name = "Type")]
    [KnownType(typeof(MarkDownBlock))]
    [KnownType(typeof(DataBlock))]
    [KnownType(typeof(HtmlBlock))]
    public abstract class ContentBlock
    {
        public Guid Id { get; set; }

        [JsonIgnore] public ContentSection? ContentSection { get; set; }

        [JsonIgnore] public Guid? ContentSectionId { get; set; }

        public int Order { get; set; }

        public DateTime? Created { get; set; }

        public List<Comment> Comments { get; set; } = new();

        public ContentBlock Clone(Release.CloneContext context, ContentSection? newContentSection = null)
        {
            var copy = MemberwiseClone() as ContentBlock;
            copy.Id = Guid.NewGuid();

            copy.ContentSection = newContentSection;
            copy.ContentSectionId = newContentSection?.Id;

            // start a new amendment with no comments
            copy.Comments = new List<Comment>();

            context.ContentBlocks.Add(this, copy);

            return copy;
        }

        public ContentBlock Clone(DateTime createdDate)
        {
            var copy = MemberwiseClone() as ContentBlock;
            copy.Id = Guid.NewGuid();
            copy.Created = createdDate;
            copy.ContentSection = null;
            copy.ContentSectionId = null;

            // start a new amendment with no comments
            copy.Comments = new List<Comment>();
            return copy;
        }
    }

    public class MarkDownBlock : ContentBlock
    {
        public MarkDownBlock()
        {
        }

        public string Body { get; set; }
    }

    public class HtmlBlock : ContentBlock
    {
        public HtmlBlock()
        {
        }

        public string Body { get; set; }
    }

    public class DataBlock : ContentBlock
    {
        public DataBlock()
        {
        }

        public string Heading { get; set; }

        public string Name { get; set; }

        public string? HighlightName { get; set; }

        public string? HighlightDescription { get; set; }

        public string Source { get; set; }

        public ObservationQueryContext Query { get; set; }

        [JsonIgnore]
        public List<IChart> Charts
        {
            get
            {
                return ChartsInternal.Select(chart =>
                {
                    if (chart.Title.IsNullOrEmpty())
                    {
                        chart.Title = Heading;
                    }
                    return chart;
                }).ToList();

            }
            set => ChartsInternal = value;
        }

        // NOTE: We serialize ChartsInternal into JSON rather than Charts so that a chart title is set to null in the
        // database JSON. If we serialized Charts, then the serialization would run through Chart's getter, and so set
        // a chart title that is identical to the table heading. So to keep the database-stored chart JSON pure, we set
        // this JsonProperty, preventing the need to migrate existing charts, and Chart's getter will provide the table
        // heading in request responses when necessary.
        [JsonProperty("Charts")]
        [NotMapped]
        private List<IChart> ChartsInternal { get; set; } = new();

        public DataBlockSummary Summary { get; set; }

        public TableBuilderConfiguration Table { get; set; }

        // TODO EES-3212 Remove these temporary fields that were added for the migration from Location codes to id's.
        [NotMapped]
        public List<IChart> ChartsMigrated { get; set; } = new();
        [NotMapped]
        public ObservationQueryContext QueryMigrated { get; set; }
        [NotMapped]
        public TableBuilderConfiguration TableMigrated { get; set; }
        public bool TableHeaderCountChanged { get; set; }
        public bool LocationsMigrated { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ContentBlockClassType : Attribute
    {
        public Type Type { get; set; }
    }

    public enum ContentBlockType
    {
        [ContentBlockClassType(Type = typeof(HtmlBlock))]
        HtmlBlock
    }

    public static class ContentBlockUtil
    {
        public static Type GetContentBlockClassTypeFromEnumValue(ContentBlockType enumValue)
        {
            return enumValue.GetEnumAttribute<ContentBlockClassType>().Type;
        }
    }
}
