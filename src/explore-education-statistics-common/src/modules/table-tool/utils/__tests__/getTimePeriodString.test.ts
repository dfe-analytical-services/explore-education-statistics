import getTimePeriodString from '../getTimePeriodString';

describe('getTimePeriodString', () => {
  test('returns the correct string when `from` and `to` are supplied', () => {
    expect(getTimePeriodString({ from: '2010', to: '2020' })).toBe(
      '2010 to 2020',
    );
  });

  test('returns the correct string when only `from` is supplied', () => {
    expect(getTimePeriodString({ from: '2010' })).toBe('2010');
  });

  test('returns the correct string when only `to` is supplied', () => {
    expect(getTimePeriodString({ to: '2020' })).toBe('2020');
  });

  test('returns undefined when neither `from` or `to` is supplied', () => {
    expect(getTimePeriodString({})).toBeUndefined();
  });
});
