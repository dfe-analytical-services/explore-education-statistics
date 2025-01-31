/**
 * Generate a hashcode for a {@param string}.
 * This is just a rip of Java's String#hashCode.
 */
export default function hashCode(string: string): number {
  let hash = 0;

  for (let i = 0; i < string.length; i += 1) {
    // eslint-disable-next-line no-bitwise
    hash = string.charCodeAt(i) + ((hash << 5) - hash);
  }

  return hash;
}
