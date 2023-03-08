#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Config
{
    public class SensitiveDataTelemetryProcessorTests
    {
        public static IEnumerable<object[]> GetSensitiveQueryParamVariations()
        {
            return new List<object[]>
            {
                new object[] { "email" },
                new object[] { "email[0]" },
                new object[] { "email[]" },
                new object[] { "Email" },
                new object[] { "EMAIL" },
                new object[] { "eMail" },
                new object[] { "email1" },
                new object[] { "email10address" },
                new object[] { "emailA" },
                new object[] { "address_email" },
                new object[] { "email_address" },
                new object[] { "email-address" },
                new object[] { "email:address" },
                new object[] { "email+address" },
                new object[] { "email.address" },
                new object[] { "EmailAddress" },
                new object[] { "emailAddress" },
                new object[] { "emailAddress1" },
                new object[] { "emailAddress10" },
                new object[] { "email_address[0]" },
                new object[] { "email_address[]" },
                new object[] { "email++address" },
                new object[] { "email[address]" },
                new object[] { "address[email]" },
                // These have escaped reserved characters
                // such as ' ', ':', '+', '|'.
                new object[] { "email%20address" },
                new object[] { "email%3Aaddress" },
                new object[] { "email%2Baddress" },
                new object[] { "email%7Caddress" },
            };
        }

        [Fact]
        public void Process_Request_NoQuery()
        {
            var telemetry = new RequestTelemetry
            {
                Url = new Uri("https://test.com/"),
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("https://test.com/", telemetry.Url.AbsoluteUri);
        }

        [Fact]
        public void Process_Request_NullUrl()
        {
            var telemetry = new RequestTelemetry();

            BuildProcessor().Process(telemetry);

            Assert.Null(telemetry.Url);
        }

        [Fact]
        public void Process_Request_SingleQueryParam()
        {
            var telemetry = new RequestTelemetry
            {
                Url = new Uri("https://test.com/?code=my-code")
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("https://test.com/?code=__redacted__", telemetry.Url.AbsoluteUri);
        }

        [Fact]
        public void Process_Request_SingleQueryParam_NotFiltered()
        {
            var telemetry = new RequestTelemetry
            {
                Url = new Uri("https://test.com/?key=value")
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("https://test.com/?key=value", telemetry.Url.AbsoluteUri);
        }

        [Fact]
        public void Process_Request_SingleQueryParam_NotFilteredLookalike()
        {
            var telemetry = new RequestTelemetry
            {
                Url = new Uri("https://test.com/?encodeType=something")
            };

            BuildProcessor().Process(telemetry);

            // Contains the word 'code', but we shouldn't filter it
            Assert.Equal("https://test.com/?encodeType=something", telemetry.Url.AbsoluteUri);
        }

        [Theory]
        [MemberData(nameof(GetSensitiveQueryParamVariations))]
        public void Process_Request_SingleQueryParam_Variations(string key)
        {
            var telemetry = new RequestTelemetry
            {
                Url = new Uri($"https://test.com/?{key}=some-value")
            };

            var processor = BuildProcessor();

            processor.Process(telemetry);
            Assert.Equal($"https://test.com/?{key}=__redacted__", telemetry.Url.AbsoluteUri);
        }

        [Fact]
        public void Process_Request_MultipleQueryParams()
        {
            var telemetry = new RequestTelemetry
            {
                Url = new Uri("https://test.com/?password=my-password&code=my-code&token=my-token&email=my-email")
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal(
                "https://test.com/?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__",
                telemetry.Url.AbsoluteUri
            );
        }

        [Fact]
        public void Process_Request_MultipleQueryParams_WithNonSensitive()
        {
            var telemetry = new RequestTelemetry
            {
                Url = new Uri("https://test.com/?test1=value1&code=my-code&test2=value2&email=my-email")
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal(
                "https://test.com/?test1=value1&code=__redacted__&test2=value2&email=__redacted__",
                telemetry.Url.AbsoluteUri
            );
        }

        [Fact]
        public void Process_Request_CallsNext()
        {
            var telemetry = new RequestTelemetry
            {
                Url = new Uri("https://test.com/")
            };

            var nextProcessor = new Mock<ITelemetryProcessor>();

            nextProcessor.Setup(s => s.Process(telemetry));

            BuildProcessor(nextProcessor.Object).Process(telemetry);

            Assert.Equal("https://test.com/", telemetry.Url.AbsoluteUri);

            MockUtils.VerifyAllMocks(nextProcessor);
        }

        [Fact]
        public void Process_PageView_NoQuery()
        {
            var telemetry = new PageViewTelemetry
            {
                Url = new Uri("https://test.com/"),
                Properties =
                {
                    { "refUri", "https://test.com/" }
                }
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("https://test.com/", telemetry.Url.AbsoluteUri);
            Assert.Equal("https://test.com/", telemetry.Properties["refUri"]);
        }

        [Fact]
        public void Process_PageView_NullUris()
        {
            var telemetry = new PageViewTelemetry();

            BuildProcessor().Process(telemetry);

            Assert.Null(telemetry.Url);
            Assert.Empty(telemetry.Properties);
        }

        [Fact]
        public void Process_PageView_InvalidRefUri()
        {
            var telemetry = new PageViewTelemetry
            {
                Url = new Uri("https://test.com/"),
                Properties =
                {
                    { "refUri", "Not a URI" }
                }
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("https://test.com/", telemetry.Url.AbsoluteUri);
            Assert.Equal("Not a URI", telemetry.Properties["refUri"]);
        }

        [Fact]
        public void Process_PageView_SingleQueryParam()
        {
            var telemetry = new PageViewTelemetry
            {
                Url = new Uri("https://test.com/?code=my-code"),
                Properties =
                {
                    { "refUri", "https://test.com/ref?code=my-code" }
                }
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("https://test.com/?code=__redacted__", telemetry.Url.AbsoluteUri);
            Assert.Equal("https://test.com/ref?code=__redacted__", telemetry.Properties["refUri"]);
        }

        [Fact]
        public void Process_PageView_SingleQueryParam_NotFiltered()
        {
            var telemetry = new PageViewTelemetry
            {
                Url = new Uri("https://test.com/?key=value"),
                Properties =
                {
                    { "refUri", "https://test.com/ref?key=value" }
                }
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("https://test.com/?key=value", telemetry.Url.AbsoluteUri);
            Assert.Equal("https://test.com/ref?key=value", telemetry.Properties["refUri"]);
        }

        [Fact]
        public void Process_PageView_SingleQueryParam_NotFilteredLookalike()
        {
            var telemetry = new PageViewTelemetry
            {
                Url = new Uri("https://test.com/?encodeType=something"),
                Properties =
                {
                    { "refUri", "https://test.com/ref?encodeType=something" }
                }
            };

            BuildProcessor().Process(telemetry);

            // Contains the word 'code', but we shouldn't filter it
            Assert.Equal("https://test.com/?encodeType=something", telemetry.Url.AbsoluteUri);
            Assert.Equal("https://test.com/ref?encodeType=something", telemetry.Properties["refUri"]);
        }

        [Theory]
        [MemberData(nameof(GetSensitiveQueryParamVariations))]
        public void Process_PageView_SingleQueryParam_Variations(string key)
        {
            var telemetry = new PageViewTelemetry
            {
                Url = new Uri($"https://test.com/?{key}=some-value"),
                Properties =
                {
                    { "refUri", $"https://test.com/ref?{key}=some-value" }
                }
            };

            var processor = BuildProcessor();

            processor.Process(telemetry);
            Assert.Equal($"https://test.com/?{key}=__redacted__", telemetry.Url.AbsoluteUri);
            Assert.Equal($"https://test.com/ref?{key}=__redacted__", telemetry.Properties["refUri"]);
        }

        [Fact]
        public void Process_PageView_MultipleQueryParams()
        {
            var telemetry = new PageViewTelemetry
            {
                Url = new Uri("https://test.com/?password=my-password&code=my-code&token=my-token&email=my-email"),
                Properties =
                {
                    { "refUri", "https://test.com/ref?password=my-password&code=my-code&token=my-token&email=my-email" }
                }
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal(
                "https://test.com/?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__",
                telemetry.Url.AbsoluteUri
            );
            Assert.Equal(
                "https://test.com/ref?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__",
                telemetry.Properties["refUri"]
            );
        }

        [Fact]
        public void Process_PageView_MultipleQueryParams_WithNonSensitive()
        {
            var telemetry = new PageViewTelemetry
            {
                Url = new Uri("https://test.com/?test1=value1&code=my-code&test2=value2&email=my-email"),
                Properties =
                {
                    { "refUri", "https://test.com/ref?test1=value1&code=my-code&test2=value2&email=my-email" }
                }
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal(
                "https://test.com/?test1=value1&code=__redacted__&test2=value2&email=__redacted__",
                telemetry.Url.AbsoluteUri
            );
            Assert.Equal(
                "https://test.com/ref?test1=value1&code=__redacted__&test2=value2&email=__redacted__",
                telemetry.Properties["refUri"]
            );
        }

        [Fact]
        public void Process_PageView_CallsNext()
        {
            var telemetry = new PageViewTelemetry
            {
                Url = new Uri("https://test.com/"),
                Properties =
                {
                    { "refUri", "https://test.com/ref" }
                }
            };

            var nextProcessor = new Mock<ITelemetryProcessor>();

            nextProcessor.Setup(s => s.Process(telemetry));

            BuildProcessor(nextProcessor.Object).Process(telemetry);

            Assert.Equal("https://test.com/", telemetry.Url.AbsoluteUri);
            Assert.Equal("https://test.com/ref", telemetry.Properties["refUri"]);

            MockUtils.VerifyAllMocks(nextProcessor);
        }

        [Fact]
        public void Process_Dependency_NoQuery()
        {
            var telemetry = new DependencyTelemetry
            {
                Name = "GET /test",
                Data = "GET /test",
                Type = "Http"
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("GET /test", telemetry.Name);
            Assert.Equal("GET /test", telemetry.Data);
        }

        [Fact]
        public void Process_Dependency_NullUris()
        {
            var telemetry = new DependencyTelemetry
            {
                Type = "Http"
            };

            BuildProcessor().Process(telemetry);

            Assert.Empty(telemetry.Name);
            Assert.Empty(telemetry.Data);
        }

        [Fact]
        public void Process_Dependency_NoQuery_NotHttp()
        {
            var telemetry = new DependencyTelemetry
            {
                Name = "db | statistics",
                Data = "Test data",
                Type = "SQL"
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("db | statistics", telemetry.Name);
            Assert.Equal("Test data", telemetry.Data);
        }

        [Fact]
        public void Process_Dependency_SingleQueryParam()
        {
            var telemetry = new DependencyTelemetry
            {
                Name = "GET /test?code=my-code",
                Data = "https://test.com/test?code=my-code",
                Type = "Http"
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("GET /test?code=__redacted__", telemetry.Name);
            Assert.Equal("https://test.com/test?code=__redacted__", telemetry.Data);
        }

        [Fact]
        public void Process_Dependency_SingleQueryParam_InvalidUrl()
        {
            var telemetry = new DependencyTelemetry
            {
                Name = "GET /test?code=my-code",
                Data = "Invalid URL",
                Type = "Http"
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("GET /test?code=__redacted__", telemetry.Name);
            Assert.Equal("Invalid URL", telemetry.Data);
        }

        [Fact]
        public void Process_Dependency_SingleQueryParam_NotHttp()
        {
            var telemetry = new DependencyTelemetry
            {
                Name = "GET /test?code=my-code",
                Data = "https://test.com/test?code=my-code",
                // We don't filter this type as we consider it
                // outside of our application code's scope.
                Type = "Azure Blob"
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("GET /test?code=my-code", telemetry.Name);
            Assert.Equal("https://test.com/test?code=my-code", telemetry.Data);
        }

        [Fact]
        public void Process_Dependency_SingleQueryParam_NotFiltered()
        {
            var telemetry = new DependencyTelemetry
            {
                Name = "GET /test?key=value",
                Data = "https://test.com/test?key=value",
                Type = "Http"
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal("GET /test?key=value", telemetry.Name);
            Assert.Equal("https://test.com/test?key=value", telemetry.Data);
        }

        [Fact]
        public void Process_Dependency_SingleQueryParam_NotFilteredLookalike()
        {
            var telemetry = new DependencyTelemetry
            {
                Name = "GET /test?encodeType=something",
                Data = "https://test.com/test?encodeType=something",
                Type = "Http"
            };

            BuildProcessor().Process(telemetry);

            // Contains the word 'code', but we shouldn't filter it
            Assert.Equal("GET /test?encodeType=something", telemetry.Name);
            Assert.Equal("https://test.com/test?encodeType=something", telemetry.Data);
        }

        [Theory]
        [MemberData(nameof(GetSensitiveQueryParamVariations))]
        public void Process_Dependency_SingleQueryParam_Variations(string key)
        {
            var telemetry = new DependencyTelemetry
            {
                Name = $"GET /test?{key}=some-value",
                Data = $"https://test.com/test?{key}=some-value",
                Type = "Http"
            };

            var processor = BuildProcessor();

            processor.Process(telemetry);
            Assert.Equal($"GET /test?{key}=__redacted__", telemetry.Name);
            Assert.Equal($"https://test.com/test?{key}=__redacted__", telemetry.Data);
        }

        [Fact]
        public void Process_Dependency_MultipleQueryParams()
        {
            var telemetry = new DependencyTelemetry
            {
                Name = "GET /test?password=my-password&code=my-code&token=my-token&email=my-email",
                Data = "https://test.com/test?password=my-password&code=my-code&token=my-token&email=my-email",
                Type = "Http"
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal(
                "GET /test?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__",
                telemetry.Name
            );
            Assert.Equal(
                "https://test.com/test?password=__redacted__&code=__redacted__&token=__redacted__&email=__redacted__",
                telemetry.Data
            );
        }

        [Fact]
        public void Process_Dependency_MultipleQueryParams_WithNonSensitive()
        {
            var telemetry = new DependencyTelemetry
            {
                Name = "GET /test?test1=value1&code=my-code&test2=value2&email=my-email",
                Data = "https://test.com/test?test1=value1&code=my-code&test2=value2&email=my-email",
                Type = "Http"
            };

            BuildProcessor().Process(telemetry);

            Assert.Equal(
                "GET /test?test1=value1&code=__redacted__&test2=value2&email=__redacted__",
                telemetry.Name
            );
            Assert.Equal(
                "https://test.com/test?test1=value1&code=__redacted__&test2=value2&email=__redacted__",
                telemetry.Data
            );
        }

        [Fact]
        public void Process_Dependency_CallsNext()
        {
            var telemetry = new DependencyTelemetry
            {
                Name = "GET /test",
                Data = "https://test.com/test",
                Type = "Http"
            };

            var nextProcessor = new Mock<ITelemetryProcessor>();

            nextProcessor.Setup(s => s.Process(telemetry));

            BuildProcessor(nextProcessor.Object).Process(telemetry);

            Assert.Equal("GET /test", telemetry.Name);
            Assert.Equal("https://test.com/test", telemetry.Data);

            MockUtils.VerifyAllMocks(nextProcessor);
        }

        private SensitiveDataTelemetryProcessor BuildProcessor(ITelemetryProcessor? nextProcessor = null)
        {
            return new SensitiveDataTelemetryProcessor(
                nextProcessor ?? Mock.Of<ITelemetryProcessor>()
            );
        }
    }
}
