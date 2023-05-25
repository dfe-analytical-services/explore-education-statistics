#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;
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

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary());

            var request = mock.Object;

            // True as no header is equivalent to accepting any type
            Assert.True(request.AcceptsCsv());
        }

        [Fact]
        public void NoHeader_Exact()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary());

            var request = mock.Object;

            Assert.False(request.AcceptsCsv(exact: true));
        }

        [Fact]
        public void Single_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/csv" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.AcceptsCsv());
        }

        [Fact]
        public void Single_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.AcceptsCsv());
        }

        [Fact]
        public void Multiple_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html, application/json, text/csv" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.AcceptsCsv());
        }


        [Fact]
        public void Multiple_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html, application/json" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.AcceptsCsv());
        }

        [Fact]
        public void AnyTextSubType()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/*" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.AcceptsCsv());
        }

        [Fact]
        public void AnyType()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "*/*" }
                    }
                );

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

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary());

            var request = mock.Object;

            // True as no header is equivalent to accepting any type
            Assert.True(request.Accepts("text/html"));
        }

        [Fact]
        public void NoHeader_Exact()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(new HeaderDictionary());

            var request = mock.Object;

            Assert.False(request.Accepts(exact: true, "text/html"));
        }

        [Fact]
        public void InvalidHeader()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "not valid" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts("text/html"));
        }

        [Fact]
        public void InvalidParam_Throws()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "not valid" }
                    }
                );

            var request = mock.Object;

            Assert.Throws<FormatException>(() => request.Accepts("not valid"));
        }
        
        [Fact]
        public void Single_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.Accepts("text/html"));
            Assert.True(request.Accepts(MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Single_Exact_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.Accepts(exact: true, "text/html"));
            Assert.True(request.Accepts(exact: true, MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Single_Header_AnySubType_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/*" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.Accepts("text/html"));
            Assert.True(request.Accepts(MediaTypeHeaderValue.Parse("text/html")));
        }


        [Fact]
        public void Single_Header_AnyType_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "*/*" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.Accepts("text/html"));
            Assert.True(request.Accepts(MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Single_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts("application/json"));
            Assert.False(request.Accepts(MediaTypeHeaderValue.Parse("application/json")));
        }

        [Fact]
        public void Single_Param_AnySubType_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts("text/*"));
            Assert.False(request.Accepts(MediaTypeHeaderValue.Parse("text/*")));
        }


        [Fact]
        public void Single_Param_AnyType_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts("*/*"));
            Assert.False(request.Accepts(MediaTypeHeaderValue.Parse("*/*")));
        }

        [Fact]
        public void Single_Header_AnySubType_Exact_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/*" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts(exact: true, "text/html"));
            Assert.False(request.Accepts(exact: true, MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Single_Header_AnyType_Exact_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "*/*" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts(exact: true, "text/html"));
            Assert.False(request.Accepts(exact: true, MediaTypeHeaderValue.Parse("text/html")));
        }

        [Fact]
        public void Multiple_Header_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html, application/json" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.Accepts("application/json"));
            Assert.True(request.Accepts(MediaTypeHeaderValue.Parse("application/json")));
        }

        [Fact]
        public void Multiple_Header_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html, application/json" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts("application/pdf"));
            Assert.False(request.Accepts(MediaTypeHeaderValue.Parse("application/pdf")));
        }

        [Fact]
        public void Multiple_Header_AnyType_Exact_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "*/*" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts(exact: true, "application/json", "text/csv"));
        }

        [Fact]
        public void Multiple_Params_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.Accepts("application/json", "text/html"));
        }

        [Fact]
        public void Multiple_Params_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts("application/json", "text/csv"));
        }

        [Fact]
        public void Multiple_ParamsAndHeader_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html, text/csv" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.Accepts("application/json", "text/csv"));
        }

        [Fact]
        public void Multiple_ParamsAndHeader_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/html, text/pdf" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts("application/json", "text/csv"));
        }

        [Fact]
        public void Multiple_ParamsAndHeader_Exact_True()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "text/csv, application/*" }
                    }
                );

            var request = mock.Object;

            Assert.True(request.Accepts(exact: true, "application/json", "text/csv"));
        }

        [Fact]
        public void Multiple_ParamsAndHeader_Exact_False()
        {
            var mock = new Mock<HttpRequest>();

            mock.SetupGet(m => m.Headers)
                .Returns(
                    new HeaderDictionary
                    {
                        { Accept, "application/*, text/*" }
                    }
                );

            var request = mock.Object;

            Assert.False(request.Accepts(exact: true, "application/json", "text/csv"));
        }
    }
}
