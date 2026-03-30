/**
 * Choose between prefixing a noun with "a" or "an".
 */

const vowels = ['a', 'e', 'i', 'o', 'u'];

export default function prefixNoun(noun: string) {
  return vowels.includes(noun.toLowerCase().slice(0, 1)) ? 'an' : 'a';
}
