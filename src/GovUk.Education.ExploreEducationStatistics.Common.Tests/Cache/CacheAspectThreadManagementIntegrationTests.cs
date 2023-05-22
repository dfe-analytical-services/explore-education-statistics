#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Cache;

[Collection(CacheTestFixture.CollectionName)]
public class CacheAspectThreadManagementIntegrationTests : 
    IClassFixture<TestApplicationFactory<TestStartup>>,
    IClassFixture<CacheTestFixture>,
    IDisposable
{
    // An artificial delay that we will add to dummy async sections of our test setup, including getting
    // cached items setting cached items and time spent in Controller methods. 
    public const int AsyncOperationDelayMillis = 250;
    
    // A reasonable amount of time for code to reach the next step of the HTTP request processing workflow if
    // no async delaying action appears between the two steps. 
    private const int NoAsyncDelayTimeMillis = 50;
    
    // An error tolerance in the time it takes for the code after a Task.Delay() to  continue running. Task.Delay() is
    // not 100% accurate in terms of delay and so it is possible for the underlying mechanisms to wake early by a few
    // milliseconds.
    private const int TaskDelayErrorToleranceMillis = 15;

    private readonly WebApplicationFactory<TestStartup> _testApp;

    public CacheAspectThreadManagementIntegrationTests(TestApplicationFactory<TestStartup> testApp)
    {
        _testApp = testApp;
    }

    public void Dispose()
    {
        BlobCacheAttribute.ClearServices();
    }

    /// <summary>
    /// This test is testing the flow of events is as we would expect to see it when the CacheAspect code is
    /// non-blocking. It fires off an HTTP request to our TestController which uses the BlobCache attribute on its
    /// called method, and records the flow of events from there. It compares these with an expected order that would
    /// be different had the CacheAspect introduced blocking code.
    ///
    /// Unfortunately we can't tell if there is further blocking code in one of the multiple continuations that will be
    /// run as a part of this, but they would get picked up in the <see cref="NoThreadExhaustion"/> test very quickly
    /// if they existed. 
    /// </summary>
    [Fact]
    public async Task CachingControllerMethod()
    {
        // This is a list of events that we will capture in order to verify that the flow of code is showing what
        // we expect to see, especially in terms of allowing threads to be freed up and not be blocked. 
        var events = new List<Event>();
        
        var blobCacheService = new TestBlobCacheService(events);
        var app = SetupApp(blobCacheService, events);

        var client = app.CreateClient();

        var expectedCacheKey = new TestCacheKey("myRequest");
        var expectedControllerResult = new ControllerResponse("myRequest");
        
        var response = await client.GetAsync("/test/caching-method?key=myRequest");

        response.AssertOk(expectedControllerResult);

        // The first 4 items in this list of events are the most important orders of Events here. They show that the
        // CacheAspect is non-blocking in this scenario, as we can see that:
        //
        // 1. The AsyncActionFilter.OnActionExecutionAsync() method is called on its way to calling the Controller
        //    method under test - this is standard MVC behaviour. This work is done in what we'll call the Controller
        //    Request thread.
        var asyncActionFilterCalled = 
            AssertEventMessageEquals(events[0], $"{nameof(AsyncActionFilter)} OnActionExecutionAsync called");
        
        // 2. The CacheAspect AOP is encountered before hitting the actual Controller method itself, as we can see from
        //    the request for any previously existing cached item. This is also part of the original Controller Request
        //    thread.
        // 3. The caching code which aims to look up any existing cached items for the given key is encountered. This
        //    code is asynchronous (as our BlobCacheService code is) and so we are hoping to see evidence at this point
        //    that this is not causing the Controller Request thread to be blocked.
        var requestingCachedItem = 
            AssertEventMessageEquals(events[1], $"Requesting cached item with key {expectedCacheKey}");
        
        // We expect this to be called almost immediately after the AsyncActionFilter call.
        Assert.InRange(
            requestingCachedItem.Millis, 
            asyncActionFilterCalled.Millis, 
            asyncActionFilterCalled.Millis + NoAsyncDelayTimeMillis);

        // 4. We then see this evidence - the AsyncActionFilter.OnActionExecutionAsync() completes, with a
        //    non-Completed Task as its result. This indicates that there is nothing in the CacheAspect code any more
        //    that is tying up the Controller Request thread, which is now freed up to be used again.
        var asyncActionFilterCompleted = 
            AssertEventMessageEquals(events[2], $"{nameof(AsyncActionFilter)} OnActionExecutionAsync " +
                                                $"completed - returned Task IsCompleted = False");
        
        // We expect this to be called almost immediately after the cached item request call.
        Assert.InRange(
            asyncActionFilterCompleted.Millis, 
            requestingCachedItem.Millis, 
            requestingCachedItem.Millis + NoAsyncDelayTimeMillis);

        // 5. The flow then continues, showing that the cache finds no existing items with the given cache key, the
        //    original Controller method is called to get the actual result, this is then cached and the result given to
        //    the AsyncResultFilter for returning with the HTTP response.
        //
        // Previously, when the CacheAspect code was blocking the thread, we would see all of the work up to and
        // including the setting of the cached item being done *within* the AsyncActionFilter.OnActionExecutionAsync()
        // method, with the OnActionExecutionAsync method completing just prior to the AsyncResultFilter kicking in to
        // return the HTTP response to the caller.
        var noCachedItemResponse = 
            AssertEventMessageEquals(events[3], $"Returning no cached item with key {expectedCacheKey} " +
                                                $"after delay");
        
        // We expect this to be called after the delay time that it took to check for a cached item.
        Assert.InRange(
            noCachedItemResponse.Millis, 
            requestingCachedItem.Millis + AsyncOperationDelayMillis - TaskDelayErrorToleranceMillis, 
            (requestingCachedItem.Millis + AsyncOperationDelayMillis * 2) - 1);

        var controllerMethodCalled =
            AssertEventMessageEquals(events[4], "Controller method with key myRequest started");
        
        // We expect this to have been run just after the CacheAspect AOP returns with no existing cached item.
        Assert.InRange(
            controllerMethodCalled.Millis, 
            noCachedItemResponse.Millis, 
            noCachedItemResponse.Millis + NoAsyncDelayTimeMillis);
        
        var controllerMethodCompleted =
            AssertEventMessageEquals(events[5], $"Controller method with key myRequest completed after " +
                                                $"delay - returning {expectedControllerResult}");
        
        // We expect this to have been run after the async delay in the Controller method.
        Assert.InRange(
            controllerMethodCompleted.Millis, 
            controllerMethodCalled.Millis + AsyncOperationDelayMillis - TaskDelayErrorToleranceMillis, 
            (controllerMethodCalled.Millis + AsyncOperationDelayMillis * 2) - 1);

        var settingCachedItemRequest =
            AssertEventMessageEquals(events[6], $"Setting cached item {expectedControllerResult} with " +
                                                $"key {expectedCacheKey}");
        
        // We expect this to have been run just after the Controller method returns with the new item to be cached.
        Assert.InRange(
            settingCachedItemRequest.Millis, 
            controllerMethodCompleted.Millis, 
            controllerMethodCompleted.Millis + NoAsyncDelayTimeMillis);

        var settingCachedItemCompleted =
            AssertEventMessageEquals(events[7], $"Set cached item {expectedControllerResult} with " +
                                                $"key {expectedCacheKey} after delay");
        
        // We expect this to have been run after the async delay in the cache setter method.
        Assert.InRange(
            settingCachedItemCompleted.Millis, 
            settingCachedItemRequest.Millis + AsyncOperationDelayMillis - TaskDelayErrorToleranceMillis, 
            (settingCachedItemRequest.Millis + AsyncOperationDelayMillis * 2) - 1);

        var asyncResultFilterCalled =
            AssertEventMessageEquals(events[8], $"{nameof(AsyncResultFilter)} OnResultExecutionAsync called");
        
        // We expect this to have been run just after the new item is set in the cache and returned as the result from
        // the Controller method to the MVC framework.
        Assert.InRange(
            asyncResultFilterCalled.Millis, 
            settingCachedItemCompleted.Millis, 
            settingCachedItemCompleted.Millis + NoAsyncDelayTimeMillis);

        var asyncResultFilterCompleted =
            AssertEventMessageEquals(events[9], $"{nameof(AsyncResultFilter)} OnResultExecutionAsync " +
                                                $"completed - returned Task IsCompleted = True");
        
        // We expect this to have been run just after the response is provided from the Controller.
        Assert.InRange(
            asyncResultFilterCompleted.Millis, 
            asyncResultFilterCalled.Millis, 
            asyncResultFilterCalled.Millis + NoAsyncDelayTimeMillis);
    }
    
    /// <summary>
    /// This test is testing the flow of events is as we would expect to see it when the CacheAspect code is
    /// non-blocking and in particular the scenario when an existing cache entry already exists.
    ///
    /// It fires off an HTTP request to our TestController which uses the BlobCache attribute on its
    /// called method, and records the flow of events from there. It compares these with an expected order that would
    /// be different had the CacheAspect introduced blocking code.
    ///
    /// Unfortunately we can't tell if there is further blocking code in one of the multiple continuations that will be
    /// run as a part of this, but they would get picked up in the <see cref="NoThreadExhaustion"/> test very quickly
    /// if they existed. 
    /// </summary>
    [Fact]
    public async Task CachingControllerMethod_ExistingCachedItem()
    {
        // This is a list of events that we will capture in order to verify that the flow of code is showing what
        // we expect to see, especially in terms of allowing threads to be freed up and not be blocked. 
        var events = new List<Event>();
        
        var blobCacheService = new TestBlobCacheService(events);
        var app = SetupApp(blobCacheService, events);

        var client = app.CreateClient();

        var expectedCacheKey = new TestCacheKey("myRequest");
        var expectedControllerResult = new ControllerResponse("existingItem");

        await blobCacheService.SetItemAsync(new TestCacheKey("myRequest"), new ControllerResponse("existingItem"));
        events.Clear();
        
        var response = await client.GetAsync("/test/caching-method?key=myRequest");

        response.AssertOk(expectedControllerResult);
        
        // A reasonable amount of time for code to reach the next step of the HTTP request processing workflow if
        // no async delaying action appears between the two steps. 
        var noDelayTimeMillis = AsyncOperationDelayMillis / 2;

        // The first 4 items in this list of events are the most important orders of Events here. They show that the
        // CacheAspect is non-blocking in this scenario, as we can see that:
        //
        // 1. The AsyncActionFilter.OnActionExecutionAsync() method is called on its way to calling the Controller
        //    method under test - this is standard MVC behaviour. This work is done in what we'll call the Controller
        //    Request thread.
        var asyncActionFilterCalled = 
            AssertEventMessageEquals(events[0], $"{nameof(AsyncActionFilter)} OnActionExecutionAsync called");
        
        // 2. The CacheAspect AOP is encountered before hitting the actual Controller method itself, as we can see from
        //    the request for any previously existing cached item. This is also part of the original Controller Request
        //    thread.
        // 3. The caching code which aims to look up any existing cached items for the given key is encountered. This
        //    code is asynchronous (as our BlobCacheService code is) and so we are hoping to see evidence at this point
        //    that this is not causing the Controller Request thread to be blocked.
        var requestingCachedItem = 
            AssertEventMessageEquals(events[1], $"Requesting cached item with key {expectedCacheKey}");
        
        // We expect this to be called almost immediately after the AsyncActionFilter call.
        Assert.InRange(
            requestingCachedItem.Millis, 
            asyncActionFilterCalled.Millis, 
            asyncActionFilterCalled.Millis + noDelayTimeMillis);

        // 4. We then see this evidence - the AsyncActionFilter.OnActionExecutionAsync() completes, with a
        //    non-Completed Task as its result. This indicates that there is nothing in the CacheAspect code any more
        //    that is tying up the Controller Request thread, which is now freed up to be used again.
        var asyncActionFilterCompleted = 
            AssertEventMessageEquals(events[2], $"{nameof(AsyncActionFilter)} OnActionExecutionAsync " +
                                                $"completed - returned Task IsCompleted = False");
        
        // We expect this to be called almost immediately after the cached item request call.
        Assert.InRange(
            asyncActionFilterCompleted.Millis, 
            requestingCachedItem.Millis, 
            requestingCachedItem.Millis + noDelayTimeMillis);

        // 5. Because the cache finds an existing item with the given key, it's able to return that in its continuation
        //    and the AsyncResultFilter is then able to return that with the HTTP response.
        var existingCachedItemResponse = 
            AssertEventMessageEquals(events[3], $"Returning cached item {expectedControllerResult} " +
                                                $"with key {expectedCacheKey} after delay");
        
        // We expect this to be called after the delay time that it took to check for a cached item.
        Assert.InRange(
            existingCachedItemResponse.Millis, 
            requestingCachedItem.Millis + AsyncOperationDelayMillis - TaskDelayErrorToleranceMillis, 
            (requestingCachedItem.Millis + AsyncOperationDelayMillis * 2) - 1);

        var asyncResultFilterCalled =
            AssertEventMessageEquals(events[4], $"{nameof(AsyncResultFilter)} OnResultExecutionAsync called");
        
        // We expect this to have been run just after the existing cached item is returned as the result from the
        // Controller method to the MVC framework.
        Assert.InRange(
            asyncResultFilterCalled.Millis, 
            existingCachedItemResponse.Millis, 
            existingCachedItemResponse.Millis + noDelayTimeMillis);

        var asyncResultFilterCompleted =
            AssertEventMessageEquals(events[5], $"{nameof(AsyncResultFilter)} OnResultExecutionAsync " +
                                                $"completed - returned Task IsCompleted = True");
        
        // We expect this to have been run just after the response is provided from the Controller.
        Assert.InRange(
            asyncResultFilterCompleted.Millis, 
            asyncResultFilterCalled.Millis, 
            asyncResultFilterCalled.Millis + noDelayTimeMillis);
    }
    
    /// <summary>
    /// This test simulates creating a number of simultaneous HTTP requests and measuring the effects in terms of
    /// time to process them all and the growth of the Worker Thread pool during the process.
    ///
    /// In a scenario where there was blocking code accidentally introduced, the current Worker Thread count would
    /// immediately increase as all the available threads get blocked. The time taken to complete processing all of
    /// the requests would also greatly exceed the expected maximum time to complete. This has been proven by
    /// deliberately adding blocking back into the <see cref="CacheAttribute.WrapAsync{T}"/> implementation and seeing
    /// these tests fail.
    /// </summary>
    /// <exception cref="Exception">Fails with an Exception if the specified 1 minute timeout is breached.</exception>
    [Fact(Timeout = 60000)]
    public async Task NoThreadExhaustion()
    {
        var events = new List<Event>();
        var blobCacheService = new TestBlobCacheService(events);
        var app = SetupApp(blobCacheService, events);
        
        var client = app.CreateClient();

        // Get a count of the current number of worker threads as this test starts.
        var initialWorkerThreads = GetWorkerThreadsCurrentlyInUse();

        var stopwatch = Stopwatch.StartNew();

        // Generate a number of HTTP requests.
        const int numberOfRequestsToProcess = 20;
        
        var requests = Enumerable
            .Range(0, numberOfRequestsToProcess)
            .Select(index => client.GetAsync($"/test/caching-method?key=request{index}"))
            .ToList();

        var allResults = Task.WhenAll(requests);

        // Start a Task which periodically interrogates the Thread Pool for the current number of worker threads in
        // order to identify a rise in blocked threads. In the scenario where blocking code was deliberately added
        // into the CacheAttribute async code, this thread escalation was caught almost immediately and the resultant
        // Exception would cause the test to fail.    
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
        var timerTask = Task.Run(async () =>
        {
            while (!allResults.IsCompleted)
            {
                await timer.WaitForNextTickAsync();
                var currentWorkerThreadsInUse = GetWorkerThreadsCurrentlyInUse();
                
                if (currentWorkerThreadsInUse - initialWorkerThreads > numberOfRequestsToProcess)
                {
                    throw new Exception("Number of additional concurrent worker threads exceeds limit " +
                                        $"of {numberOfRequestsToProcess}. {currentWorkerThreadsInUse} more threads are " +
                                        "currently in use");
                }
            }
        });

        // Wait for all of the HTTP responses to complete. 
        await Task.WhenAll(allResults, timerTask);

        // Verify that all HTTP responses returned the expected results and that all of the results were also
        // successfully cached.
        var expectedResponses = Enumerable
            .Range(0, 20)
            .Select(i => new ControllerResponse($"request{i}"))
            .ToList();
        
        Assert.Equal(
            expectedResponses, 
            requests.Select(r => r.Result.Content.ReadFromJson<ControllerResponse>()));
        
        Assert.Equal(
            expectedResponses.OrderBy(r => r.Key), 
            blobCacheService.Cache.Values.OrderBy(r => r.Key));
        
        // We expect the minimum amount of time for a single request in isolation to also be the minimum time this code
        // can take to run. This includes 3 pieces of code using Task.Delay(AsyncOperationDelayMillis), so this is a 
        // good indication for a bare minimum amount of time it takes to run this code as we expect it to execute (and
        // also taking into consideration the slight error tolerance for Task.Delays waking early)
        var totalTimeMillis = stopwatch.ElapsedMilliseconds;
        const int expectedMinimumTimeToRun = (AsyncOperationDelayMillis - TaskDelayErrorToleranceMillis) * 3;
        
        Assert.True(
            totalTimeMillis >= expectedMinimumTimeToRun, 
            $"Total time to process {numberOfRequestsToProcess} took {totalTimeMillis}, which is less than " +
            $"expected minimum time of {expectedMinimumTimeToRun}");

        // We expect all of these requests to be processed almost concurrently with no threads blocking each other, so
        // on a fast machine we would expect 20 requests to complete almost as fast as a single request.
        // To allow for slower environments however e.g. CI and with the impact of other resources in play at the time
        // of running this test, we will expect the maximum time to be 20 times that.
        const int expectedTimeThresholdToRun = expectedMinimumTimeToRun * 20;
        
        Assert.True(
            totalTimeMillis < expectedTimeThresholdToRun, 
            $"Total time to process {numberOfRequestsToProcess} took {totalTimeMillis}, which exceeds " +
            $"expected threshold of {expectedTimeThresholdToRun}");
    }

    private static int GetWorkerThreadsCurrentlyInUse()
    {
        ThreadPool.GetMaxThreads(out var maxWorkerThreads, out _);
        ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out _);
        return maxWorkerThreads - availableWorkerThreads;
    }

    private WebApplicationFactory<TestStartup> SetupApp(
        IBlobCacheService blobCacheService,
        List<Event> events)
    {
        var app = _testApp
            .ConfigureServices(services => services
                .AddSingleton(_ => blobCacheService)
                .AddSingleton(_ => events)
                .AddMvcCore(options =>
                {
                    options.Filters.Add<AsyncActionFilter>();
                    options.Filters.Add<AsyncResultFilter>();
                }))
            .WithWebHostBuilder(builder => builder
                .WithAdditionalControllers(typeof(TestController)));

        BlobCacheAttribute.AddService("default", blobCacheService);
        
        return app;
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class AsyncActionFilter : IAsyncActionFilter
    {
        private readonly List<Event> _events;

        public AsyncActionFilter(List<Event> events)
        {
            _events = events;
        }

        public Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _events.Add(new Event($"{nameof(AsyncActionFilter)} OnActionExecutionAsync called"));
        
            var result = next();
        
            _events.Add(new Event($"{nameof(AsyncActionFilter)} OnActionExecutionAsync completed - returned Task IsCompleted = {result.IsCompleted}"));
        
            return result;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class AsyncResultFilter : IAsyncResultFilter
    {
        private readonly List<Event> _events;

        public AsyncResultFilter(List<Event> events)
        {
            _events = events;
        }

        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            _events.Add(new Event($"{nameof(AsyncResultFilter)} OnResultExecutionAsync called"));
        
            var result = next();
        
            _events.Add(new Event($"{nameof(AsyncResultFilter)} OnResultExecutionAsync completed - returned Task IsCompleted = {result.IsCompleted}"));
        
            return result;
        }
    }
    
    /// <summary>
    /// This TestBlobCacheService adds an artificial delay to its GetItemAsync and SetItemAsync methods to simulate some
    /// async work e.g. reading and writing from Azure Blob Storage.
    /// </summary>
    private class TestBlobCacheService : IBlobCacheService
    {
        public readonly ConcurrentDictionary<TestCacheKey, ControllerResponse> Cache = new();
        private readonly List<Event> _events;

        public TestBlobCacheService(List<Event> events)
        {
            _events = events;
        }
        
        public object GetItem(IBlobCacheKey cacheKey, Type targetType)
        {
            throw new NotImplementedException();
        }

        public async Task<object?> GetItemAsync(IBlobCacheKey cacheKey, Type targetType)
        {
            _events.Add(new Event($"Requesting cached item with key {cacheKey}"));
            
            var key = cacheKey as TestCacheKey;
            await Task.Delay(AsyncOperationDelayMillis);
            
            if (Cache.ContainsKey(key!))
            {
                var result = Cache[key!];
                _events.Add(new Event($"Returning cached item {result} with key {cacheKey} after delay"));
                return result;
            }
            
            _events.Add(new Event($"Returning no cached item with key {cacheKey} after delay"));
            return null;
        }

        public void SetItem<TItem>(IBlobCacheKey cacheKey, TItem item)
        {
            throw new NotImplementedException();
        }

        public async Task SetItemAsync<TItem>(IBlobCacheKey cacheKey, TItem item)
        {
            _events.Add(new Event($"Setting cached item {item} with key {cacheKey}"));
            
            var result = item as ControllerResponse;
            Cache[(cacheKey as TestCacheKey)!] = result!;
            await Task.Delay(AsyncOperationDelayMillis);
            
            _events.Add(new Event($"Set cached item {item} with key {cacheKey} after delay"));
        }

        public Task DeleteItemAsync(IBlobCacheKey cacheKey)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCacheFolderAsync(IBlobCacheKey cacheFolderKey)
        {
            throw new NotImplementedException();
        }
    }
    
    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private static Event AssertEventMessageEquals(Event e, string expectedMessage)
    {
        Assert.Equal(expectedMessage, e.Message);
        return e;
    }
    
    /// <summary>
    /// This TestController has a single async method that has a BlobCache attribute.
    /// This method uses an artificial delay to simulate doing some async work e.g. calling some code that interacts
    /// with the database. 
    /// </summary>
    private class TestController : ControllerBase
    {
        private readonly List<Event> _events;

        public TestController(List<Event> events)
        {
            _events = events;
        }
        
        [BlobCache(typeof(TestCacheKey))]
        [HttpGet("/test/caching-method")]
        public async Task<ControllerResponse> CachingMethod(
            [FromQuery] string key)
        {
            _events.Add(new Event($"Controller method with key {key} started"));
            await Task.Delay(AsyncOperationDelayMillis);

            var resultToCache = new ControllerResponse(key);
            _events.Add(new Event($"Controller method with key {key} completed after delay - returning {resultToCache}"));
            return resultToCache;
        }
    }

    private record ControllerResponse(string Key);

    private record TestCacheKey(string Key) : IBlobCacheKey
    {
        public IBlobContainer Container => null!;
    }

    private record Event(string Message)
    {
        public long Millis { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}