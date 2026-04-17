import {
  type NextFetchEvent,
  type NextMiddleware,
  NextRequest,
} from 'next/server';

export default async function cacheControlHeaders(
  req: NextRequest,
  event: NextFetchEvent,
  middleware: NextMiddleware,
) {
  const res = await middleware(req, event);

  if (!res) {
    return null;
  }

  const { pathname } = req.nextUrl;

  // Cache assets retrieved via Next API routes for a moderate duration.
  if (pathname.startsWith('/api/assets/')) {
    res.headers.set('Cache-Control', 'public, max-age=86400');
    return res;
  }

  // Do not cache general Next API route responses.
  if (pathname.startsWith('/api/')) {
    res.headers.set('Cache-Control', 'no-store, no-cache, must-revalidate');
    return res;
  }

  // Cache meta files for a moderate amount of time.
  if (pathname === '/favicon.svg' || pathname === '/manifest.json') {
    res.headers.set('Cache-Control', 'public, max-age=3600');
    return res;
  }

  // Cache immutabe static assets indefinitely.
  if (pathname.startsWith('/assets/')) {
    res.headers.set('Cache-Control', 'public, max-age=31536000, immutable');
    return res;
  }

  // For all other requests, responses are cached for a small period of time.
  // Note that in development mode, Next will overwrite these headers
  const generalResponseCacheControlValue = `public, max-age=0, s-maxage=${getDefaultMaxAgeSeconds()}, stale-while-revalidate=30`;

  res.headers.set('Cache-Control', generalResponseCacheControlValue);
  return res;
}

function getDefaultMaxAgeSeconds() {
  const maxAge = Number(process.env.DEFAULT_CACHE_MAX_AGE_SECONDS);
  return maxAge > 0 ? maxAge : 30;
}
