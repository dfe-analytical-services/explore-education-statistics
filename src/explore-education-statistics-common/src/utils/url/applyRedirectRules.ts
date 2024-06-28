import redirectService, {
  RedirectType,
  redirectPathStarts,
} from '@common/services/redirectService';

const prodPublicUrl = 'https://explore-education-statistics.service.gov.uk/';

export default async function applyRedirectRules(url: string): Promise<string> {
  let newUrl: string = url;

  newUrl = await noTrailingSlashUnlessOnlyHost(newUrl);
  newUrl = await pathMustNotEndInSlash1000(newUrl);
  newUrl = await publicSiteHostMustNotStartWithWww(newUrl);
  newUrl = await mustNotTriggerAContentApiRedirect(newUrl);

  return newUrl;
}

type RedirectRule =
  | ((url: string) => string)
  | ((url: string) => Promise<string>);

const noTrailingSlashUnlessOnlyHost: RedirectRule = (url: string) => {
  const parsedUrl: URL = new URL(url, prodPublicUrl);
  if (parsedUrl.pathname === '/') {
    return url;
  }

  return url.replace(/\/+$/, '');
};

const pathMustNotEndInSlash1000: RedirectRule = (url: string) => {
  const newUrl = url.replace(/(\/1000)+$/, '');
  return newUrl === '' ? '/' : newUrl;
};

const publicSiteHostMustNotStartWithWww: RedirectRule = (url: string) => {
  const parsedUrl: URL = new URL(url, prodPublicUrl);
  if (
    parsedUrl.hostname.includes(
      'www.explore-education-statistics.service.gov.uk',
    )
  ) {
    parsedUrl.hostname = parsedUrl.hostname.replace('www.', '');
    return parsedUrl.href;
  }

  return url;
};

const mustNotTriggerAContentApiRedirect: RedirectRule = async (url: string) => {
  const parsedUrl: URL = new URL(url, prodPublicUrl);
  // Remove leading slash to match the redirectPathStarts used by the redirectPages middleware
  const parsedPath = parsedUrl.pathname;

  const redirectsFromContentApi = await redirectService.list();

  if (
    Object.values(redirectPathStarts).find(path =>
      parsedPath.startsWith(path),
    ) &&
    parsedPath.split('/').length > 1
  ) {
    const redirectPath = Object.keys(redirectPathStarts).reduce((acc, key) => {
      const redirectType = key as RedirectType;
      // If starts with "methodologies/" or "publications/"
      if (parsedPath.startsWith(redirectPathStarts[redirectType])) {
        const pathSegments = parsedPath.split('/');

        const rewriteRule = redirectsFromContentApi[redirectType]?.find(
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
      return new URL(`${redirectPath}${parsedUrl.search}`, prodPublicUrl).href;
    }
  }

  return url;
};
