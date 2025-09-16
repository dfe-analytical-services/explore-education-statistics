import { internalHosts } from '@common/utils/url/allowedHosts';

/**
 * Formats urls in content links
 * - converts internal urls to lower case, excluding query params and anchors.
 * - trims and encodes urls
 */
export default function formatContentLinkUrl(urlString: string) {
  try {
    const url = new URL(urlString);

    if (internalHosts.includes(url.host.toLowerCase())) {
      url.pathname = url.pathname.toLowerCase();
    }
    return url.href;
  } catch {
    return urlString;
  }
}
