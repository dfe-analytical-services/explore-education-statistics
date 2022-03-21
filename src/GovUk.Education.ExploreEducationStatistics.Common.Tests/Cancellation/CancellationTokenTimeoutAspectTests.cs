#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using Microsoft.Extensions.Configuration;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Cancellation.CancellationTokenTimeoutTestFixture;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Cancellation
{
    [Collection(CollectionName)]
    public class CancellationTokenTimeoutAspectTests : IClassFixture<CancellationTokenTimeoutTestFixture>
    {
        private static class TestMethods
        {
            [CancellationTokenTimeout(TimeoutMillis)]
            public static void NoCancellationTokenParameterAvailableForAddTimeout()
            {
            }
            
            [CancellationTokenTimeout(TimeoutMillis)]
            public static void AmbiguousCancellationTokenParametersAvailableForAddTimeout(
                CancellationToken token1, CancellationToken? token2 = default)
            {
            }
        
            [CancellationTokenTimeout(TimeoutMillis)]
            public static void AddTimeout(
                Action<CancellationToken?> assertions, 
                CancellationToken? token = null)
            {
                assertions.Invoke(token);
            }
            
            [CancellationTokenTimeout(TimeoutMillis)]
            public static async Task AddTimeoutAsync(
                Func<CancellationToken?, Task> assertions, 
                CancellationToken? token = null)
            {
                await assertions.Invoke(token);
            }
            
            [CancellationTokenTimeout(TimeoutMillis)]
            public static async Task AddTimeoutAsyncWithDifferingTokenPosition(
                CancellationToken? token,
                Func<CancellationToken?, Task> assertions)
            {
                await assertions.Invoke(token);
            }
            
            [CancellationTokenTimeout(ExistingConfigurationItemKey)]
            public static void AddTimeoutWithConfiguration(
                Action<CancellationToken?> assertions, 
                CancellationToken? token = null)
            {
                assertions.Invoke(token);
            }
            
            [CancellationTokenTimeout(MisconfiguredConfigurationItemKey)]
            public static void AddTimeoutWithMisconfiguredConfiguration(CancellationToken? token = null)
            {
            }
            
            [CancellationTokenTimeout(MissingConfigurationItemKey)]
            public static void AddTimeoutWithMissingConfiguration(CancellationToken? token = null)
            {
            }
        }

        [Fact]
        public void NoCancellationTokenParameterAvailableForAddTimeout()
        {
            Assert.Throws<ArgumentException>(TestMethods.NoCancellationTokenParameterAvailableForAddTimeout);
        }

        [Fact]
        public void AmbiguousCancellationTokenParametersAvailableForAddTimeout()
        {
            Assert.Throws<ArgumentException>(() => 
                TestMethods.AmbiguousCancellationTokenParametersAvailableForAddTimeout(new CancellationToken()));
        }
        
        [Fact]
        public async Task TimeoutNotExceededAsync()
        {
            await TestMethods.AddTimeoutAsync(providedToken =>
            {
                // A CancellationToken should have been provided by the Aspect
                Assert.NotNull(providedToken);
                return Task.CompletedTask;
            });
        }
        
        [Fact]
        public async Task TimeoutExceededAsync()
        {
            await Assert.ThrowsAsync<TaskCanceledException>(() => 
                TestMethods.AddTimeoutAsync(async providedToken =>
                {
                    // A CancellationToken should have been provided by the Aspect
                    Assert.NotNull(providedToken);
                
                    await Task.Delay(TimeoutMillis * 2, providedToken!.Value);
                }));
        }
        
        [Fact]
        public async Task TimeoutExceededAsyncWithDifferingTokenPosition()
        {
            // Test that CancellationTokens can be intercepted in any method
            // parameter position, not just the final position.
            await Assert.ThrowsAsync<TaskCanceledException>(() => 
                TestMethods.AddTimeoutAsyncWithDifferingTokenPosition(
                    new CancellationToken(),
                    async providedToken =>
                    {
                        // A CancellationToken should have been provided by the Aspect
                        Assert.NotNull(providedToken);
                    
                        await Task.Delay(TimeoutMillis * 2, providedToken!.Value);
                    }));
        }

        [Fact]
        public async Task TimeoutExceededAsyncNotPassingToChildTasks()
        {
            await TestMethods.AddTimeoutAsync(async providedToken =>
            {
                // A CancellationToken should have been provided by the Aspect
                Assert.NotNull(providedToken);

                // This Task won't be cancelled as the token wasn't propagated to it
                await Task.Delay(TimeoutMillis * 2);
                
                // Due to the above delay however, the token has had the time to expire, and so is marked as
                // "cancellation requested"
                Assert.True(providedToken!.Value.IsCancellationRequested);
            });
        }
    
        [Fact]
        public void TimeoutNotExceededNonAsync()
        {
            TestMethods.AddTimeout(providedToken =>
            {
                // A CancellationToken should have been provided by the Aspect
                Assert.NotNull(providedToken);
            });
        }
        
        [Fact]
        public void TimeoutExceededNonAsync()
        {
            var exception = Assert.Throws<AggregateException>(() => 
                TestMethods.AddTimeout(providedToken =>
                {
                    // A CancellationToken should have been provided by the Aspect
                    Assert.NotNull(providedToken);

                    Task.WaitAll(Task.Delay(TimeoutMillis * 2, providedToken!.Value));
                }));

            Assert.IsAssignableFrom<TaskCanceledException>(exception.InnerExceptions[0]);
        }
        
        [Fact]
        public void TimeoutExceededWithConfiguration()
        {
            var exception = Assert.Throws<AggregateException>(() => 
                TestMethods.AddTimeoutWithConfiguration(providedToken =>
                {
                    // A CancellationToken should have been provided by the Aspect
                    Assert.NotNull(providedToken);

                    Task.WaitAll(Task.Delay(TimeoutMillis * 2, providedToken!.Value));
                }));

            Assert.IsAssignableFrom<TaskCanceledException>(exception.InnerExceptions[0]);
        }
        
        [Fact]
        public void TimeoutExceededWithMisconfiguredConfiguration()
        {
            var exception = Assert.Throws<ArgumentException>(() => TestMethods.AddTimeoutWithMisconfiguredConfiguration());
            Assert.Equal($"Timeout configuration setting for key {MisconfiguredConfigurationItemKey} " +
                         $"must be an integer", exception.Message);
        }
        
        [Fact]
        public void TimeoutExceededWithMissingConfiguration()
        {
            var exception = Assert.Throws<ArgumentException>(() => TestMethods.AddTimeoutWithMissingConfiguration());
            Assert.Equal($"Could not find timeout configuration setting for " +
                         $"key {MissingConfigurationItemKey}", exception.Message);
        }
        
        [Fact]
        public void TimeoutExceededWithNoConfigurationSection()
        {
            CancellationTokenTimeoutAttribute.SetTimeoutConfiguration(null);

            try
            {
                var exception = Assert.Throws<ArgumentException>(() => 
                    TestMethods.AddTimeoutWithMissingConfiguration());
                
                Assert.StartsWith("Timeout configuration section cannot be null", exception.Message);
            }
            finally
            {
                CancellationTokenTimeoutAttribute.SetTimeoutConfiguration(TimeoutConfiguration);
            }
        }
        
        [Fact]
        public async Task MergeWithOriginalCancellationToken()
        {
            var originalTokenSource = new CancellationTokenSource();
            var originalToken = originalTokenSource.Token;
            
            await TestMethods.AddTimeoutAsync(providedToken =>
            {
                // A CancellationToken should have been provided by the Aspect, which is merged between the original
                // CancellationToken passed to the method, and the new Timeout one that was created by the Aspect.
                Assert.NotNull(providedToken);
                Assert.NotEqual(originalToken, providedToken!.Value);
                
                // Show that both the original and the provided Token are uncancelled at
                // this point.
                Assert.False(originalToken.IsCancellationRequested);
                Assert.False(providedToken.Value.IsCancellationRequested);
                
                // Now cancel the original CancellationToken - if the providedToken is 
                // properly merged with it, they will now both be marked as Cancelled.
                originalTokenSource.Cancel();

                // Now show that both the original and the provided Token are now marked
                // as cancelled at this point, due to being linked.
                Assert.True(originalToken.IsCancellationRequested);
                Assert.True(providedToken.Value.IsCancellationRequested);
                
                return Task.CompletedTask;
                
            }, originalToken);
        }
    }

    internal class CancellationTokenTimeoutTestFixture : IDisposable
    {
        public const string CollectionName = "CancellationTokenTimeout tests";
        public const int TimeoutMillis = 1000;
        public const string ExistingConfigurationItemKey = "ExistingConfigurationItem";
        public const string MisconfiguredConfigurationItemKey = "MisconfiguredConfigurationItem";
        public const string MissingConfigurationItemKey = "NonExistentConfigurationItem";

        public static readonly IConfiguration TimeoutConfiguration = CreateMockConfiguration(
            new Tuple<string, string>(ExistingConfigurationItemKey, $"{TimeoutMillis}"),
            new Tuple<string, string>(MisconfiguredConfigurationItemKey, "Not a number"),
            new Tuple<string, string>(MissingConfigurationItemKey, null!)
            ).Object;
        
        public CancellationTokenTimeoutTestFixture()
        {
            CancellationTokenTimeoutAspect.Enabled = true;
            CancellationTokenTimeoutAttribute.SetTimeoutConfiguration(TimeoutConfiguration);
        }

        public void Dispose()
        {
            CancellationTokenTimeoutAspect.Enabled = false;
            CancellationTokenTimeoutAttribute.SetTimeoutConfiguration(null);
        }
    }
}
