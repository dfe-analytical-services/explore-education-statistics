import UkTimeHelper from '@common/utils/date/ukTimeHelper';

describe('UkTimeHelper', () => {
  test('should return correct UK start of day in UTC', () => {
    const isoDate = '2025-10-15T14:30:00Z';
    const result = UkTimeHelper.ukStartOfDayUtc(isoDate);

    expect(result).toBeInstanceOf(Date);
    expect(result.toISOString()).toBe('2025-10-14T23:00:00.000Z'); // UK midnight in UTC during BST
  });

  test('should return correct UK end of day in UTC', () => {
    const isoDate = '2025-10-15T14:30:00Z';
    const result = UkTimeHelper.ukEndOfDayUtc(isoDate);

    expect(result).toBeInstanceOf(Date);
    expect(result.toISOString()).toBe('2025-10-15T22:59:59.000Z'); // UK 23:59:59 in UTC during BST
  });

  test('should return correct UK start of day in UTC during DST', () => {
    const isoDate = '2025-01-15T14:30:00Z';
    const result = UkTimeHelper.ukStartOfDayUtc(isoDate);

    expect(result).toBeInstanceOf(Date);
    expect(result.toISOString()).toBe('2025-01-15T00:00:00.000Z'); // UK midnight in UTC during DST
  });

  test('should return correct UK end of day in UTC during DST', () => {
    const isoDate = '2025-01-15T14:30:00Z';
    const result = UkTimeHelper.ukEndOfDayUtc(isoDate);

    expect(result).toBeInstanceOf(Date);
    expect(result.toISOString()).toBe('2025-01-15T23:59:59.000Z'); // UK 23:59:59 in UTC during DST
  });

  test('should return today midnight in UK', () => {
    const result = UkTimeHelper.todayMidnightUk();

    expect(result).toMatch(/^\d{4}-\d{2}-\d{2}T00:00:00(Z|[+-]\d{2}:\d{2})$/);
  });

  test('should return midnight UK time for given date string', () => {
    const dateString = '2025-10-15T14:30:00Z';
    const result = UkTimeHelper.dateMidnightUk(dateString);

    expect(result).toMatch(/^2025-10-15T00:00:00[+-]\d{2}:\d{2}$/);
  });

  test('should return midnight UK time for given Date object', () => {
    const date = new Date('2025-10-15T14:30:00Z');
    const result = UkTimeHelper.dateMidnightUk(date);

    expect(result).toMatch(/^2025-10-15T00:00:00[+-]\d{2}:\d{2}$/);
  });

  test('should return last tick UK time for given date string', () => {
    const dateString = '2025-10-15T14:30:00Z';
    const result = UkTimeHelper.dateLastSecondUk(dateString);

    expect(result).toMatch(/^2025-10-15T23:59:59[+-]\d{2}:\d{2}$/);
  });

  test('should return last tick UK time for given date object', () => {
    const date = new Date('2025-10-15T14:30:00Z');
    const result = UkTimeHelper.dateLastSecondUk(date);

    expect(result).toMatch(/^2025-10-15T23:59:59[+-]\d{2}:\d{2}$/);
  });

  test('should handle winter time (GMT) correctly', () => {
    const winterDate = '2023-01-15T14:30:00Z';
    const result = UkTimeHelper.dateMidnightUk(winterDate);

    expect(result).toMatch(/^2023-01-15T00:00:00Z$/);
  });

  test('should handle summer time (BST) correctly', () => {
    const summerDate = '2025-10-15T14:30:00Z';
    const result = UkTimeHelper.dateMidnightUk(summerDate);

    expect(result).toMatch(/^2025-10-15T00:00:00\+01:00$/);
  });
});
