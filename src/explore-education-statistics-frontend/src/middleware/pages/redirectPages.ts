import redirectService, {
  redirectPathStarts,
  Redirects,
  RedirectType,
} from '@common/services/redirectService';
import type { NextRequest } from 'next/server';
import { NextResponse } from 'next/server';

interface CachedRedirects {
  redirects: Redirects;
  fetchedAt: number;
}

const cacheTime = getCacheTime();

let cachedRedirects: CachedRedirects | undefined;

export default async function redirectPages(request: NextRequest) {
  const { nextUrl } = request;
  const decodedPathname = decodeURIComponent(request.nextUrl.pathname);

  // Check for redirects for release and methodology pages
  if (
    Object.values(redirectPathStarts).find(path =>
      decodedPathname.toLowerCase().startsWith(path),
    ) &&
    decodedPathname.split('/').length > 2
  ) {
    const shouldRefetch =
      !cachedRedirects || cachedRedirects.fetchedAt + cacheTime < Date.now();

    if (shouldRefetch) {
      cachedRedirects = {
        redirects: await redirectService.list(),
        fetchedAt: Date.now(),
      };
    }

    const redirectPath = Object.keys(redirectPathStarts).reduce((acc, key) => {
      const redirectType = key as RedirectType;
      if (
        decodedPathname
          .toLowerCase()
          .startsWith(redirectPathStarts[redirectType])
      ) {
        const pathSegments = decodedPathname.split('/');

        const rewriteRule = cachedRedirects?.redirects[redirectType]?.find(
          ({ fromSlug }) => pathSegments[2].toLowerCase() === fromSlug,
        );

        if (rewriteRule) {
          return pathSegments
            .map(segment =>
              segment.toLowerCase() === rewriteRule?.fromSlug
                ? rewriteRule?.toSlug
                : segment.toLowerCase(),
            )
            .join('/');
        }
      }
      return acc;
    }, '');

    if (redirectPath) {
      const redirectUrl = nextUrl.clone();
      redirectUrl.pathname = redirectPath;
      return NextResponse.redirect(redirectUrl, 301);
    }
  }

  // Redirect any URLs with uppercase characters to lowercase.
  if (decodedPathname !== decodedPathname.toLowerCase()) {
    const url = nextUrl.clone();
    url.pathname = decodedPathname.toLowerCase();
    return NextResponse.redirect(url, 301);
  }

  return NextResponse.next();
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
