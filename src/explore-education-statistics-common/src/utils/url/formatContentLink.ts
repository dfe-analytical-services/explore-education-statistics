import getExternality, { Externality } from './getExternality';

const eesOrigins: Externality[] = ['internal', 'external-admin'];

// Format links in content to ensure they are valid.
// Make sure any internal links are lower case, excluding query params.
export default function formatContentLink(urlString: string): string {
  try {
    const url = new URL(urlString);
    if (eesOrigins.includes(getExternality(urlString))) {
      url.pathname = url.pathname.toLowerCase();
    }
    return url.href;
  } catch {
    return urlString;
  }
}
