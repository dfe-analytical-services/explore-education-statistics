import chunkLabelToFitCharLimit from '@common/modules/charts/components/utils/chunkLabelToFitCharLimit';

describe('chunkLabelToFitCharLimit', () => {
  test('splits a long string into an array of smaller strings', () => {
    expect(
      chunkLabelToFitCharLimit('Label with long string to test splitting'),
    ).toStrictEqual(['Label with long', 'string to test', 'splitting']);
  });

  test('splits a long string into an array of smaller strings with different char limit', () => {
    expect(
      chunkLabelToFitCharLimit('Label with long string to test splitting', 30),
    ).toStrictEqual(['Label with long string to test', 'splitting']);
  });

  test(`doesn't break words londer than char limit`, () => {
    expect(
      chunkLabelToFitCharLimit(
        'Sutton-under-Whitestonecliffe (North Yorkshire) - antidisestablishmentarianism',
        30,
      ),
    ).toStrictEqual([
      'Sutton-under-Whitestonecliffe',
      '(North Yorkshire) -',
      'antidisestablishmentarianism',
    ]);
  });

  test('returns array with empty string if empty string provided', () => {
    expect(chunkLabelToFitCharLimit('')).toStrictEqual(['']);
  });
});
