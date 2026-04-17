import cacheControlHeaders from '@frontend/middleware/headers/cacheControlHeaders';
import runMiddleware from '@frontend/middleware/__tests__/util/runMiddleware';

describe('cacheControlHeaders', () => {
  test('applies a Cache-Control header for assets fetched through a Next API route to cache for a moderate amount of time', async () => {
    const response = await getResponseForUrl(
      'https://my-env/api/assets/an-asset',
    );
    expect(response.headers.get('Cache-Control')).toBe('public, max-age=86400');
  });

  test('applies a Cache-Control header for general responses fetched through a Next API route to prevent caching', async () => {
    const response = await getResponseForUrl('https://my-env/api/a-service');
    expect(response.headers.get('Cache-Control')).toBe(
      'no-store, no-cache, must-revalidate',
    );
  });

  it.each(['favicon.svg', 'manifest.json'])(
    `applies a Cache-Control header to cache metadata file "%s" for a small amount of time`,
    async metafileName => {
      const response = await getResponseForUrl(
        `https://my-env/${metafileName}`,
      );
      expect(response.headers.get('Cache-Control')).toBe(
        'public, max-age=3600',
      );
    },
  );

  test('applies a Cache-Control header for immutable assets to cache indefinitely', async () => {
    const response = await getResponseForUrl('https://my-env/assets/an-asset');
    expect(response.headers.get('Cache-Control')).toBe(
      'public, max-age=31536000, immutable',
    );
  });

  test('applies a Cache-Control header for all other requests to cache for a specific number of seconds', async () => {
    const response = await getResponseForUrl(
      'https://my-env/any-other-request',
      '25',
    );
    expect(response.headers.get('Cache-Control')).toBe(
      'public, max-age=0, s-maxage=25, stale-while-revalidate=30',
    );
  });

  test('applies a Cache-Control header for all other requests to cache for 30 seconds (a default value)', async () => {
    const response = await getResponseForUrl(
      'https://my-env/any-other-request',
    );
    expect(response.headers.get('Cache-Control')).toBe(
      'public, max-age=0, s-maxage=30, stale-while-revalidate=30',
    );
  });

  test('applies a Cache-Control header for all other requests to cache for 30 seconds if the default max age variable is invalid', async () => {
    const response = await getResponseForUrl(
      'https://my-env/any-other-request',
      'invalid',
    );
    expect(response.headers.get('Cache-Control')).toBe(
      'public, max-age=0, s-maxage=30, stale-while-revalidate=30',
    );
  });

  async function getResponseForUrl(
    url: string,
    cacheMaxAgeSeconds: string | undefined = undefined,
  ) {
    if (cacheMaxAgeSeconds) {
      process.env.DEFAULT_CACHE_MAX_AGE_SECONDS = cacheMaxAgeSeconds;
    }
    try {
      const middlewareResult = await runMiddleware(cacheControlHeaders, url);
      return middlewareResult!;
    } finally {
      delete (process.env as Record<string, string | undefined>)
        .DEFAULT_CACHE_MAX_AGE_SECONDS;
    }
  }
});
