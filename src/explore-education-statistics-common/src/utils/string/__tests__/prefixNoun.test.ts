import prefixNoun from '@common/utils/string/prefixNoun';

const alphabet = 'abcdefghijklmnopqrstuvwxyz'.split('');
const vowels = ['a', 'e', 'i', 'o', 'u'];
const consonants = alphabet.filter(letter => !vowels.includes(letter));

describe('prefixNoun', () => {
  it.each(consonants)(
    `returns "a" when the noun begins with consonant %s`,
    consonant => {
      expect(prefixNoun(`${consonant}word`)).toBe('a');
    },
  );

  it.each(consonants)(
    `returns "a" when the noun begins with uppercase consonant %s`,
    consonant => {
      expect(prefixNoun(`${consonant.toUpperCase()}word`)).toBe('a');
    },
  );

  it.each(vowels)(`returns "an" when the noun begins with vowel %s`, vowel => {
    expect(prefixNoun(`${vowel}word`)).toBe('an');
  });

  it.each(vowels)(
    `returns "an" when the noun begins with uppercase vowel %s`,
    vowel => {
      expect(prefixNoun(`${vowel.toUpperCase()}word`)).toBe('an');
    },
  );
});
