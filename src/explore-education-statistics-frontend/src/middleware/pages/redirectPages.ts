import { Dictionary } from '@common/types';
import redirectService, {
  Redirects,
  RedirectType,
} from '@frontend/services/redirectService';
import _ from 'lodash';
import type { NextFetchEvent, NextMiddleware, NextRequest } from 'next/server';
import { NextResponse } from 'next/server';

interface CachedRedirects {
  redirects: Redirects;
  fetchedAt: number;
}

interface RedirectPattern {
  redirectTypes: RedirectType[];
  urlPattern: URLPattern;
}

const methodologyRedirectType: RedirectType = 'methodologyRedirects';
const publicationRedirectType: RedirectType = 'publicationRedirects';
const releaseRedirectType: RedirectType = 'releaseRedirectsByPublicationSlug';
const methodologySlugKey = 'methodologySlug';
const publicationSlugKey = 'publicationSlug';
const releaseSlugKey = 'releaseSlug';
const cacheTime = getCacheTime();

let cachedRedirects: CachedRedirects | undefined;

const redirectPatterns: RedirectPattern[] = [
  {
    redirectTypes: [methodologyRedirectType],
    urlPattern: new URLPattern({
      pathname: `/methodology/:${methodologySlugKey}{/}?`,
    }),
  },
  {
    redirectTypes: [publicationRedirectType],
    urlPattern: new URLPattern({
      pathname: `/find-statistics/:${publicationSlugKey}/(data-guidance/?|prerelease-access-list/?|.{0})?`,
    }),
  },
  {
    redirectTypes: [publicationRedirectType, releaseRedirectType],
    urlPattern: new URLPattern({
      pathname: `/find-statistics/:${publicationSlugKey}/:${releaseSlugKey}/(data-guidance/?|prerelease-access-list/?|.{0})?`,
    }),
  },
  {
    redirectTypes: [publicationRedirectType],
    urlPattern: new URLPattern({
      pathname: `/data-tables/:${publicationSlugKey}/(fast-track/[^/]*/?|permalink/[^/]*/?|.{0})?`,
    }),
  },
  {
    redirectTypes: [publicationRedirectType, releaseRedirectType],
    urlPattern: new URLPattern({
      pathname: `/data-tables/:${publicationSlugKey}/:${releaseSlugKey}{/}?`,
    }),
  },
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
  const routeSlugsByRedirectType =
    findRouteSlugsByRedirectType(lowerCasedPathname);

  if (!_.isEmpty(routeSlugsByRedirectType)) {
    await refreshCache();

    const redirectPath = determineRedirectPath(
      lowerCasedPathname,
      routeSlugsByRedirectType,
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

function findRouteSlugsByRedirectType(
  lowerCasedPathname: string,
): Dictionary<string> {
  for (let i = 0; i < redirectPatterns.length; i += 1) {
    const redirectPattern = redirectPatterns[i];

    const urlPatternMatch = redirectPattern.urlPattern.exec({
      pathname: lowerCasedPathname,
    });

    if (urlPatternMatch) {
      return Object.fromEntries(
        redirectPattern.redirectTypes.map(redirectType => {
          switch (redirectType) {
            case methodologyRedirectType:
              return [
                redirectType,
                urlPatternMatch.pathname.groups[methodologySlugKey]!,
              ];
            case publicationRedirectType:
              return [
                redirectType,
                urlPatternMatch.pathname.groups[publicationSlugKey]!,
              ];
            case releaseRedirectType:
              return [
                redirectType,
                urlPatternMatch.pathname.groups[releaseSlugKey]!,
              ];
            default:
              throw new Error(
                `'${redirectType}' is not a valid redirect type.`,
              );
          }
        }),
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
  routeSlugsByRedirectType: Dictionary<string>,
): string {
  let redirectPath = originalPath;

  Object.entries(routeSlugsByRedirectType).forEach(([redirectType, slug]) => {
    const redirect = findRedirectIfExists(
      redirectType,
      slug,
      routeSlugsByRedirectType,
    );

    if (redirect) {
      redirectPath = redirectPath.replace(slug, redirect.toSlug);
    }
  });

  return redirectPath;
}

function findRedirectIfExists(
  redirectType: string,
  slug: string,
  routeSlugsByRedirectType: Dictionary<string>,
) {
  switch (redirectType) {
    case methodologyRedirectType:
      return cachedRedirects?.redirects.methodologyRedirects?.find(
        ({ fromSlug }) => slug === fromSlug,
      );
    case publicationRedirectType:
      return cachedRedirects?.redirects.publicationRedirects?.find(
        ({ fromSlug }) => slug === fromSlug,
      );
    case releaseRedirectType: {
      let latestPublicationSlug =
        routeSlugsByRedirectType[publicationRedirectType];

      const publicationRedirect =
        cachedRedirects?.redirects.publicationRedirects?.find(
          ({ fromSlug }) => latestPublicationSlug === fromSlug,
        );

      if (publicationRedirect) {
        latestPublicationSlug = publicationRedirect.toSlug;
      }

      return _.get(
        cachedRedirects?.redirects.releaseRedirectsByPublicationSlug,
        latestPublicationSlug,
      )?.find(({ fromSlug }) => slug === fromSlug);
    }
    default:
      throw new Error(`'${redirectType}' is not a valid redirect type.`);
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
