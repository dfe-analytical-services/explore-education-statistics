import prefixNoun from '@common/utils/string/prefixNoun';

const alphabet = 'abcdefghijklmnopqrstuvwxyz'.split('');
const vowels = ['a', 'e', 'i', 'o', 'u'];
const consonants = alphabet.filter(letter => !vowels.includes(letter));

describe('prefixNoun', () => {
  consonants.forEach(consonant => {
    test(`returns "a" when the noun begins with consonant ${consonant} or ${consonant.toUpperCase()}`, () => {
      expect(prefixNoun(`${consonant}word`)).toBe('a');
      expect(prefixNoun(`${consonant.toUpperCase()}word`)).toBe('a');
    });
  });

  vowels.forEach(vowel => {
    test(`returns "an" when the noun begins with vowel ${vowel} or ${vowel.toUpperCase()}`, () => {
      expect(prefixNoun(`${vowel}word`)).toBe('an');
      expect(prefixNoun(`${vowel.toUpperCase()}word`)).toBe('an');
    });
  });
});
