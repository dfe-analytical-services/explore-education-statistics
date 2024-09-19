export interface Redirects {
  methodologies: Redirect[];
  publications: Redirect[];
}

export type RedirectType = keyof Redirects;

interface Redirect {
  fromSlug: string;
  toSlug: string;
}

export const redirectPathStarts = {
  methodologies: '/methodology',
  publications: '/find-statistics',
};

const prodPublicUrl = 'https://explore-education-statistics.service.gov.uk/';

export default function applyRedirectRules(
  url: string,
  redirects: Redirects,
): string {
  let parsedUrl: URL = new URL(url, prodPublicUrl);

  // noTrailingSlashUnlessOnlyHost
  if (parsedUrl.pathname === '/') {
    return url;
  }
  parsedUrl.pathname = parsedUrl.pathname.replace(/\/+$/, '');

  // pathMustNotEndInSlash1000
  parsedUrl.pathname = parsedUrl.pathname.replace(/(\/1000)+$/, '');

  // publicSiteHostMustNotStartWithWww
  if (
    parsedUrl.hostname.includes(
      'www.explore-education-statistics.service.gov.uk',
    )
  ) {
    parsedUrl.hostname = parsedUrl.hostname.replace('www.', '');
  }

  // mustNotTriggerAContentApiRedirect
  parsedUrl = applyContentApiRedirects(parsedUrl, redirects);

  // If original url was relative, return a relative url also
  if (isAbsoluteUrl(url)) {
    return parsedUrl.href;
  }
  return `${parsedUrl.pathname}${parsedUrl.search}`;
}

const applyContentApiRedirects = (
  parsedUrl: URL,
  redirects: Redirects,
): URL => {
  const redirectPathStart = Object.entries(redirectPathStarts).find(
    ([_, path]) => parsedUrl.pathname.startsWith(path),
  );
  const pathSegments = parsedUrl.pathname.split('/');

  if (redirectPathStart && pathSegments.length > 1) {
    const [key] = redirectPathStart;
    const redirectType = key as RedirectType;

    const rewriteRule = redirects[redirectType].find(
      ({ fromSlug }) => pathSegments[2].toLowerCase() === fromSlug,
    );

    if (rewriteRule) {
      const path = pathSegments
        .map(segment =>
          segment.toLowerCase() === rewriteRule?.fromSlug
            ? rewriteRule?.toSlug
            : segment.toLowerCase(),
        )
        .join('/');

      return new URL(`${path}${parsedUrl.search}`, prodPublicUrl);
    }
  }

  return parsedUrl;
};

const isAbsoluteUrl = (url: string) =>
  url.startsWith('http') ||
  url.startsWith('www') ||
  url.includes('explore-education-statistics.service.gov.uk');
