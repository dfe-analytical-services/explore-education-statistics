#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Cancellation.UseCapturedCancellationTokenAttribute.NoExistingTokenBehaviour;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.AssertExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Cancellation
{
    public class CancellationAspectTests
    {
        private const int TimeoutMillis = 10;
        
        [Collection(CancellationTestFixture.CollectionName)]
        public class CaptureCancellationTokenTests : IClassFixture<CancellationTestFixture>
        {
            private static class TestMethods
            {
                [CaptureCancellationToken]
                public static void NoCancellationTokenParameterAvailableForCapture()
                {
                }
            
                [CaptureCancellationToken]
                public static void CaptureCancellationToken(Action<CancellationToken?> assertions, CancellationToken token)
                {
                    assertions.Invoke(token);
                }
            }
            
            [Fact]
            public void NoCancellationTokenParameterAvailableForCapture()
            {
                Assert.Throws<ArgumentException>(TestMethods.NoCancellationTokenParameterAvailableForCapture);
            }

            [Fact]
            public void CaptureCancellationToken()
            {
                var originalCancellationToken = new CancellationToken();
                
                // Assert that there is originally no CancellationToken captured.
                Assert.Null(CancellationContext.GetCurrent());
                
                TestMethods.CaptureCancellationToken(providedToken =>
                {
                    // The CancellationToken that the method was called with should be the same
                    // one that the target method was called with, unchanged by Advice
                    Assert.Equal(providedToken!.Value, originalCancellationToken);
                
                    // Assert that the CancellationToken has been captured on the thread.
                    Assert.Equal(originalCancellationToken, CancellationContext.GetCurrent());
                }, originalCancellationToken);
                
                // Assert that after the method completes, the CancellationToken is no longer captured.
                Assert.Null(CancellationContext.GetCurrent());
            }

            [Fact]
            public void CaptureCancellationToken_ExceptionThrown()
            {
                var originalCancellationToken = new CancellationToken();
                
                // Assert that there is originally no CancellationToken captured.
                Assert.Null(CancellationContext.GetCurrent());
                
                Assert.Throws<OperationCanceledException>(() => 
                    TestMethods.CaptureCancellationToken(providedToken =>
                    {
                        // The CancellationToken that the method was called with should be the same
                        // one that the target method was called with, unchanged by Advice
                        Assert.Equal(providedToken!.Value, originalCancellationToken);

                        // Assert that the CancellationToken has been captured on the thread.
                        Assert.Equal(originalCancellationToken, CancellationContext.GetCurrent());

                        // Now throw an Exception.
                        throw new OperationCanceledException("Throwing an Exception");
                    }, originalCancellationToken));
                
                // Assert that after the method has thrown its Exception, the CancellationToken is no longer captured.
                Assert.Null(CancellationContext.GetCurrent());
            }
        }

        [Collection(CancellationTestFixture.CollectionName)]
        public class AddTimeoutCancellationTests : IClassFixture<CancellationTestFixture>
        {
            private static class TestMethods
            {
                [AddTimeoutCancellation(TimeoutMillis)]
                public static async Task AddTimeoutAsync(Func<CancellationToken?, Task> assertions, CancellationToken? token = null)
                {
                    await assertions.Invoke(token);
                }
            
                [AddTimeoutCancellation(TimeoutMillis)]
                public static void NoCancellationTokenParameterAvailableForAddTimeout()
                {
                }
            
                [AddTimeoutCancellation(TimeoutMillis)]
                public static void AddTimeoutNonAsync(Action<CancellationToken?> assertions, CancellationToken? token = null)
                {
                    assertions.Invoke(token);
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

                    await Task.Delay(TimeoutMillis * 2);
                    
                    Assert.True(providedToken!.Value.IsCancellationRequested);
                });
            }
        
            [Fact]
            public void TimeoutNotExceededNonAsync()
            {
                TestMethods.AddTimeoutNonAsync(providedToken =>
                {
                    // A CancellationToken should have been provided by the Aspect
                    Assert.NotNull(providedToken);
                });
            }
            
            [Fact]
            public void TimeoutExceededNonAsync()
            {
                var exception = Assert.Throws<AggregateException>(() => 
                    TestMethods.AddTimeoutNonAsync(providedToken =>
                    {
                        // A CancellationToken should have been provided by the Aspect
                        Assert.NotNull(providedToken);

                        Task.WaitAll(Task.Delay(TimeoutMillis * 2, providedToken!.Value));
                    }));

                Assert.IsAssignableFrom<TaskCanceledException>(exception.InnerExceptions[0]);
            }
            
            [Fact]
            public async Task MergeWithOriginalCancellationToken()
            {
                var originalTokenSource = new CancellationTokenSource();
                var originalToken = originalTokenSource.Token;
                
                await TestMethods.AddTimeoutAsync(providedToken =>
                {
                    // A CancellationToken should have been provided by the Aspect
                    Assert.NotNull(providedToken);
                    
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

        [Collection(CancellationTestFixture.CollectionName)]
        public class AddTimeoutAndCaptureTests : IClassFixture<CancellationTestFixture>
        {
            private static class TestMethods
            {
                [AddTimeoutCancellation(TimeoutMillis)]
                [CaptureCancellationToken]
                public static async Task AddTimeoutAndCaptureTokenAsync(Func<CancellationToken?, Task> assertions, CancellationToken? token = null)
                {
                    await assertions.Invoke(token);
                }
            }
            
            [Fact]
            public async Task AddTimeoutAndCaptureTokenAsync()
            {
                await Assert.ThrowsAsync<TaskCanceledException>(() => 
                    TestMethods.AddTimeoutAndCaptureTokenAsync(async providedToken =>
                    {
                        // A CancellationToken should have been provided by the Aspect
                        Assert.NotNull(providedToken);
                        
                        // Assert that the CancellationToken has been captured on the thread.
                        Assert.NotNull(CancellationContext.GetCurrent());
                    
                        await Task.Delay(TimeoutMillis * 2, providedToken!.Value);
                    }));
            }
            
            [Fact]
            public async Task AddTimeoutAndCaptureTokenAsync_MergeWithOriginalToken()
            {
                var originalTokenSource = new CancellationTokenSource();
                var originalToken = originalTokenSource.Token;
                
                await Assert.ThrowsAsync<TaskCanceledException>(() => 
                    TestMethods.AddTimeoutAndCaptureTokenAsync(async providedToken =>
                    {
                        // A CancellationToken should have been provided by the Aspect
                        Assert.NotNull(providedToken);
                        
                        // The CancellationToken should not be the same as the one that the original
                        // method was called on, as the Advice should have added a Timeout to it.
                        Assert.NotEqual(providedToken, originalToken);

                        // This same CancellationToken should also have been captured on the thread.
                        Assert.Equal(providedToken, CancellationContext.GetCurrent());
                        
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
                    
                        await Task.Delay(TimeoutMillis * 2, providedToken.Value);
                    }, originalToken));
                
                Assert.Null(CancellationContext.GetCurrent());
            }
        }
        
        [Collection(CancellationTestFixture.CollectionName)]
        public class UseCapturedCancellationTokenTests : IClassFixture<CancellationTestFixture>
        {
            private static class TestMethods
            {
                [UseCapturedCancellationToken]
                public static void NoCancellationTokenParameter()
                {
                    
                }
                
                [UseCapturedCancellationToken]
                public static void CancellationTokenParameter(
                    Action<CancellationToken?> assertions, 
                    CancellationToken? providedToken = null)
                {
                    assertions.Invoke(providedToken);
                }
                
                [UseCapturedCancellationToken(noTokenBehaviour: Throw)]
                public static void CancellationTokenParameterThrowsIfNoneExists(
                    Action<CancellationToken?>? assertions = null, 
                    CancellationToken? providedToken = null)
                {
                    assertions?.Invoke(providedToken);
                }
                
                [UseCapturedCancellationToken]
                [AddTimeoutCancellation(TimeoutMillis)]
                public static void UseCapturedTokenAndAddTimeout(
                    Action<CancellationToken?> assertions, 
                    CancellationToken? providedToken = null)
                {
                    assertions.Invoke(providedToken);
                }
            }
            
            [Fact]
            public void UseCapturedCancellationTokenWithNoCancellationTokenParameter()
            {
                Assert.Throws<ArgumentException>(TestMethods.NoCancellationTokenParameter);
            }
            
            [Fact]
            public void UseCapturedCancellationToken_WithNoExistingCapturedToken()
            {
                TestMethods.CancellationTokenParameter(providedToken =>
                {
                    // A CancellationToken could not be provided as none has yet been captured.
                    Assert.Null(providedToken);
                });
            }
            
            [Fact]
            public void UseCapturedCancellationToken_WithExistingCapturedToken()
            {
                var capturedToken = new CancellationToken();
                CancellationContext.SetCurrent(capturedToken);

                TestMethods.CancellationTokenParameter(providedToken =>
                {
                    // The CancellationToken captured on the thread should get provided by the Advice.
                    Assert.Equal(capturedToken, providedToken);
                });
            }
            
            [Fact]
            public void UseCapturedCancellationTokenThrowsIfNoneExists_WithNoExistingCapturedToken()
            {
                Assert.Throws<ArgumentException>(() => TestMethods.CancellationTokenParameterThrowsIfNoneExists());
            }
            
            [Fact]
            public void UseCapturedCancellationTokenThrowsIfNoneExists_WithExistingCapturedToken()
            {
                var capturedToken = new CancellationToken();
                CancellationContext.SetCurrent(capturedToken);

                TestMethods.CancellationTokenParameter(providedToken =>
                {
                    // The CancellationToken captured on the thread should get provided by the Advice.
                    Assert.Equal(capturedToken, providedToken);
                });
            }

            [Fact]
            public void UseCapturedTokenAndAddTimeout_WithExistingCapturedToken()
            {
                var capturedToken = new CancellationToken();
                CancellationContext.SetCurrent(capturedToken);

                var exception = Assert.Throws<AggregateException>(() => 
                    TestMethods.UseCapturedTokenAndAddTimeout(providedToken =>
                    {
                        // A CancellationToken should get provided by the Advice, based on the captured Token and with the
                        // additional Timeout.
                        Assert.NotNull(providedToken);
                        Assert.NotEqual(capturedToken, providedToken);

                        // Wait for longer than the Timeout, to get a Task Cancellation.
                        Task.WaitAll(Task.Delay(TimeoutMillis * 2, providedToken!.Value));
                    }));
                
                Assert.IsAssignableFrom<TaskCanceledException>(exception.InnerExceptions[0]);
            }
            
            [Fact]
            public void UseCapturedTokenAndAddTimeout_ManuallyCancellingOriginalToken()
            {
                var capturedTokenSource = new CancellationTokenSource();
                var capturedToken = capturedTokenSource.Token;
                CancellationContext.SetCurrent(capturedToken);

                TestMethods.UseCapturedTokenAndAddTimeout(providedToken =>
                {
                    // A CancellationToken should get provided by the Advice, based on the captured Token and with the
                    // additional Timeout.
                    Assert.NotNull(providedToken);
                    Assert.NotEqual(capturedToken, providedToken);
                    
                    // Show that both the original and the provided Token are uncancelled at
                    // this point.
                    Assert.False(capturedToken.IsCancellationRequested);
                    Assert.False(providedToken!.Value.IsCancellationRequested);
                
                    // Now cancel the original CancellationToken - if the providedToken is 
                    // properly merged with it, they will now both be marked as Cancelled.
                    capturedTokenSource.Cancel();

                    // Now show that both the original and the provided Token are now marked
                    // as cancelled at this point, due to being linked.
                    Assert.True(capturedToken.IsCancellationRequested);
                    Assert.True(providedToken.Value.IsCancellationRequested);
                });
            }
        }
    }
}