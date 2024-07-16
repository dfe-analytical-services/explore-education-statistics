import getUrlOrigin, { UrlOrigin } from './getUrlOrigin';

const eesOrigins: UrlOrigin[] = ['public', 'admin'];

// Format links in content to ensure they are valid.
// Make sure any internal links are lower case, excluding query params.
export default function formatContentLink(urlString: string): string {
  let parsedUrl: URL;
  try {
    parsedUrl = new URL(urlString);
  } catch {
    return urlString;
  }

  if (eesOrigins.includes(getUrlOrigin(urlString))) {
    parsedUrl.pathname = parsedUrl.pathname.toLowerCase();
  }

  return parsedUrl.href;
}
