import formatPretty from '../formatPretty';

describe('formatPretty', () => {
  test('returns formatted string from integer', () => {
    expect(formatPretty(150000000)).toBe('150,000,000');
  });

  test('returns formatted string rounded down from float', () => {
    expect(formatPretty(150000000.1234)).toBe('150,000,000.123');
    expect(formatPretty(150000000.1234, 2)).toBe('150,000,000.12');
    expect(formatPretty(150000000.1234, 4)).toBe('150,000,000.1234');
    expect(formatPretty(150000000.12344, 4)).toBe('150,000,000.1234');
  });

  test('returns formatted string rounded up from float', () => {
    expect(formatPretty(150000000.2567)).toBe('150,000,000.257');
    expect(formatPretty(150000000.2567, 2)).toBe('150,000,000.26');
    expect(formatPretty(150000000.2567, 4)).toBe('150,000,000.2567');
    expect(formatPretty(150000000.25678, 4)).toBe('150,000,000.2568');
  });

  test('returns formatted string from string containing integer', () => {
    expect(formatPretty('150000000')).toBe('150,000,000');
  });

  test('returns formatted string rounded down from string containing float', () => {
    expect(formatPretty('150000000.1234')).toBe('150,000,000.123');
  });

  test('returns formatted string rounded up from string containing float', () => {
    expect(formatPretty('150000000.2567')).toBe('150,000,000.257');
    expect(formatPretty('150000000.2567', 2)).toBe('150,000,000.26');
    expect(formatPretty('150000000.2567', 4)).toBe('150,000,000.2567');
    expect(formatPretty('150000000.25678', 4)).toBe('150,000,000.2568');
  });

  test('returns formatted string from string containing trailing zeroes', () => {
    expect(formatPretty('150000000.0')).toBe('150,000,000.0');
    expect(formatPretty('150000000.00')).toBe('150,000,000.00');
    expect(formatPretty('150000000.000')).toBe('150,000,000.000');
    expect(formatPretty('150000000.0000')).toBe('150,000,000.000');

    expect(formatPretty('150000000.10')).toBe('150,000,000.10');
    expect(formatPretty('150000000.100')).toBe('150,000,000.100');
    expect(formatPretty('150000000.1000')).toBe('150,000,000.100');

    expect(formatPretty('150000000.11')).toBe('150,000,000.11');
    expect(formatPretty('150000000.110')).toBe('150,000,000.110');
    expect(formatPretty('150000000.1100')).toBe('150,000,000.110');
  });
});
