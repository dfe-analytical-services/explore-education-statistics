import { NextRequest, NextResponse } from 'next/server';
import redirectService, {
  Redirects,
  RedirectType,
} from '@frontend/services/redirectService';

interface CachedRedirects {
  redirects: Redirects;
  fetchedAt: number;
}

// Cache the redirect paths for 2 seconds on Local,
// 10 seconds on Development, and 60 seconds in all other
// environments.
function getCacheTime(): number {
  switch (process.env.APP_ENV) {
    case 'Local':
      return 2_000;
    case 'Development':
      return 10_000;
    default:
      return 60_000;
  }
}

export default async function middleware(request: NextRequest) {
  // The middleware only runs on paths defined in the config
  // in middleware.ts, that will also need to be
  // updated if any other paths are added here.
  const redirectPaths = {
    methodologies: '/methodology',
    publications: '/find-statistics',
  };

  let cachedRedirects: CachedRedirects | undefined;
  const cacheTime = getCacheTime();

  const shouldRefetch =
    !cachedRedirects || cachedRedirects.fetchedAt + cacheTime < Date.now();

  if (shouldRefetch) {
    cachedRedirects = {
      redirects: await redirectService.list(),
      fetchedAt: Date.now(),
    };
  }

  const redirectUrl = Object.keys(redirectPaths).reduce((acc, key) => {
    const redirectType = key as RedirectType;
    if (request.nextUrl.pathname.startsWith(redirectPaths[redirectType])) {
      const pathSegments = request.nextUrl.pathname.split('/');

      const rewriteRule = cachedRedirects?.redirects[redirectType]?.find(
        ({ fromSlug }) => pathSegments[2] === fromSlug,
      );

      if (rewriteRule) {
        return pathSegments
          .map(segment =>
            segment === rewriteRule?.fromSlug ? rewriteRule?.toSlug : segment,
          )
          .join('/');
      }
    }
    return acc;
  }, '');

  if (redirectUrl) {
    return NextResponse.redirect(new URL(redirectUrl, request.url));
  }

  return NextResponse.next();
}
