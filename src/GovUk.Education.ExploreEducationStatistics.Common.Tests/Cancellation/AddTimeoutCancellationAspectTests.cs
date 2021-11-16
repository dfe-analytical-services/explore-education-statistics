#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using Microsoft.Extensions.Configuration;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Cancellation.AddTimeoutCancellationTestFixture;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Cancellation
{
    [Collection(CollectionName)]
    public class AddTimeoutCancellationAspectTests : IClassFixture<AddTimeoutCancellationTestFixture>
    {
        private static class TestMethods
        {
            [AddTimeoutCancellation(TimeoutMillis)]
            public static void NoCancellationTokenParameterAvailableForAddTimeout()
            {
            }
        
            [AddTimeoutCancellation(TimeoutMillis)]
            public static void AddTimeout(
                Action<CancellationToken?> assertions, 
                CancellationToken? token = null)
            {
                assertions.Invoke(token);
            }
            
            [AddTimeoutCancellation(TimeoutMillis)]
            public static async Task AddTimeoutAsync(
                Func<CancellationToken?, Task> assertions, 
                CancellationToken? token = null)
            {
                await assertions.Invoke(token);
            }
            
            [AddTimeoutCancellation(ExistingConfigurationItemKey)]
            public static void AddTimeoutWithConfiguration(
                Action<CancellationToken?> assertions, 
                CancellationToken? token = null)
            {
                assertions.Invoke(token);
            }
            
            [AddTimeoutCancellation(MisconfiguredConfigurationItemKey)]
            public static void AddTimeoutWithMisconfiguredConfiguration(CancellationToken? token = null)
            {
            }
            
            [AddTimeoutCancellation(MissingConfigurationItemKey)]
            public static void AddTimeoutWithMissingConfiguration(CancellationToken? token = null)
            {
            }
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
            Assert.Equal($"TimeoutConfiguration configuration setting for key {MisconfiguredConfigurationItemKey} " +
                         $"must be an integer", exception.Message);
        }
        
        [Fact]
        public void TimeoutExceededWithMissingConfiguration()
        {
            var exception = Assert.Throws<ArgumentException>(() => TestMethods.AddTimeoutWithMissingConfiguration());
            Assert.Equal($"Could not find TimeoutConfiguration configuration setting for " +
                         $"key {MissingConfigurationItemKey}", exception.Message);
        }
        
        [Fact]
        public void TimeoutExceededWithNoConfigurationSection()
        {
            AddTimeoutCancellationAttribute.SetTimeoutConfiguration(null);

            try
            {
                var exception = Assert.Throws<ArgumentException>(() => 
                    TestMethods.AddTimeoutWithMissingConfiguration());
                
                Assert.StartsWith("TimeoutConfiguration section cannot be null", exception.Message);
            }
            finally
            {
                AddTimeoutCancellationAttribute.SetTimeoutConfiguration(TimeoutConfiguration);
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
                Assert.False(providedToken!.Value.IsCancellationRequested);
                
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

        [Fact]
        public void NoCancellationTokenParameterAvailableForAddTimeout()
        {
            Assert.Throws<ArgumentException>(TestMethods.NoCancellationTokenParameterAvailableForAddTimeout);
        }
    }

    internal class AddTimeoutCancellationTestFixture : IDisposable
    {
        public const string CollectionName = "AddTimeoutCancellation tests";
        public const int TimeoutMillis = 10;
        public const string ExistingConfigurationItemKey = "ExistingConfigurationItem";
        public const string MisconfiguredConfigurationItemKey = "MisconfiguredConfigurationItem";
        public const string MissingConfigurationItemKey = "NonExistentConfigurationItem";

        public static readonly IConfiguration TimeoutConfiguration = CreateMockConfiguration(
            new Tuple<string, string>(ExistingConfigurationItemKey, $"{TimeoutMillis}"),
            new Tuple<string, string>(MisconfiguredConfigurationItemKey, "Not a number"),
            new Tuple<string, string>(MissingConfigurationItemKey, null!)
            ).Object;
        
        public AddTimeoutCancellationTestFixture()
        {
            AddTimeoutCancellationAspect.Enabled = true;
            AddTimeoutCancellationAttribute.SetTimeoutConfiguration(TimeoutConfiguration);
        }

        public void Dispose()
        {
            AddTimeoutCancellationAspect.Enabled = false;
            AddTimeoutCancellationAttribute.SetTimeoutConfiguration(null);
        }
    }
}