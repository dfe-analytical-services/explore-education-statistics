import getExternality, { Externality } from './getExternality';

// Format links in content to ensure they are valid.
// Make sure any internal links are lower case, excluding query params.
export default function formatContentLink(
  urlString: string,
  externality?: Externality,
): string {
  try {
    const url = new URL(urlString);
    if (
      ['internal', 'external-admin'].includes(
        externality ?? getExternality(urlString),
      )
    ) {
      url.pathname = url.pathname.toLowerCase();
    }
    return url.href;
  } catch {
    return urlString;
  }
}
