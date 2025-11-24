import UkTimeHelper from '@common/utils/date/ukTimeHelper';

describe('UkTimeHelper', () => {
  test('should return correct UK start of day in UTC', () => {
    const isoDate = '2025-10-15T14:30:00Z';
    const result = UkTimeHelper.toUkStartOfDay(isoDate);

    expect(result).toBeInstanceOf(Date);
    expect(result.toISOString()).toBe('2025-10-14T23:00:00.000Z'); // UK midnight in UTC during BST
  });

  test('should return correct UK end of day in UTC', () => {
    const isoDate = '2025-10-15T14:30:00Z';
    const result = UkTimeHelper.toUkEndOfDay(isoDate);

    expect(result).toBeInstanceOf(Date);
    expect(result.toISOString()).toBe('2025-10-15T22:59:59.000Z'); // UK 23:59:59 in UTC during BST
  });

  test('should return correct UK start of day in UTC during DST', () => {
    const isoDate = '2025-01-15T14:30:00Z';
    const result = UkTimeHelper.toUkStartOfDay(isoDate);

    expect(result).toBeInstanceOf(Date);
    expect(result.toISOString()).toBe('2025-01-15T00:00:00.000Z'); // UK midnight in UTC during DST
  });

  test('should return correct UK end of day in UTC during DST', () => {
    const isoDate = '2025-01-15T14:30:00Z';
    const result = UkTimeHelper.toUkEndOfDay(isoDate);

    expect(result).toBeInstanceOf(Date);
    expect(result.toISOString()).toBe('2025-01-15T23:59:59.000Z'); // UK 23:59:59 in UTC during DST
  });

  test('should return today midnight in UK', () => {
    const result = UkTimeHelper.todayStartOfDayUk();

    expect(result).toMatch(/^\d{4}-\d{2}-\d{2}T00:00:00(Z|[+-]\d{2}:\d{2})$/);
  });

  test('should return midnight UK time for given date string', () => {
    const dateString = '2025-10-15T14:30:00Z';
    const result = UkTimeHelper.toUkStartOfDayString(dateString);

    expect(result).toMatch(/^2025-10-15T00:00:00[+-]\d{2}:\d{2}$/);
  });

  test('should return midnight UK time for given Date object', () => {
    const date = new Date('2025-10-15T14:30:00Z');
    const result = UkTimeHelper.toUkStartOfDayString(date);

    expect(result).toMatch(/^2025-10-15T00:00:00[+-]\d{2}:\d{2}$/);
  });

  test('should return last tick UK time for given date string', () => {
    const dateString = '2025-10-15T14:30:00Z';
    const result = UkTimeHelper.toUkEndOfDayString(dateString);

    expect(result).toMatch(/^2025-10-15T23:59:59[+-]\d{2}:\d{2}$/);
  });

  test('should return last tick UK time for given date object', () => {
    const date = new Date('2025-10-15T14:30:00Z');
    const result = UkTimeHelper.toUkEndOfDayString(date);

    expect(result).toMatch(/^2025-10-15T23:59:59[+-]\d{2}:\d{2}$/);
  });

  test('should handle winter time (GMT) correctly', () => {
    const winterDate = '2023-01-15T14:30:00Z';
    const result = UkTimeHelper.toUkStartOfDayString(winterDate);

    expect(result).toMatch(/^2023-01-15T00:00:00Z$/);
  });

  test('should handle summer time (BST) correctly', () => {
    const summerDate = '2025-10-15T14:30:00Z';
    const result = UkTimeHelper.toUkStartOfDayString(summerDate);

    expect(result).toMatch(/^2025-10-15T00:00:00\+01:00$/);
  });

  describe('UkTimeHelper.dateTimeInLondonTimeZone — BST → GMT edge cases', () => {
    test.each([
      // [inputDateString, expected]
      ['2025-10-26T01:00:00+01:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-26T00:00:00+00:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-26T04:00:00+04:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-25T20:00:00-04:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-26T01:00:00+00:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-26T02:00:00+01:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-26T05:00:00+04:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-25T21:00:00-04:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-26T02:00:00+00:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-26T03:00:00+01:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-26T06:00:00+04:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-25T22:00:00-04:00', '2025-10-26T00:00:00+01:00'],
      ['2025-10-27T01:00:00+00:00', '2025-10-27T00:00:00Z'],
      ['2025-10-27T02:00:00+01:00', '2025-10-27T00:00:00Z'],
      ['2025-10-27T05:00:00+04:00', '2025-10-27T00:00:00Z'],
      ['2025-10-26T21:00:00-04:00', '2025-10-27T00:00:00Z'],
    ])('converts %s correctly (start of day)', (input, expected) => {
      const date = new Date(input);
      const result = UkTimeHelper.dateTimeInLondonTimeZone(date, true);
      expect(result).toBe(expected);
    });

    test.each([
      // [inputDateString, expected]
      ['2025-10-26T01:00:00+01:00', '2025-10-26T23:59:59Z'],
      ['2025-10-26T00:00:00+00:00', '2025-10-26T23:59:59Z'],
      ['2025-10-26T04:00:00+04:00', '2025-10-26T23:59:59Z'],
      ['2025-10-25T20:00:00-04:00', '2025-10-26T23:59:59Z'],
      ['2025-10-26T01:00:00+00:00', '2025-10-26T23:59:59Z'],
      ['2025-10-26T02:00:00+01:00', '2025-10-26T23:59:59Z'],
      ['2025-10-26T05:00:00+04:00', '2025-10-26T23:59:59Z'],
      ['2025-10-25T21:00:00-04:00', '2025-10-26T23:59:59Z'],
      ['2025-10-26T02:00:00+00:00', '2025-10-26T23:59:59Z'],
      ['2025-10-26T03:00:00+01:00', '2025-10-26T23:59:59Z'],
      ['2025-10-26T06:00:00+04:00', '2025-10-26T23:59:59Z'],
      ['2025-10-25T22:00:00-04:00', '2025-10-26T23:59:59Z'],
      ['2025-10-27T01:00:00+00:00', '2025-10-27T23:59:59Z'],
      ['2025-10-27T02:00:00+01:00', '2025-10-27T23:59:59Z'],
      ['2025-10-27T05:00:00+04:00', '2025-10-27T23:59:59Z'],
      ['2025-10-26T21:00:00-04:00', '2025-10-27T23:59:59Z'],
    ])('converts %s correctly (end of day)', (input, expected) => {
      const date = new Date(input);
      const result = UkTimeHelper.dateTimeInLondonTimeZone(date, false);
      expect(result).toBe(expected);
    });
  });

  describe('UkTimeHelper.dateTimeInLondonTimeZone — GMT → BST edge cases', () => {
    test.each([
      // [inputDateString, expected]
      ['2025-03-30T00:00:00+00:00', '2025-03-30T00:00:00Z'],
      ['2025-03-30T01:00:00+01:00', '2025-03-30T00:00:00Z'],
      ['2025-03-30T04:00:00+04:00', '2025-03-30T00:00:00Z'],
      ['2025-03-29T20:00:00-04:00', '2025-03-30T00:00:00Z'],
      ['2025-03-30T02:00:00+01:00', '2025-03-30T00:00:00Z'],
      ['2025-03-30T01:00:00+00:00', '2025-03-30T00:00:00Z'],
      ['2025-03-30T05:00:00+04:00', '2025-03-30T00:00:00Z'],
      ['2025-03-29T21:00:00-04:00', '2025-03-30T00:00:00Z'],
      ['2025-03-30T03:00:00+01:00', '2025-03-30T00:00:00Z'],
      ['2025-03-30T02:00:00+00:00', '2025-03-30T00:00:00Z'],
      ['2025-03-30T06:00:00+04:00', '2025-03-30T00:00:00Z'],
      ['2025-03-29T22:00:00-04:00', '2025-03-30T00:00:00Z'],
      ['2025-03-31T02:00:00+01:00', '2025-03-31T00:00:00+01:00'],
      ['2025-03-31T01:00:00+00:00', '2025-03-31T00:00:00+01:00'],
      ['2025-03-31T05:00:00+04:00', '2025-03-31T00:00:00+01:00'],
      ['2025-03-30T21:00:00-04:00', '2025-03-31T00:00:00+01:00'],
    ])('converts %s correctly (start of day)', (input, expected) => {
      const date = new Date(input);
      const result = UkTimeHelper.dateTimeInLondonTimeZone(date, true);
      expect(result).toBe(expected);
    });

    test.each([
      // [inputDateString, expected]
      ['2025-03-30T00:00:00+00:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-30T01:00:00+01:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-30T04:00:00+04:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-29T20:00:00-04:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-30T02:00:00+01:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-30T01:00:00+00:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-30T05:00:00+04:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-29T21:00:00-04:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-30T03:00:00+01:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-30T02:00:00+00:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-30T06:00:00+04:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-29T22:00:00-04:00', '2025-03-30T23:59:59+01:00'],
      ['2025-03-31T02:00:00+01:00', '2025-03-31T23:59:59+01:00'],
      ['2025-03-31T01:00:00+00:00', '2025-03-31T23:59:59+01:00'],
      ['2025-03-31T05:00:00+04:00', '2025-03-31T23:59:59+01:00'],
      ['2025-03-30T21:00:00-04:00', '2025-03-31T23:59:59+01:00'],
    ])('converts %s correctly (end of day)', (input, expected) => {
      const date = new Date(input);
      const result = UkTimeHelper.dateTimeInLondonTimeZone(date, false);
      expect(result).toBe(expected);
    });
  });
});
