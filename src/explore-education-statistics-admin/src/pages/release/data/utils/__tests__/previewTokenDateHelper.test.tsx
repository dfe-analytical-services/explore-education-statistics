import { isToday } from 'date-fns';
import { zonedTimeToUtc } from 'date-fns-tz';
import PreviewTokenDateHelper from '@admin/pages/release/data/utils/previewTokenDateHelper';

jest.mock('@admin/services/utils/service');
jest.mock('date-fns-tz');
jest.mock('date-fns');

describe('PreviewTokenDateHelper', () => {
  let helper: PreviewTokenDateHelper;
  const mockIsToday = isToday as jest.MockedFunction<typeof isToday>;
  const mockZonedTimeToUtc = zonedTimeToUtc as jest.MockedFunction<
    typeof zonedTimeToUtc
  >;

  beforeEach(() => {
    jest.clearAllMocks();
    helper = new PreviewTokenDateHelper();
  });

  describe('setDateRangeBasedOnCustomDates', () => {
    test('should use provided dates when both activates and expires are given and activates is not today', () => {
      const activatesDate = new Date('2025-01-15T10:00:00');
      const expiresDate = new Date('2025-01-20T15:30:00');
      const mockStartUtc = new Date('2025-01-15T00:00:00Z');
      const mockEndUtc = new Date('2025-01-20T23:59:59Z');

      mockIsToday.mockReturnValue(false);
      mockZonedTimeToUtc
        .mockReturnValueOnce(mockStartUtc)
        .mockReturnValueOnce(mockEndUtc);

      const result = helper.setDateRangeBasedOnCustomDates(
        activatesDate,
        expiresDate,
      );

      expect(mockIsToday).toHaveBeenCalledWith(activatesDate);
      expect(mockZonedTimeToUtc).toHaveBeenCalledWith(
        '2025-01-15T00:00:00',
        'Europe/London',
      );
      expect(mockZonedTimeToUtc).toHaveBeenCalledWith(
        '2025-01-20T23:59:59',
        'Europe/London',
      );
      expect(result).toEqual({ startDate: mockStartUtc, endDate: mockEndUtc });
    });

    test('should use current time as start date when activates is today', () => {
      mockIsToday.mockReturnValue(true);
      const activatesDate = new Date('2025-01-15T10:00:00');
      const expiresDate = new Date('2025-01-20T23:59:59');

      mockZonedTimeToUtc
        .mockReturnValueOnce(activatesDate)
        .mockReturnValueOnce(expiresDate);
      helper = new PreviewTokenDateHelper();
      const result = helper.setDateRangeBasedOnCustomDates(
        activatesDate,
        expiresDate,
      );

      expect(mockIsToday).toHaveBeenCalledWith(activatesDate);
      expect(mockZonedTimeToUtc).toHaveBeenNthCalledWith(
        1,
        expect.stringMatching(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$/),
        'Europe/London',
      ); // Current time
      expect(mockZonedTimeToUtc).toHaveBeenNthCalledWith(
        2,
        '2025-01-20T23:59:59',
        'Europe/London',
      );
      expect(result).toEqual({
        startDate: activatesDate,
        endDate: expiresDate,
      });
    });

    test('should default to 24-hour range when dates are not provided', () => {
      const currentDate = new Date('2025-01-10T10:59:59');

      mockZonedTimeToUtc
        .mockReturnValueOnce(currentDate)
        .mockReturnValueOnce(currentDate);

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
      const mockStartUtc = new Date('2025-01-15T14:30:45Z');
      const mockEndUtc = new Date('2025-01-18T23:59:59Z');

      mockZonedTimeToUtc
        .mockReturnValueOnce(mockStartUtc)
        .mockReturnValueOnce(mockEndUtc)
        .mockReturnValueOnce(mockStartUtc);

      const result = helper.setDateRangeBasedOnPresets(3);

      expect(mockZonedTimeToUtc).toHaveBeenCalledTimes(3);
      expect(mockZonedTimeToUtc).toHaveBeenNthCalledWith(
        1,
        expect.stringMatching(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$/),
        'Europe/London',
      );
      expect(mockZonedTimeToUtc).toHaveBeenNthCalledWith(
        2,
        expect.stringMatching(/^\d{4}-\d{2}-\d{2}T23:59:59$/),
        'Europe/London',
      );
      expect(result).toEqual({ startDate: mockStartUtc, endDate: mockEndUtc });
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
