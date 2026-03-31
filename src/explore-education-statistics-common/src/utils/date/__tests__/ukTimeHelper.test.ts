import UkTimeHelper from '@common/utils/date/ukTimeHelper';

describe('UkTimeHelper', () => {
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

  describe('UkTimeHelper.toUkStartOfDay', () => {
    // Test case for Winter (GMT - Offset 0)
    test('should return midnight UTC during Winter (GMT)', () => {
      const winterDate = new Date('2026-01-15T12:00:00Z');
      const result = UkTimeHelper.toUkStartOfDay(winterDate);

      // In Winter, 00:00 UK is 00:00 UTC
      expect(result.toISOString()).toMatch(/^2026-01-15T00:00:00.000Z$/);
    });

    // Test case for Summer (BST - Offset +1)
    test('should return 23:00 UTC of previous day during Summer (BST)', () => {
      const summerDate = new Date('2026-06-15T12:00:00Z');
      const result = UkTimeHelper.toUkStartOfDay(summerDate);

      // In Summer, 00:00 UK is 23:00 UTC of the previous day
      expect(result.toISOString()).toMatch(/^2026-06-14T23:00:00.000Z$/);
    });
  });

  describe('UkTimeHelper.toUkEndOfDay', () => {
    // Test case for Winter (GMT - Offset 0)
    test('should return 23:59:59 during Winter (GMT)', () => {
      const winterDate = new Date('2026-01-15T12:00:00Z');
      const result = UkTimeHelper.toUkEndOfDay(winterDate);

      // In Winter, 23:59 UK is 23:59 UTC
      expect(result.toISOString()).toMatch(/^2026-01-15T23:59:59.000Z$/);
    });

    // Test case for Summer (BST - Offset +1)
    test('should return 22:59:59 UTC of previous day during Summer (BST)', () => {
      const summerDate = new Date('2026-06-15T12:00:00Z');
      const result = UkTimeHelper.toUkEndOfDay(summerDate);

      // In Summer, 23:59 UK is 22:59 UTC
      expect(result.toISOString()).toMatch(/^2026-06-15T22:59:59.000Z$/);
    });
  });

  describe('UkTimeHelper.getDateRangeFromDate', () => {
    test('should not allow adding 0 days', () => {
      const startDate = new Date('2025-06-15T10:30:00Z');
      expect(() => UkTimeHelper.getDateRangeFromDate(0, startDate)).toThrow(
        'Please specify a number greater than 0 to create a date range.',
      );
    });

    test('should correctly add days during BST period', () => {
      const startDate = new Date('2025-06-15T14:30:00Z'); // BST period
      const result = UkTimeHelper.getDateRangeFromDate(3, startDate);

      expect(result.startDate).toBe(startDate);
      expect(result.endDate.toISOString()).toBe('2025-06-18T22:59:59.000Z'); // 3 days later, end of day in BST
    });

    test('should correctly add days during GMT period', () => {
      const startDate = new Date('2025-01-15T14:30:00Z'); // GMT period
      const result = UkTimeHelper.getDateRangeFromDate(5, startDate);

      expect(result.startDate).toBe(startDate);
      expect(result.endDate.toISOString()).toBe('2025-01-20T23:59:59.000Z'); // 5 days later, end of day in GMT
    });

    test('should handle transition from GMT to BST', () => {
      const startDate = new Date('2025-03-29T12:00:00Z'); // Day before BST starts
      const result = UkTimeHelper.getDateRangeFromDate(2, startDate);

      expect(result.startDate).toBe(startDate);
      expect(result.endDate.toISOString()).toBe('2025-03-31T22:59:59.000Z'); // End of day in BST after transition
    });

    test('should handle transition from BST to GMT', () => {
      const startDate = new Date('2025-10-25T12:00:00Z'); // Day before GMT starts
      const result = UkTimeHelper.getDateRangeFromDate(2, startDate);

      expect(result.startDate).toBe(startDate);
      expect(result.endDate.toISOString()).toBe('2025-10-27T23:59:59.000Z'); // End of day in GMT after transition
    });

    test('should throw error when supplied with negative days (past dates)', () => {
      const startDate = new Date('2025-06-15T14:30:00Z');
      expect(() => UkTimeHelper.getDateRangeFromDate(-3, startDate)).toThrow(
        'Cannot add negative days to a date.',
      );
    });

    test('should handle edge case of adding many days', () => {
      const startDate = new Date('2025-01-01T00:00:00Z');
      const result = UkTimeHelper.getDateRangeFromDate(365, startDate);

      expect(result.startDate).toBe(startDate);
      expect(result.endDate.toISOString()).toBe('2026-01-01T23:59:59.000Z'); // One year later
    });

    test('should handle start date at different times of day consistently', () => {
      const morning = new Date('2025-06-15T08:00:00Z');
      const evening = new Date('2025-06-15T20:00:00Z');

      const morningResult = UkTimeHelper.getDateRangeFromDate(1, morning);
      const eveningResult = UkTimeHelper.getDateRangeFromDate(1, evening);

      // Both should end at the same time regardless of start time within the day
      expect(morningResult.endDate.toISOString()).toBe(
        eveningResult.endDate.toISOString(),
      );
    });
  });

  describe('UkTimeHelper start and end day — BST → GMT edge cases', () => {
    test.each([
      // [inputDateString, expected]
      ['2025-10-26T01:00:00+01:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-26T00:00:00+00:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-26T04:00:00+04:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-25T20:00:00-04:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-26T01:00:00+00:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-26T02:00:00+01:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-26T05:00:00+04:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-25T21:00:00-04:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-26T02:00:00+00:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-26T03:00:00+01:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-26T06:00:00+04:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-25T22:00:00-04:00', '2025-10-25T23:00:00.000Z'],
      ['2025-10-27T01:00:00+00:00', '2025-10-27T00:00:00.000Z'],
      ['2025-10-27T02:00:00+01:00', '2025-10-27T00:00:00.000Z'],
      ['2025-10-27T05:00:00+04:00', '2025-10-27T00:00:00.000Z'],
      ['2025-10-26T21:00:00-04:00', '2025-10-27T00:00:00.000Z'],
    ])('converts %s correctly (start of day)', (input, expected) => {
      const date = new Date(input);
      const result = UkTimeHelper.toUkStartOfDay(date).toISOString();
      expect(result).toBe(expected);
    });

    test.each([
      // [inputDateString, expected]
      ['2025-10-26T01:00:00+01:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-26T00:00:00+00:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-26T04:00:00+04:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-25T20:00:00-04:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-26T01:00:00+00:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-26T02:00:00+01:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-26T05:00:00+04:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-25T21:00:00-04:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-26T02:00:00+00:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-26T03:00:00+01:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-26T06:00:00+04:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-25T22:00:00-04:00', '2025-10-26T23:59:59.000Z'],
      ['2025-10-27T01:00:00+00:00', '2025-10-27T23:59:59.000Z'],
      ['2025-10-27T02:00:00+01:00', '2025-10-27T23:59:59.000Z'],
      ['2025-10-27T05:00:00+04:00', '2025-10-27T23:59:59.000Z'],
      ['2025-10-26T21:00:00-04:00', '2025-10-27T23:59:59.000Z'],
    ])('converts %s correctly (end of day)', (input, expected) => {
      const date = new Date(input);
      const result = UkTimeHelper.toUkEndOfDay(date).toISOString();
      expect(result).toBe(expected);
    });
  });

  describe('UkTimeHelper — GMT → BST edge cases', () => {
    test.each([
      // [inputDateString, expected]
      ['2025-03-30T00:00:00+00:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-30T01:00:00+01:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-30T04:00:00+04:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-29T20:00:00-04:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-30T02:00:00+01:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-30T01:00:00+00:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-30T05:00:00+04:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-29T21:00:00-04:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-30T03:00:00+01:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-30T02:00:00+00:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-30T06:00:00+04:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-29T22:00:00-04:00', '2025-03-30T00:00:00.000Z'],
      ['2025-03-31T02:00:00+01:00', '2025-03-30T23:00:00.000Z'],
      ['2025-03-31T01:00:00+00:00', '2025-03-30T23:00:00.000Z'],
      ['2025-03-31T05:00:00+04:00', '2025-03-30T23:00:00.000Z'],
      ['2025-03-30T21:00:00-04:00', '2025-03-30T23:00:00.000Z'],
    ])('converts %s correctly (start of day)', (input, expected) => {
      const date = new Date(input);
      const result = UkTimeHelper.toUkStartOfDay(date).toISOString();
      expect(result).toBe(expected);
    });

    test.each([
      // [inputDateString, expected]
      ['2025-03-30T00:00:00+00:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-30T01:00:00+01:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-30T04:00:00+04:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-29T20:00:00-04:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-30T02:00:00+01:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-30T01:00:00+00:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-30T05:00:00+04:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-29T21:00:00-04:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-30T03:00:00+01:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-30T02:00:00+00:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-30T06:00:00+04:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-29T22:00:00-04:00', '2025-03-30T22:59:59.000Z'],
      ['2025-03-31T02:00:00+01:00', '2025-03-31T22:59:59.000Z'],
      ['2025-03-31T01:00:00+00:00', '2025-03-31T22:59:59.000Z'],
      ['2025-03-31T05:00:00+04:00', '2025-03-31T22:59:59.000Z'],
      ['2025-03-30T21:00:00-04:00', '2025-03-31T22:59:59.000Z'],
    ])('converts %s correctly (end of day)', (input, expected) => {
      const date = new Date(input);
      const result = UkTimeHelper.toUkEndOfDay(date).toISOString();
      expect(result).toBe(expected);
    });
  });
});
