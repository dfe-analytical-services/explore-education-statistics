import getExternality, { Externality } from './getExternality';

// Format links in content to ensure they are valid.
// Make sure any internal links are lower case, excluding query params.
export default function formatContentLink(
  url: string | URL,
  externality?: Externality,
) {
  try {
    const formattedUrl = new URL(url);

    if (
      ['internal', 'external-admin'].includes(
        externality ?? getExternality(url),
      )
    ) {
      formattedUrl.pathname = formattedUrl.pathname.toLowerCase();
    }

    return formattedUrl.href;
  } catch {
    return url.toString();
  }
}
