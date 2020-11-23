import formatPretty from '../formatPretty';

describe('formatPretty', () => {
  test('returns formatted string from integer', () => {
    expect(formatPretty(150000000)).toBe('150,000,000.00');
    expect(formatPretty(150000000, '', 1)).toBe('150,000,000.0');
    expect(formatPretty(150000000, '', 2)).toBe('150,000,000.00');
  });

  test('returns formatted string rounded down from float', () => {
    expect(formatPretty(150000000.1234, '', 2)).toBe('150,000,000.12');
    expect(formatPretty(150000000.1234, '', 3)).toBe('150,000,000.123');
    expect(formatPretty(150000000.1234, '', 4)).toBe('150,000,000.1234');
    expect(formatPretty(150000000.12344, '', 4)).toBe('150,000,000.1234');
    expect(formatPretty(150000000.004, '', 2)).toBe('150,000,000.00');
  });

  test('returns formatted string rounded up from float', () => {
    expect(formatPretty(150000000.2567, '', 2)).toBe('150,000,000.26');
    expect(formatPretty(150000000.2567, '', 3)).toBe('150,000,000.257');
    expect(formatPretty(150000000.2567, '', 4)).toBe('150,000,000.2567');
    expect(formatPretty(150000000.25678, '', 4)).toBe('150,000,000.2568');
  });

  test('returns formatted string from string containing integer', () => {
    expect(formatPretty('150000000')).toBe('150,000,000.00');
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
});
