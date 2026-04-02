import { fromZonedTime, formatInTimeZone, toZonedTime } from 'date-fns-tz';
import { addDays, format } from 'date-fns';

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

    const startDateYmdLondon = formatInTimeZone(startDate, TZ, 'yyyy-MM-dd');

    const startDateMidnightUtc = fromZonedTime(
      `${startDateYmdLondon}T00:00:00`,
      TZ,
    );

    const endDate = UkTimeHelper.addDaysInTimeZoneEndOfDay(
      TZ,
      startDateMidnightUtc,
      daysToAdd,
    );

    return { startDate, endDate };
  }

  public static addDaysInTimeZoneEndOfDay(
    TZ: string,
    date: Date,
    days: number,
  ): Date {
    // 1) Convert the UTC instant to the local time of the target TZ first
    const localDate = toZonedTime(date, TZ);

    // 2) Add the calendar days to the LOCAL date
    const localDatePlusN = addDays(localDate, days);

    // 3) Get the 'yyyy-MM-dd' string of that local shifted date
    const plusNYmd = format(localDatePlusN, 'yyyy-MM-dd');

    // 4) Convert that local end-of-day back to a UTC instant
    return fromZonedTime(`${plusNYmd}T23:59:59`, TZ);
  }
}
