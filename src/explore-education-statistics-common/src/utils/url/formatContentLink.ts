import allowedHosts from '@common/utils/url/allowedHosts';

// Format links in content to ensure they are valid.
// Make sure any internal links are lower case, excluding query params.
export default function formatContentLink(urlString: string) {
  try {
    const url = new URL(urlString);

    if (allowedHosts.includes(url.host.toLowerCase())) {
      url.pathname = url.pathname.toLowerCase();
    }
    return url.href;
  } catch {
    return urlString;
  }
}
