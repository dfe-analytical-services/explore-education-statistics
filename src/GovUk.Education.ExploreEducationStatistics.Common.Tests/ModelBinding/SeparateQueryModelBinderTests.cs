#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.ModelBinding;

public class SeparateQueryModelBinderTests : IntegrationTest<TestStartup>
{
    public SeparateQueryModelBinderTests(TestApplicationFactory<TestStartup> testApp)
        : base(testApp) { }

    [Fact]
    public async Task BindsToIntList()
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync($"{nameof(TestController.IntList)}?items=1,2,3");

        response.AssertOk(new List<int> { 1, 2, 3 });
    }

    [Fact]
    public async Task BindsToIntListUsingPost()
    {
        var client = BuildApp().CreateClient();

        var response = await client.PostAsync($"{nameof(TestController.IntListPost)}?items=1,2,3", null);

        response.AssertOk(new List<int> { 1, 2, 3 });
    }

    [Fact]
    public async Task BindsToIntListClass()
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync($"{nameof(TestController.IntListClass)}?items=1,2,3");

        response.AssertOk(
            new IntListRequest
            {
                Items = new List<int> { 1, 2, 3 },
            }
        );
    }

    [Fact]
    public async Task BindsToIntArray()
    {
        var client = BuildApp().CreateClient();

        var response = await client.GetAsync($"{nameof(TestController.IntArray)}?items=1,2,3");

        response.AssertOk(new List<int> { 1, 2, 3 });
    }

    [Fact]
    public async Task BindsToGuidList()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var guid3 = Guid.NewGuid();

        var client = BuildApp().CreateClient();

        var response = await client.GetAsync($"{nameof(TestController.GuidList)}?items={guid1},{guid2},{guid3}");

        response.AssertOk(new List<Guid> { guid1, guid2, guid3 });
    }

    [Fact]
    public async Task DoesNotBindToStringListByDefault()
    {
        const string string1 = "one";
        const string string2 = "two";
        const string string3 = "three";

        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(
            $"{nameof(TestController.StringList)}?items={string1},{string2},{string3}"
        );

        // Only a single item list is returned, meaning the model was not bound as expected.
        response.AssertOk(new List<string> { $"{string1},{string2},{string3}" });
    }

    [Fact]
    public async Task DoesNotBindToStringListClassByDefault()
    {
        const string string1 = "one";
        const string string2 = "two";
        const string string3 = "three";

        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(
            $"{nameof(TestController.StringListClass)}?items={string1},{string2},{string3}"
        );

        // Only a single item list is returned, meaning the model was not bound as expected.
        response.AssertOk(new StringListRequest { Items = new List<string> { $"{string1},{string2},{string3}" } });
    }

    [Fact]
    public async Task BindsToStringListWithQuerySeparatorAttribute()
    {
        const string string1 = "one";
        const string string2 = "two";
        const string string3 = "three";

        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(
            $"{nameof(TestController.StringListQuerySeparator)}?items={string1},{string2},{string3}"
        );

        response.AssertOk(new List<string> { string1, string2, string3 });
    }

    [Fact]
    public async Task BindsToStringListClassWithQuerySeparatorAttribute()
    {
        const string string1 = "one";
        const string string2 = "two";
        const string string3 = "three";

        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(
            $"{nameof(TestController.StringListClassQuerySeparator)}?items={string1},{string2},{string3}"
        );

        response.AssertOk(
            new StringListWithQuerySeparatorRequest
            {
                Items = new List<string> { string1, string2, string3 },
            }
        );
    }

    [Fact]
    public async Task BindsToStringListWithCustomisedQuerySeparatorAttribute()
    {
        const string string1 = "one";
        const string string2 = "two";
        const string string3 = "three";

        var client = BuildApp().CreateClient();

        var response = await client.GetAsync(
            $"{nameof(TestController.StringListQuerySeparatorCustom)}?items={string1}:{string2}:{string3}"
        );

        response.AssertOk(new List<string> { string1, string2, string3 });
    }

    private record IntListRequest
    {
        [FromQuery]
        public IList<int> Items { get; set; } = default!;
    }

    private record StringListRequest
    {
        [FromQuery]
        public IList<string> Items { get; set; } = default!;
    }

    private record StringListWithQuerySeparatorRequest
    {
        [FromQuery, QuerySeparator]
        public IList<string> Items { get; set; } = default!;
    }

    [ApiController]
    private class TestController : ControllerBase
    {
        [HttpGet(nameof(IntList))]
        public IList<int> IntList([FromQuery] IList<int> items) => items;

        [HttpPost(nameof(IntListPost))]
        public IList<int> IntListPost([FromQuery] IList<int> items) => items;

        [HttpGet(nameof(IntListClass))]
        public IntListRequest IntListClass([FromQuery] IntListRequest request) => request;

        [HttpGet(nameof(IntArray))]
        public IList<int> IntArray([FromQuery] int[] items) => items;

        [HttpGet(nameof(GuidList))]
        public IList<Guid> GuidList([FromQuery] IList<Guid> items) => items;

        [HttpGet(nameof(StringList))]
        public IList<string> StringList([FromQuery] IList<string> items) => items;

        [HttpGet(nameof(StringListClass))]
        public StringListRequest StringListClass([FromQuery] StringListRequest request) => request;

        [HttpGet(nameof(StringListQuerySeparator))]
        public IList<string> StringListQuerySeparator([FromQuery, QuerySeparator] IList<string> items) => items;

        [HttpGet(nameof(StringListClassQuerySeparator))]
        public StringListWithQuerySeparatorRequest StringListClassQuerySeparator(
            [FromQuery] StringListWithQuerySeparatorRequest request
        ) => request;

        [HttpGet(nameof(StringListQuerySeparatorCustom))]
        public IList<string> StringListQuerySeparatorCustom([FromQuery, QuerySeparator(":")] IList<string> items) =>
            items;
    }

    private WebApplicationFactory<TestStartup> BuildApp()
    {
        return TestApp.WithWebHostBuilder(builder => builder.WithAdditionalControllers(typeof(TestController)));
    }
}
