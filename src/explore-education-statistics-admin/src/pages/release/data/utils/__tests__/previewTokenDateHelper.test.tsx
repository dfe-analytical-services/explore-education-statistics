import { isToday } from 'date-fns';
import PreviewTokenDateHelper from '@admin/pages/release/data/utils/previewTokenDateHelper';

jest.mock('date-fns');
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

    test('should default to 24-hour range when dates are not provided', () => {
      const result = helper.setDateRangeBasedOnCustomDates(null, null);

      expect(mockIsToday).not.toHaveBeenCalled();
      expect(result.endDate.getTime() - result.startDate.getTime()).toBeCloseTo(
        24 * 60 * 60 * 1000,
        -3,
      );
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
  });

  describe('setDateRangeBasedOnPresets', () => {
    test('should create date range for valid preset span', () => {
      const activatesDate = new Date();
      const expiresDate = new Date(activatesDate);
      const datePresetSpan = 3;
      expiresDate.setDate(activatesDate.getDate() + datePresetSpan);
      const timeThreshold = 100; // 100ms

      const result = helper.setDateRangeBasedOnPresets(datePresetSpan);

      const difference = Math.abs(
        result.startDate.getTime() - activatesDate.getTime(),
      );
      expect(difference).toBeLessThanOrEqual(timeThreshold);

      expiresDate.setHours(23, 59, 59, 0);
      expect(result.endDate.toISOString()).toBe(expiresDate.toISOString());
    });

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
