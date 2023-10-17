#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class HtmlBlockGeneratorExtensions
{
    public static Generator<HtmlBlock> DefaultHtmlBlock(this DataFixture fixture)
        => fixture.Generator<HtmlBlock>().WithDefaults();

    public static Generator<HtmlBlock> WithDefaults(this Generator<HtmlBlock> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<HtmlBlock> SetDefaults(this InstanceSetters<HtmlBlock> setters)
        => setters
            .SetDefault(block => block.Id)
            .SetDefault(block => block.Created)
            .SetDefault(block => block.Updated)
            .SetDefault(block => block.Body)
            .SetDefault(block => block.Order, offset: 1)
            .SetDefault(block => block.Order, offset: 1)
            .Set(d => d.Comments, (_, htmlBlock) => Enumerable
                .Range(1, 2)
                .Select(num =>
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        Content = $"{htmlBlock.Body} comment {num}"
                    })
                .ToList());
}
