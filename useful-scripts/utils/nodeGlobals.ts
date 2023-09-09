import url from 'node:url';

/**
 * Get the path to the current module's directory.
 *
 * Equivalent to the `__dirname` global.
 */
export function getDirname(importMetaUrl: string) {
  return url.fileURLToPath(new URL('.', importMetaUrl));
}

/**
 * Get the path to the current module file.
 *
 * Equivalent to the `__filename` global.
 */
export function getFilename(importMetaUrl: string) {
  return url.fileURLToPath(new URL('', importMetaUrl));
}
