import { addDays, isToday } from 'date-fns';
import PreviewTokenDateHelper from '@admin/pages/release/data/utils/previewTokenDateHelper';
import UkTimeHelper from '@common/utils/date/ukTimeHelper';
import { fromZonedTime, formatInTimeZone } from 'date-fns-tz';

jest.mock('date-fns', () => {
  const actual = jest.requireActual('date-fns');
  return {
    ...actual,
    isToday: jest.fn(),
  };
});
describe('PreviewTokenDateHelper', () => {
  let helper: PreviewTokenDateHelper;
  const mockIsToday = isToday as jest.MockedFunction<typeof isToday>;

  beforeEach(() => {
    jest.clearAllMocks();
    helper = new PreviewTokenDateHelper();
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  describe('setDateRangeBasedOnCustomDates', () => {
    test('should use provided dates when both activates and expires are given and activates is not today', () => {
      const activatesDate = new Date('2025-01-15T10:00:00');
      const expiresDate = new Date('2025-01-20T15:30:00');
      const expectedStart = new Date('2025-01-15T00:00:00.000Z');
      const expectedEnd = new Date('2025-01-20T23:59:59.000Z');
      mockIsToday.mockReturnValue(false);
      const result = helper.setDateRangeBasedOnCustomDates(
        activatesDate,
        expiresDate,
      );

      expect(mockIsToday).toHaveBeenCalledWith(activatesDate);
      expect(result).toEqual({
        startDate: expectedStart,
        endDate: expectedEnd,
      });
    });

    test('should use current time as start date when activates is today', () => {
      const activatesDate = new Date();
      const expiresDate = new Date(activatesDate);
      expiresDate.setDate(activatesDate.getDate() + 1);
      const timeThreshold = 100; // 100ms

      mockIsToday.mockReturnValue(true);

      const result = helper.setDateRangeBasedOnCustomDates(
        activatesDate,
        expiresDate,
      );

      expect(mockIsToday).toHaveBeenCalledWith(activatesDate);
      const difference = Math.abs(
        result.startDate.getTime() - activatesDate.getTime(),
      );
      expect(difference).toBeLessThanOrEqual(timeThreshold);

      expiresDate.setHours(23, 59, 59, 0);
      expect(result.endDate).toStrictEqual(expiresDate);
    });

    test('should use midnight as start date when activates is not today', () => {
      const activatesDate = new Date('2025-01-15T10:00:00');
      const expiresDate = new Date('2025-01-20T23:59:59');
      mockIsToday.mockReturnValue(false);

      helper = new PreviewTokenDateHelper();
      const result = helper.setDateRangeBasedOnCustomDates(
        activatesDate,
        expiresDate,
      );

      expect(mockIsToday).toHaveBeenCalledWith(activatesDate);

      expect(result.startDate).toEqual(new Date('2025-01-15T00:00:00.000Z'));
      expect(result.endDate).toEqual(new Date('2025-01-20T23:59:59.000Z'));
    });

    test('should throw error if only activates or expires is provided', () => {
      const aDate = new Date('2025-01-15T10:00:00');
      expect(() => helper.setDateRangeBasedOnCustomDates(aDate, null)).toThrow(
        'Both activates and expires dates must be provided together, or both must be null/undefined',
      );
      expect(() => helper.setDateRangeBasedOnCustomDates(null, aDate)).toThrow(
        'Both activates and expires dates must be provided together, or both must be null/undefined',
      );
    });

    const TZ = 'Europe/London';

    const cases: string[] = [
      // ——— Around BST start (last Sunday in March 2025: 2025-03-30) ———
      '2025-03-28T23:59:00.000Z', // two days before
      '2025-03-29T23:59:00.000Z', // eve of BST start
      '2025-03-30T00:30:00.000Z', // BST start day (short day)
      '2025-03-31T10:00:00.000Z', // day after

      // ——— Around BST end (last Sunday in Oct 2025: 2025-10-26) ———
      '2025-10-24T23:59:00.000Z', // two days before
      '2025-10-15T23:59:00.000Z',
      '2025-10-25T23:59:00.000Z', // eve of BST end
      '2025-10-26T00:30:00.000Z', // BST end day (long day / repeated hour)
      '2025-10-27T10:00:00.000Z', // day after

      // ——— Representative mid-year / winter cases ———
      '2025-06-15T12:00:00.000Z', // summer (BST active)
      '2025-01-15T12:00:00.000Z', // winter (GMT)
      '2025-06-15T00:00:00.000Z', // exact midnight UTC (London 01:00 in BST)
      '2025-06-15T23:59:59.000Z', // one second to midnight UTC (London 00:59:59 next day in BST)
    ];

    test.each(cases)(
      'fixed now = %s → end should be end-of-tomorrow (London)',
      fixedDateIso => {
        const fixedDate = new Date(fixedDateIso);
        jest.setSystemTime(fixedDate);

        // Arrange expected end: end of *tomorrow* in London
        // 1) London calendar for "today"
        const todayYmdLondon = formatInTimeZone(fixedDate, TZ, 'yyyy-MM-dd');
        // 2) Build "today at 00:00 London" → get UTC instant, add 1 day in absolute time, then get "tomorrow" YMD in London
        const todayMidnightUtc = fromZonedTime(
          `${todayYmdLondon}T00:00:00`,
          TZ,
        );
        const plusOneDayUtc = addDays(todayMidnightUtc, 1);
        const tomorrowYmdLondon = formatInTimeZone(
          plusOneDayUtc,
          TZ,
          'yyyy-MM-dd',
        );
        // 3) End of tomorrow (London wall time) → exact UTC instant
        const expectedEndUtc = fromZonedTime(
          `${tomorrowYmdLondon}T23:59:59`,
          TZ,
        );

        // Act
        const result = helper.setDateRangeBasedOnCustomDates();

        // Assert: start is "now" (tight tolerance to avoid flakiness on CI)
        expect(
          Math.abs(result.startDate.getTime() - fixedDate.getTime()),
        ).toBeLessThanOrEqual(2);

        // Assert: end equals exact end-of-tomorrow instant
        expect(result.endDate.toISOString()).toBe(expectedEndUtc.toISOString());

        // Assert: in London, the wall-clock time is 23:59:59.999
        const londonEndHms = formatInTimeZone(
          result.endDate,
          TZ,
          'HH:mm:ss.SSS',
        );
        expect(londonEndHms).toBe('23:59:59.999');
      },
    );

    test.each([
      // [fixedDate, expectedHoursDifference]
      ['2025-01-12T23:59:00.000Z', 24],
      ['2025-10-12T23:59:00.000Z', 47],
      ['2025-01-27T23:59:00.000Z', 24],
      ['2025-10-01T23:59:00.000Z', 47],
      ['2025-10-25T10:00:00.000Z', 37],
      ['2025-03-30T10:00:00.000Z', 36],
      ['2025-06-15T12:00:00.000Z', 34],
      ['2025-01-15T12:00:00.000Z', 35],
      ['2025-06-15T17:30:00.000Z', 29],
      ['2025-01-15T17:30:00.000Z', 30],
      ['2025-06-15T15:30:00.000Z', 31],
      ['2025-01-15T15:30:00.000Z', 32],
      ['2025-10-15T08:30:00.000Z', 38],
      ['2025-01-15T08:30:00.000Z', 39],
    ])(
      'fixed `%s` endDate time should be 23:59:59 and about %s hours later than startDate',
      (fixedDateIso, expectedHoursDifference) => {
        const fixedDate = new Date(fixedDateIso);
        jest.setSystemTime(fixedDate);

        const result = helper.setDateRangeBasedOnCustomDates();

        const hoursDifference =
          (result.endDate.getTime() - result.startDate.getTime()) /
          (1000 * 60 * 60);

        expect(Math.floor(hoursDifference)).toBe(expectedHoursDifference);
      },
    );

    test('endDate should be exactly at UK end of day', () => {
      const fixedDate = new Date('2023-01-01T12:00:00.000Z');
      jest.setSystemTime(fixedDate);

      const result = helper.setDateRangeBasedOnPresets(1);

      const expectedEndDate = UkTimeHelper.toUkEndOfDayString('2023-01-02');
      expect(result.endDate).toEqual(expectedEndDate);
    });
  });

  describe('setDateRangeBasedOnPresets', () => {
    test.each([1, 2, 3, 4, 5, 6, 7])(
      'should create date range for valid preset span = %i',
      datePresetSpan => {
        const activatesDate = new Date();
        const expiresDate = new Date(activatesDate);
        expiresDate.setDate(activatesDate.getDate() + datePresetSpan);
        const timeThreshold = 100; // 100ms

        const result = helper.setDateRangeBasedOnPresets(datePresetSpan);

        // Start date should be approximately "now"
        const difference = Math.abs(
          result.startDate.getTime() - activatesDate.getTime(),
        );
        expect(difference).toBeLessThanOrEqual(timeThreshold);

        // End date should be "datePresetSpan" days later, set to 23:59:59
        expiresDate.setHours(23, 59, 59, 0);
        expect(result.endDate.toISOString()).toBe(expiresDate.toISOString());
      },
    );

    test('should throw error for invalid preset span - zero', () => {
      expect(() => helper.setDateRangeBasedOnPresets(0)).toThrow(
        'The number of days (0) selected is not allowed, please select between 1 to 7 days.',
      );
    });

    test('should throw error for invalid preset span - greater than 7', () => {
      expect(() => helper.setDateRangeBasedOnPresets(8)).toThrow(
        'The number of days (8) selected is not allowed, please select between 1 to 7 days.',
      );
    });

    test('should throw error for invalid preset span - negative number', () => {
      expect(() => helper.setDateRangeBasedOnPresets(-1)).toThrow(
        'The number of days (-1) selected is not allowed, please select between 1 to 7 days.',
      );
    });

    test('should throw error for invalid preset span - non-integer', () => {
      expect(() => helper.setDateRangeBasedOnPresets(3.5)).toThrow(
        'The number of days (3.5) selected is not allowed, please select between 1 to 7 days.',
      );
    });
  });
});
