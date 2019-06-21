import decimalPlaces from '../decimalPlaces';

describe('decimalPlaces', () => {
  test('returns decimal places if they exist', () => {
    expect(decimalPlaces(15.1002)).toBe(4);
  });

  test('returns 0 if there are no decimal places', () => {
    expect(decimalPlaces(15)).toBe(0);
  });
});
