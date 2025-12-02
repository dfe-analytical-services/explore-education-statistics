import { isToday } from 'date-fns';
import PreviewTokenDateHelper from '@admin/pages/release/data/utils/previewTokenDateHelper';
import UkTimeHelper from '@common/utils/date/ukTimeHelper';
import { formatInTimeZone } from 'date-fns-tz';
import mockDate from '@common-test/mockDate';

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
      expect(() =>
        helper.setDateRangeBasedOnCustomDates(aDate, undefined),
      ).toThrow(
        'Both activates and expires dates must be provided together, or both must be null/undefined',
      );
      expect(() =>
        helper.setDateRangeBasedOnCustomDates(undefined, aDate),
      ).toThrow(
        'Both activates and expires dates must be provided together, or both must be null/undefined',
      );
    });

    const TZ = UkTimeHelper.europeLondonTimeZoneId;

    const cases: [string, string][] = [
      // ——— Around BST start (last Sunday in March 2025: 2025-03-30) ———
      ['2025-03-28T23:59:00.000Z', '2025-03-29T23:59:59.000Z'],
      ['2025-03-29T23:59:00.000Z', '2025-03-30T22:59:59.000Z'],
      ['2025-03-30T00:30:00.000Z', '2025-03-31T22:59:59.000Z'],
      ['2025-03-31T10:00:00.000Z', '2025-04-01T22:59:59.000Z'],

      // ——— Around BST end (last Sunday in Oct 2025: 2025-10-26) ———
      ['2025-10-24T23:59:00.000Z', '2025-10-26T23:59:59.000Z'],
      ['2025-10-15T23:59:00.000Z', '2025-10-17T22:59:59.000Z'],
      ['2025-10-25T23:59:00.000Z', '2025-10-27T23:59:59.000Z'],
      ['2025-10-26T00:30:00.000Z', '2025-10-27T23:59:59.000Z'],
      ['2025-10-27T10:00:00.000Z', '2025-10-28T23:59:59.000Z'],

      // ——— Representative mid-year / winter cases ———
      ['2025-06-15T12:00:00.000Z', '2025-06-16T22:59:59.000Z'], // summer (BST)
      ['2025-01-15T12:00:00.000Z', '2025-01-16T23:59:59.000Z'], // winter (GMT)
      ['2025-06-15T00:00:00.000Z', '2025-06-16T22:59:59.000Z'], // 2025-06-15 01:00 BST local
      ['2025-06-15T23:59:59.000Z', '2025-06-17T22:59:59.000Z'], // local just before 2025-06-16 01:00 BST → tomorrow = 17th
    ];

    test.each(cases)(
      'fixed now = %s → end should be end-of-tomorrow (London)',
      (fixedDateIso, expectedEndIso) => {
        const fixedDate = new Date(fixedDateIso);
        mockDate.set(fixedDate);

        // Act
        const result = helper.setDateRangeBasedOnCustomDates();

        // Assert: start is "now" (tight tolerance to avoid flakiness on CI)
        expect(
          Math.abs(result.startDate.getTime() - fixedDate.getTime()),
        ).toBeLessThanOrEqual(2);

        // Assert: end equals the expected end-of-tomorrow instant (UTC)
        expect(result.endDate.toISOString()).toBe(expectedEndIso);

        // Assert: in London, the wall-clock time is 23:59:59
        const londonEndHms = formatInTimeZone(
          result.endDate,
          TZ,
          'HH:mm:ss.SSS',
        );
        expect(londonEndHms).toBe('23:59:59.000');
      },
    );

    test('endDate should be exactly at UK end of day', () => {
      const fixedDate = new Date('2023-01-01T12:00:00Z');
      mockDate.set(fixedDate);

      const result = helper.setDateRangeBasedOnPresets(1);

      expect(result.endDate).toEqual(new Date('2023-01-02T23:59:59.000Z'));
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
