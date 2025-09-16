/**
 * Return a {@param str} with a trailing slash.
 */
export default function ensureTrailingSlash(str: string) {
  return `${str}${str.endsWith('/') ? '' : '/'}`;
}
