/**
 * Check if a {@param str} is null or whitespace.
 */

const vowels = ['a', 'e', 'i', 'o', 'u'];

export default function prefixNoun(noun: string) {
  return vowels.includes(noun.toLowerCase().slice(0, 1)) ? 'an' : 'a';
}
