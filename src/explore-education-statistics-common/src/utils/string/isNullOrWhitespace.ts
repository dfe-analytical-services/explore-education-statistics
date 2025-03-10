/**
 * Check if a {@param str} is null or whitespace.
 */
export default function isNullOrWhitespace(str?: string) {
  return !str || !str.trim();
}
