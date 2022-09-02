import formatPretty from '../formatPretty';

describe('formatPretty', () => {
  test('returns formatted string from integer', () => {
    expect(formatPretty(150000000)).toBe('150,000,000');
    expect(formatPretty(150000000, '', 1)).toBe('150,000,000.0');
    expect(formatPretty(150000000, '', 2)).toBe('150,000,000.00');
  });

  test('returns formatted string rounded down from float', () => {
    expect(formatPretty(150000000.1)).toBe('150,000,000.1');
    expect(formatPretty(150000000.101)).toBe('150,000,000.10');
    expect(formatPretty(150000000.12)).toBe('150,000,000.12');
    expect(formatPretty(150000000.123)).toBe('150,000,000.12');
    expect(formatPretty(150000000.1234)).toBe('150,000,000.12');

    expect(formatPretty(150000000.1234, '', 2)).toBe('150,000,000.12');
    expect(formatPretty(150000000.1234, '', 3)).toBe('150,000,000.123');
    expect(formatPretty(150000000.1234, '', 4)).toBe('150,000,000.1234');
    expect(formatPretty(150000000.12344, '', 4)).toBe('150,000,000.1234');
    expect(formatPretty(150000000.004, '', 2)).toBe('150,000,000.00');
  });

  test('returns formatted string rounded up from float', () => {
    expect(formatPretty(150000000.2)).toBe('150,000,000.2');
    expect(formatPretty(150000000.25)).toBe('150,000,000.25');
    expect(formatPretty(150000000.256)).toBe('150,000,000.26');
    expect(formatPretty(150000000.2567)).toBe('150,000,000.26');

    expect(formatPretty(150000000.2567, '', 2)).toBe('150,000,000.26');
    expect(formatPretty(150000000.2567, '', 3)).toBe('150,000,000.257');
    expect(formatPretty(150000000.2567, '', 4)).toBe('150,000,000.2567');
    expect(formatPretty(150000000.25678, '', 4)).toBe('150,000,000.2568');
  });

  test('returns formatted string from string containing integer', () => {
    expect(formatPretty('150000000')).toBe('150,000,000');
    expect(formatPretty('150000000', '', 1)).toBe('150,000,000.0');
    expect(formatPretty('150000000', '', 2)).toBe('150,000,000.00');
  });

  test('returns formatted string rounded down from string containing float', () => {
    expect(formatPretty('150000000.1234', '', 2)).toBe('150,000,000.12');
    expect(formatPretty('150000000.1234', '', 3)).toBe('150,000,000.123');
    expect(formatPretty('150000000.1234', '', 4)).toBe('150,000,000.1234');
    expect(formatPretty('150000000.12344', '', 4)).toBe('150,000,000.1234');
    expect(formatPretty('150000000.004', '', 2)).toBe('150,000,000.00');
  });

  test('returns formatted string rounded up from string containing float', () => {
    expect(formatPretty('150000000.2567', '', 2)).toBe('150,000,000.26');
    expect(formatPretty('150000000.2567', '', 3)).toBe('150,000,000.257');
    expect(formatPretty('150000000.2567', '', 4)).toBe('150,000,000.2567');
    expect(formatPretty('150000000.25678', '', 4)).toBe('150,000,000.2568');
  });

  test('returns formatted string from string containing trailing zeroes', () => {
    expect(formatPretty('150000000.0', '')).toBe('150,000,000.0');
    expect(formatPretty('150000000.00', '')).toBe('150,000,000.00');
    expect(formatPretty('150000000.000', '')).toBe('150,000,000.00');

    expect(formatPretty('150000000.0', '', 2)).toBe('150,000,000.00');
    expect(formatPretty('150000000.00', '', 2)).toBe('150,000,000.00');
    expect(formatPretty('150000000.000', '', 2)).toBe('150,000,000.00');
    expect(formatPretty('150000000.0000', '', 2)).toBe('150,000,000.00');

    expect(formatPretty('150000000.10', '', 2)).toBe('150,000,000.10');
    expect(formatPretty('150000000.100', '', 2)).toBe('150,000,000.10');
    expect(formatPretty('150000000.1000', '', 2)).toBe('150,000,000.10');

    expect(formatPretty('150000000.11', '', 2)).toBe('150,000,000.11');
    expect(formatPretty('150000000.110', '', 2)).toBe('150,000,000.11');
    expect(formatPretty('150000000.1100', '', 2)).toBe('150,000,000.11');
  });

  test('returns formatted string with correctly formatted unit', () => {
    expect(formatPretty(150000000.1234, '£', 2)).toBe('£150,000,000.12');
    expect(formatPretty(15.1234, '£m', 2)).toBe('£15.12m');
    expect(formatPretty(15.1234, '%', 2)).toBe('15.12%');
    expect(formatPretty(150000000.1234, '', 4)).toBe('150,000,000.1234');
  });

  test('returns NaN string if number value is not a number', () => {
    expect(formatPretty(Number.NaN)).toBe('NaN');
  });

  test('returns infinity string if number is infinite', () => {
    expect(formatPretty(1 / 0)).toBe('∞');
    expect(formatPretty(Number.NEGATIVE_INFINITY)).toBe('-∞');
    expect(formatPretty(Number.POSITIVE_INFINITY)).toBe('∞');
  });

  test('returns string value as-is if it is not a number', () => {
    expect(formatPretty('not a number')).toBe('not a number');
    expect(formatPretty('n/a')).toBe('n/a');
    expect(formatPretty('')).toBe('');
    expect(formatPretty(' ')).toBe(' ');
  });

  describe('Handling units', () => {
    test('returns the unit after the value', () => {
      expect(formatPretty('42', '%')).toBe('42%');
    });

    test('returns negative values with the unit', () => {
      expect(formatPretty('-42', '%')).toBe('-42%');
    });

    test('returns £ before the value for £ units', () => {
      expect(formatPretty('42', '£')).toBe('£42');
    });

    test('returns the correct format for negative numbers with £ units', () => {
      expect(formatPretty('-42', '£')).toBe('-£42');
    });

    test('returns £ before the value for £m units', () => {
      expect(formatPretty('42', '£m')).toBe('£42m');
    });

    test('returns the correct format for negative numbers with £m units', () => {
      expect(formatPretty('-42', '£m')).toBe('-£42m');
    });
  });
});
