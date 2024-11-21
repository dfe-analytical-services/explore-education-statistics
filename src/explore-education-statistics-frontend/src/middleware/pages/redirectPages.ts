import redirectService, {
  Redirects,
  RedirectType,
} from '@frontend/services/redirectService';
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

interface MatchedRedirectSlug {
  redirectType: RedirectType;
  slug: string;
}

const methodologySlugKey = 'methodologySlug';
const publicationSlugKey = 'publicationSlug';
const releaseSlugKey = 'releaseSlug';
const cacheTime = getCacheTime();

let cachedRedirects: CachedRedirects | undefined;

// POSSIBLE REDIRECT ROUTES FOR PUBLICATIONS AND RELEASES:
// find-statistics/{publication-slug}
// find-statistics/{publication-slug}/data-guidance
// find-statistics/{publication-slug}/prerelease-access-list
// find-statistics/{publication-slug}/{release-slug}
// find-statistics/{publication-slug}/{release-slug}/data-guidance
// find-statistics/{publication-slug}/{release-slug}/prerelease-access-list
// data-tables/{publication-slug}
// data-tables/{publication-slug}/{release-slug}
// data-tables/{publication-slug}/fast-track/{data-block-parent-id}
// data-tables/{publication-slug}/permalink/{permalink}
const redirectPatterns: RedirectPattern[] = [
  {
    redirectTypes: ['methodologies'],
    urlPattern: new URLPattern({
      pathname: `/methodology/:${methodologySlugKey}{/*}?`,
    }),
  },
  {
    redirectTypes: ['publications'],
    urlPattern: new URLPattern({
      pathname: `/find-statistics/:${publicationSlugKey}{/(data-guidance|prerelease-access-list)}?`,
    }),
  },
  {
    redirectTypes: ['publications', 'releases'],
    urlPattern: new URLPattern({
      pathname: `/find-statistics/:${publicationSlugKey}/:${releaseSlugKey}{/(data-guidance|prerelease-access-list)}?`,
    }),
  },
  {
    redirectTypes: ['publications'],
    urlPattern: new URLPattern({
      pathname: `/data-tables/:${publicationSlugKey}{/(\\(fast-track|permalink\\)\\(/.*\\))}?`,
    }),
  },
  {
    redirectTypes: ['publications', 'releases'],
    urlPattern: new URLPattern({
      pathname: `/data-tables/:${publicationSlugKey}/:${releaseSlugKey}`,
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
  const matchedRedirectSlugs = findMatchedRedirectSlugs(lowerCasedPathname);

  if (matchedRedirectSlugs.length > 0) {
    await refreshCache();

    const redirectPath = determineRedirectPath(
      lowerCasedPathname,
      matchedRedirectSlugs,
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

function findMatchedRedirectSlugs(
  lowerCasedPathname: string,
): MatchedRedirectSlug[] {
  for (let i = 0; i < redirectPatterns.length; i += 1) {
    const redirectPattern = redirectPatterns[i];

    const urlPatternMatch = redirectPattern.urlPattern.exec({
      pathname: lowerCasedPathname,
    });

    if (urlPatternMatch) {
      return redirectPattern.redirectTypes.map(
        (redirectType): MatchedRedirectSlug => {
          switch (redirectType) {
            case 'methodologies':
              return {
                redirectType,
                slug: urlPatternMatch.pathname.groups[methodologySlugKey]!,
              };
            case 'publications':
              return {
                redirectType,
                slug: urlPatternMatch.pathname.groups[publicationSlugKey]!,
              };
            case 'releases':
              return {
                redirectType,
                slug: urlPatternMatch.pathname.groups[releaseSlugKey]!,
              };
            default:
              throw new Error(
                `'${redirectType}' is not a valid redirect type.`,
              );
          }
        },
      );
    }
  }

  return [];
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
  matchedRedirectSlugs: MatchedRedirectSlug[],
): string {
  let redirectPath = originalPath;

  matchedRedirectSlugs.forEach(matchedRedirectSlug => {
    const redirect = cachedRedirects?.redirects[
      matchedRedirectSlug.redirectType
    ]?.find(({ fromSlug }) => matchedRedirectSlug.slug === fromSlug);

    if (redirect) {
      redirectPath = redirectPath.replace(
        matchedRedirectSlug.slug,
        redirect.toSlug,
      );
    }
  });

  return redirectPath;
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
