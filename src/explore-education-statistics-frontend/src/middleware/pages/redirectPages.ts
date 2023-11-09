import redirectService, {
  Redirects,
  RedirectType,
} from '@frontend/services/redirectService';
import type { NextRequest } from 'next/server';
import { NextResponse } from 'next/server';

interface CachedRedirects {
  redirects: Redirects;
  fetchedAt: number;
}

let cachedRedirects: CachedRedirects | undefined;

// The middleware only runs on paths defined in the config
// in middleware.ts, that will also need to be
// updated if any other paths are added here.
const redirectPaths = {
  methodologies: '/methodology',
  publications: '/find-statistics',
};

export default async function redirectPages(request: NextRequest) {
  // Cache the redirect paths for 10 seconds on local and dev,
  // 60 seconds on other envs.
  const cacheTime =
    process.env.APP_ENV === 'Local' || process.env.APP_ENV === 'Development'
      ? 10_000
      : 60_0000;

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
