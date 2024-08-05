import { getHostUrl } from '@common/utils/url/hostUrl';

export type UrlOrigin = 'public' | 'admin' | 'external' | 'external-trusted';

const PUBLIC_HOSTNAME = 'explore-education-statistics.service.gov.uk';

export default function getUrlOrigin(url: string): UrlOrigin {
  const hostUrl = getHostUrl();

  // TODO: replace with if (!URL.canParse(url)) if/when we can use this in common
  if ((url.startsWith('/') || url.startsWith('?')) && !url.includes('://')) {
    // Assume internal if it's not a parseable URL
    // i.e. it's a hash, query, absolute or relative path

    return hostUrl.origin.includes('admin') ? 'admin' : 'public';
  }

  const { host } = new URL(url);

  if (host.startsWith('admin.') && host.endsWith(PUBLIC_HOSTNAME)) {
    return 'admin';
  }

  if (host.endsWith(PUBLIC_HOSTNAME)) {
    return 'public';
  }

  if (host.endsWith('gov.uk')) {
    return 'external-trusted';
  }

  return 'external';
}
