import { fromZonedTime, formatInTimeZone } from 'date-fns-tz';
import { addDays } from 'date-fns';

export type DateRange = { startDate: Date; endDate: Date };

export default class UkTimeHelper {
  public static readonly europeLondonTimeZoneId = 'Europe/London';

  public static toUkStartOfDay(dateTime: Date | string): Date {
    const ymdLondon = formatInTimeZone(
      dateTime,
      this.europeLondonTimeZoneId,
      'yyyy-MM-dd',
    );

    const time = 'T00:00:00.000';

    const utcInstant = fromZonedTime(
      `${ymdLondon}${time}`,
      this.europeLondonTimeZoneId,
    );

    return new Date(
      formatInTimeZone(
        utcInstant,
        this.europeLondonTimeZoneId,
        "yyyy-MM-dd'T'HH:mm:ssXXX",
      ),
    );
  }

  public static toUkEndOfDay(dateTime: Date | string): Date {
    const ymdLondon = formatInTimeZone(
      dateTime,
      this.europeLondonTimeZoneId,
      'yyyy-MM-dd',
    );

    const time = 'T23:59:59.000';

    const utcInstant = fromZonedTime(
      `${ymdLondon}${time}`,
      this.europeLondonTimeZoneId,
    );

    return new Date(
      formatInTimeZone(
        utcInstant,
        this.europeLondonTimeZoneId,
        "yyyy-MM-dd'T'HH:mm:ssXXX",
      ),
    );
  }

  /**
   * Calculates a date range from a given start date (defaults to today) to N days in the future.
   * All dates are handled in UK time zone to ensure consistent calendar date calculations.
   *
   * @param daysToAdd - The number of calendar days to add to the start date
   * @param startDate - Optional start date (defaults to current date/time)
   * @returns DateRange containing the start date and end date (at 23:59:59) in UK time
   */
  public static getDateRangeFromDate(
    daysToAdd: number,
    startDate: Date = new Date(),
  ): DateRange {
    if (daysToAdd < 0) {
      throw new Error('Cannot add negative days to a date.');
    } else if (daysToAdd === 0) {
      throw new Error(
        'Please specify a number greater than 0 to create a date range.',
      );
    }

    const TZ = UkTimeHelper.europeLondonTimeZoneId;

    // 1) "Today" in London as a calendar date (no time)
    const todayYmdLondon = formatInTimeZone(startDate, TZ, 'yyyy-MM-dd');

    // 2) Today at 00:00 London → UTC instant
    const todayMidnightUtc = fromZonedTime(`${todayYmdLondon}T00:00:00`, TZ);

    // 3) Add N *calendar* days from that midnight
    const plusNMidnightUtc = addDays(todayMidnightUtc, daysToAdd);

    // 4) What calendar day is that in London?
    const plusNYmdLondon = formatInTimeZone(plusNMidnightUtc, TZ, 'yyyy-MM-dd');

    // 5) End of that day in London → UTC instant
    const endDate = fromZonedTime(`${plusNYmdLondon}T23:59:59`, TZ);

    return { startDate, endDate };
  }
}
