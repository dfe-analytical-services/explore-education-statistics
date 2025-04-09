/**
 * Check if a {@param str} is null or whitespace.
 */
export default function isUndefinedOrWhitespace(str?: string) {
  return !str?.trim();
}
