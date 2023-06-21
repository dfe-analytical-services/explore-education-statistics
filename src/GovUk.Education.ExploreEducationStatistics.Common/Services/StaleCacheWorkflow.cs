# nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public record StaleCacheWorkflow<TCacheKey, TLogger>(
    Func<TCacheKey, Task<object>> GetCachedItemFn,
    Func<TCacheKey, Task<CacheItemMeta>> GetCacheItemMetaFn,
    Func<TCacheKey, object, Task> CacheItemFn,
    ILogger<TLogger>? Logger = null)
{
    private readonly CacheLocks<TCacheKey> _locks = new();
    
    public async Task<object?> GetOrCreateItemAsync(
        TCacheKey cacheKey,
        Func<Task<object>> createItemFn)
    {
        // *Is other request currently generating cached content?
        // *No
        // *  Is there any cached content?
        // *     No
        // *       Acquire lock to identify that this request will be generating the content if necessary (using try-catch)
        // *       Generate and cache it
        // *       Release lock
        // *       Return it
        // *    Yes
        // *       Is it stale?
        // *         No
        // *           Return it
        // *         Yes
        // *           Acquire lock to identify that this request will be generating the content if necessary (using try-catch)
        // *           Regenerate it and cache it
        // *           Release lock
        // *           Return it
        // *Yes
        // *  Is stale content available?
        // *    Yes
        // *      Return the stale content
        // *    No
        // *      Wait on lock that other request has acquired
        //        Did lock time out?
        //          Yes
        //            Return timeout error
        //          No
        //          Once lock has been released, is the content now in the cache?
        //            Yes
        //              Return the content that is now in the cache
        //            No
        //              Other request must have failed.
        //              Acquire lock to identify that this request will be generating the content if necessary (using try-catch)
        //              Regenerate it and cache it
        //              Release lock
        //              Return it
        
        // See if there is any existing cached content.
        var meta = await GetCacheItemMetaFn.Invoke(cacheKey);

        // Check to see if another thread is currently generating cached results for this
        // Blob Container / Cache Key combination.
        var existingLock = _locks.GetCurrentCacheLock(cacheKey);

        // No other thread is currently generating cached content for this container / key combination.
        if (existingLock == null)
        {
            // If not, acquire the lock so that this thread can generate the content to be cached.
            if (!meta.Exists)
            {
                return await AcquireLockAndGenerateContent(cacheKey, createItemFn);
            }
            
            // Is the existing content stale?
            if (!meta.Stale)
            {
                // Return the fresh cached content.
                return GetCachedItemFn.Invoke(cacheKey);
            }
            
            return await AcquireLockAndGenerateContent(cacheKey, createItemFn);
        }
        
        //
        // Another thread is currently generating this cached content.
        //
        
        // Does any cached content currently exist whilst the other thread is generating new content?
        if (meta.Exists)
        {
            // Return existing (stale) content.
            return GetCachedItemFn.Invoke(cacheKey);
        }
        
        // No cached content exists whilst the other thread is generating new cached content, so wait
        // for the other thread to generate the content and then return it.
        try
        {
            await _locks.Await(cacheKey);
            
            // TODO is it worth handling scenario where this is null? e.g. the other thread failed to generate the content
            return GetCachedItemFn.Invoke(cacheKey);
        }
        // Have we timed out waiting for the other thread to generate the content?
        catch (TimeoutException e)
        {
            Logger.LogError("Timed out waiting for other thread to generate cached content for key {CacheKey}", 
                cacheKey);
            throw e;
        }
    }

    private async Task<object> AcquireLockAndGenerateContent(
        TCacheKey cacheKey, 
        Func<Task<object>> createItemFn)
    {
        try
        {
            _locks.Acquire(cacheKey);

            // Then generate the content to be cached.
            var content = await createItemFn.Invoke();

            // Cache it.
            await CacheItemFn(cacheKey, content);

            // return the newly cached content.
            return content;
        }
        finally
        {
            // Release the lock.
            _locks.Release(cacheKey);
        }
    }
}

public record CacheItemMeta(bool Exists, bool Stale);

record CacheLocks<TCacheKey>
{
    public CacheLock<TCacheKey>? GetCurrentCacheLock(TCacheKey key)
    {
        return null;
    }

    public CacheLock<TCacheKey> Acquire(TCacheKey key)
    {
        throw new NotImplementedException();
    }

    public void Remove(CacheLock<TCacheKey> cacheLock)
    {
        throw new NotImplementedException();
    }

    public void Release(TCacheKey key)
    {
        throw new NotImplementedException();
    }

    public async Task Await(TCacheKey key)
    {
        throw new NotImplementedException();
    }
}

record CacheLock<TCacheKey>(TCacheKey CacheKey) {
        
    Task AwaitCachedContent()
    {
        return Task.CompletedTask;
    }

    public void Acquire()
    {
        throw new NotImplementedException();
    }

    public void Release()
    {
        throw new NotImplementedException();
    }
}