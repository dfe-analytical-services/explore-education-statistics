#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;
using static Microsoft.Net.Http.Headers.HeaderNames;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class HttpRequestExtensionsTests
{
    public class AcceptsCsvTests
    {
        [Fact]
        public void NoHeader()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary());

            var request = mock.Object;

            // True as no header is equivalent to accepting any type
            Assert.True(request.AcceptsCsv());
        }

        [Fact]
        public void NoHeader_Exact()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary());

            var request = mock.Object;

            Assert.False(request.AcceptsCsv(exact: true));
        }

        [Fact]
        public void Single_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/csv" } });

            var request = mock.Object;

            Assert.True(request.AcceptsCsv());
        }

        [Fact]
        public void Single_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/html" } });

            var request = mock.Object;

            Assert.False(request.AcceptsCsv());
        }

        [Fact]
        public void Multiple_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary { { Accept, "text/html, application/json, text/csv" } }
                );

            var request = mock.Object;

            Assert.True(request.AcceptsCsv());
        }

        [Fact]
        public void Multiple_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary { { Accept, "text/html, application/json" } });

            var request = mock.Object;

            Assert.False(request.AcceptsCsv());
        }

        [Fact]
        public void AnyTextSubType()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/*" } });

            var request = mock.Object;

            Assert.True(request.AcceptsCsv());
        }

        [Fact]
        public void AnyType()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "*/*" } });

            var request = mock.Object;

            Assert.True(request.AcceptsCsv());
        }
    }

    public class AcceptsTests
    {
        [Fact]
        public void NoHeader()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary());

            var request = mock.Object;

            // True as no header is equivalent to accepting any type
            Assert.True(request.Accepts("text/html"));
        }

        [Fact]
        public void NoHeader_Exact()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary());

            var request = mock.Object;

            Assert.False(request.Accepts(exact: true, "text/html"));
        }

        [Fact]
        public void InvalidHeader()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "not valid" } });

            var request = mock.Object;

            Assert.False(request.Accepts("text/html"));
        }

        [Fact]
        public void InvalidParam_Throws()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "not valid" } });

            var request = mock.Object;

            Assert.Throws<FormatException>(() => request.Accepts("not valid"));
        }

        [Fact]
        public void Single_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/html" } });

            var request = mock.Object;

            Assert.True(request.Accepts("text/html"));
            Assert.True(request.Accepts(MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Single_Exact_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/html" } });

            var request = mock.Object;

            Assert.True(request.Accepts(exact: true, "text/html"));
            Assert.True(request.Accepts(exact: true, MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Single_Header_AnySubType_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/*" } });

            var request = mock.Object;

            Assert.True(request.Accepts("text/html"));
            Assert.True(request.Accepts(MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Single_Header_AnyType_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "*/*" } });

            var request = mock.Object;

            Assert.True(request.Accepts("text/html"));
            Assert.True(request.Accepts(MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Single_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/html" } });

            var request = mock.Object;

            Assert.False(request.Accepts("application/json"));
            Assert.False(request.Accepts(MediaTypeHeaderValue.Parse("application/json")));
        }

        [Fact]
        public void Single_Param_AnySubType_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/html" } });

            var request = mock.Object;

            Assert.False(request.Accepts("text/*"));
            Assert.False(request.Accepts(MediaTypeHeaderValue.Parse("text/*")));
        }

        [Fact]
        public void Single_Param_AnyType_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/html" } });

            var request = mock.Object;

            Assert.False(request.Accepts("*/*"));
            Assert.False(request.Accepts(MediaTypeHeaderValue.Parse("*/*")));
        }

        [Fact]
        public void Single_Header_AnySubType_Exact_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/*" } });

            var request = mock.Object;

            Assert.False(request.Accepts(exact: true, "text/html"));
            Assert.False(request.Accepts(exact: true, MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Single_Header_AnyType_Exact_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "*/*" } });

            var request = mock.Object;

            Assert.False(request.Accepts(exact: true, "text/html"));
            Assert.False(request.Accepts(exact: true, MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Multiple_Header_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary { { Accept, "text/html, application/json" } });

            var request = mock.Object;

            Assert.True(request.Accepts("application/json"));
            Assert.True(request.Accepts(MediaTypeHeaderValue.Parse("application/json")));
        }

        [Fact]
        public void Multiple_Header_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary { { Accept, "text/html, application/json" } });

            var request = mock.Object;

            Assert.False(request.Accepts("application/pdf"));
            Assert.False(request.Accepts(MediaTypeHeaderValue.Parse("application/pdf")));
        }

        [Fact]
        public void Multiple_Header_AnyType_Exact_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "*/*" } });

            var request = mock.Object;

            Assert.False(request.Accepts(exact: true, "application/json", "text/csv"));
        }

        [Fact]
        public void Multiple_Params_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/html" } });

            var request = mock.Object;

            Assert.True(request.Accepts("application/json", "text/html"));
        }

        [Fact]
        public void Multiple_Params_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers).Returns(new HeaderDictionary { { Accept, "text/html" } });

            var request = mock.Object;

            Assert.False(request.Accepts("application/json", "text/csv"));
        }

        [Fact]
        public void Multiple_ParamsAndHeader_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary { { Accept, "text/html, text/csv" } });

            var request = mock.Object;

            Assert.True(request.Accepts("application/json", "text/csv"));
        }

        [Fact]
        public void Multiple_ParamsAndHeader_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary { { Accept, "text/html, text/pdf" } });

            var request = mock.Object;

            Assert.False(request.Accepts("application/json", "text/csv"));
        }

        [Fact]
        public void Multiple_ParamsAndHeader_Exact_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary { { Accept, "text/csv, application/*" } });

            var request = mock.Object;

            Assert.True(request.Accepts(exact: true, "application/json", "text/csv"));
        }

        [Fact]
        public void Multiple_ParamsAndHeader_Exact_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary { { Accept, "application/*, text/*" } });

            var request = mock.Object;

            Assert.False(request.Accepts(exact: true, "application/json", "text/csv"));
        }
    }

    public class GetRequestParamTests
    {
        [Fact]
        public void Success()
        {
            var mock = new Mock<HttpRequest>();

            mock.Setup(m => m.Query)
                .Returns(
                    new QueryCollection(
                        new Dictionary<string, StringValues> { { "testParam", "testValue" } }
                    )
                );

            var request = mock.Object;

            Assert.Equal("testValue", request.GetRequestParam("testParam"));
        }

        [Fact]
        public void Success_NoMatchingRequestParam()
        {
            var mock = new Mock<HttpRequest>();

            mock.Setup(m => m.Query)
                .Returns(
                    new QueryCollection(
                        new Dictionary<string, StringValues> { { "testParam", "testValue" } }
                    )
                );

            var request = mock.Object;

            Assert.Null(request.GetRequestParam("anotherParam"));
        }

        [Fact]
        public void Success_NoMatchingRequestParam_DefaultValue()
        {
            var mock = new Mock<HttpRequest>();

            mock.Setup(m => m.Query)
                .Returns(
                    new QueryCollection(
                        new Dictionary<string, StringValues> { { "testParam", "testValue" } }
                    )
                );

            var request = mock.Object;

            Assert.Equal(
                "defaultValue",
                request.GetRequestParam("anotherParam", defaultValue: "defaultValue")
            );
        }
    }

    public class GetRequestParamBoolTests
    {
        [Theory]
        [InlineData("true", false)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("false", true)]
        public void Success(string? paramValue, bool defaultValue)
        {
            var mock = new Mock<HttpRequest>();

            mock.Setup(m => m.Query)
                .Returns(
                    new QueryCollection(
                        new Dictionary<string, StringValues> { { "testParam", paramValue } }
                    )
                );

            var request = mock.Object;

            var value = request.GetRequestParamBool("testParam", defaultValue: defaultValue);
            Assert.Equal(bool.Parse(paramValue), value);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Success_NoMatchingRequestParam(bool defaultValue)
        {
            var mock = new Mock<HttpRequest>();

            mock.Setup(m => m.Query)
                .Returns(
                    new QueryCollection(
                        new Dictionary<string, StringValues> { { "testParam", "true" } }
                    )
                );

            var request = mock.Object;

            var value = request.GetRequestParamBool("anotherParam", defaultValue);
            Assert.Equal(defaultValue, value);
        }

        [Theory]
        [InlineData("", true)]
        [InlineData("", false)]
        [InlineData(null, true)]
        [InlineData(null, false)]
        public void Success_EmptyValues_UseDefault(string? paramValue, bool defaultValue)
        {
            var mock = new Mock<HttpRequest>();

            mock.Setup(m => m.Query)
                .Returns(
                    new QueryCollection(
                        new Dictionary<string, StringValues> { { "testParam", paramValue } }
                    )
                );

            var request = mock.Object;

            var value = request.GetRequestParamBool("testParam", defaultValue);
            Assert.Equal(defaultValue, value);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("1")]
        public void Failure_NonBooleanStrings(string paramValue)
        {
            var mock = new Mock<HttpRequest>();

            mock.Setup(m => m.Query)
                .Returns(
                    new QueryCollection(
                        new Dictionary<string, StringValues> { { "testParam", paramValue } }
                    )
                );

            var request = mock.Object;

            Assert.Throws<FormatException>(() => request.GetRequestParamBool("testParam", false));
        }
    }
}
