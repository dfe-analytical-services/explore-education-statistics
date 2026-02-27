import 'urlpattern-polyfill';
import { Dictionary } from '@common/types';
import redirectService, { Redirects } from '@frontend/services/redirectService';
import type { NextFetchEvent, NextMiddleware, NextRequest } from 'next/server';
import { NextResponse } from 'next/server';

interface CachedRedirects {
  redirects: Redirects;
  fetchedAt: number;
}

const methodologySlugKey = 'methodologySlug';
const publicationSlugKey = 'publicationSlug';
const releaseSlugKey = 'releaseSlug';
const cacheTime = getCacheTime();

let cachedRedirects: CachedRedirects | undefined;

const redirectPatterns: URLPattern[] = [
  new URLPattern({
    pathname: `/methodology/:${methodologySlugKey}{/}?`,
  }),
  new URLPattern({
    pathname: `/find-statistics/:${publicationSlugKey}/(releases/?|.{0})?`,
  }),
  new URLPattern({
    pathname: `/find-statistics/:${publicationSlugKey}/:${releaseSlugKey}/(explore/?|methodology/?|help/?|updates/?|.{0})?`,
  }),
  new URLPattern({
    pathname: `/data-tables/:${publicationSlugKey}{/}?`,
  }),
  new URLPattern({
    pathname: `/data-tables/:${publicationSlugKey}/:${releaseSlugKey}{/}?`,
  }),
];

export default async function redirectPages(
  request: NextRequest,
  event: NextFetchEvent,
  middleware: NextMiddleware,
) {
  const { nextUrl } = request;
  const decodedPathname = decodeURIComponent(nextUrl.pathname);
  const lowerCasedPathname = decodedPathname.toLowerCase();

  // Check for redirects for publication, release, and methodology pages
  const routeSlugsBySlugKey = findRouteSlugsBySlugKey(lowerCasedPathname);

  if (Object.keys(routeSlugsBySlugKey).length > 0) {
    await refreshCache();

    const redirectPath = determineRedirectPath(
      lowerCasedPathname,
      routeSlugsBySlugKey,
    );

    if (redirectPath !== lowerCasedPathname) {
      const redirectUrl = nextUrl.clone();
      redirectUrl.pathname = redirectPath;
      return NextResponse.redirect(redirectUrl, 301);
    }
  }

  // Redirect any URLs with uppercase characters to lowercase.
  if (decodedPathname !== lowerCasedPathname) {
    const url = nextUrl.clone();
    url.pathname = lowerCasedPathname;
    return NextResponse.redirect(url, 301);
  }

  return middleware(request, event);
}

function findRouteSlugsBySlugKey(
  lowerCasedPathname: string,
): Dictionary<string> {
  for (let i = 0; i < redirectPatterns.length; i += 1) {
    const redirectPattern = redirectPatterns[i];

    const urlPatternMatch = redirectPattern.exec({
      pathname: lowerCasedPathname,
    });

    if (urlPatternMatch) {
      return Object.fromEntries(
        Object.entries(urlPatternMatch.pathname.groups)
          .filter(
            ([slugKey, slug]) =>
              (slugKey === methodologySlugKey ||
                slugKey === publicationSlugKey ||
                slugKey === releaseSlugKey) &&
              slug,
          )
          .map(([slugKey, slug]) => [slugKey, slug!]),
      );
    }
  }

  return {};
}

async function refreshCache() {
  const shouldRefetch =
    !cachedRedirects || cachedRedirects.fetchedAt + cacheTime < Date.now();

  if (shouldRefetch) {
    cachedRedirects = {
      redirects: await redirectService.list(),
      fetchedAt: Date.now(),
    };
  }
}

function determineRedirectPath(
  originalPath: string,
  routeSlugsBySlugKey: Dictionary<string>,
): string {
  let redirectPath = originalPath;

  Object.entries(routeSlugsBySlugKey).forEach(([slugKey, slug]) => {
    const redirect = findRedirectIfExists(slugKey, slug, routeSlugsBySlugKey);

    if (redirect) {
      redirectPath = redirectPath.replace(slug, redirect.toSlug);
    }
  });

  return redirectPath;
}

function findRedirectIfExists(
  slugKey: string,
  slug: string,
  routeSlugsBySlugKey: Dictionary<string>,
) {
  switch (slugKey) {
    case methodologySlugKey:
      return cachedRedirects?.redirects.methodologyRedirects?.find(
        ({ fromSlug }) => slug === fromSlug,
      );
    case publicationSlugKey:
      return cachedRedirects?.redirects.publicationRedirects?.find(
        ({ fromSlug }) => slug === fromSlug,
      );
    case releaseSlugKey: {
      let latestPublicationSlug = routeSlugsBySlugKey[publicationSlugKey];

      const publicationRedirect =
        cachedRedirects?.redirects.publicationRedirects?.find(
          ({ fromSlug }) => latestPublicationSlug === fromSlug,
        );

      if (publicationRedirect) {
        latestPublicationSlug = publicationRedirect.toSlug;
      }

      const {
        [latestPublicationSlug]: releaseRedirectsForLatestPublication = [],
      } = cachedRedirects?.redirects.releaseRedirectsByPublicationSlug || {};

      return releaseRedirectsForLatestPublication.find(
        ({ fromSlug }) => slug === fromSlug,
      );
    }
    default:
      throw new Error(`'${slugKey}' is not a valid redirect type.`);
  }
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
