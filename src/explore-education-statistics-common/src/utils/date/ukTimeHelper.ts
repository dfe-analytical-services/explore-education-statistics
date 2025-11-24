import { fromZonedTime, formatInTimeZone } from 'date-fns-tz';

export default class UkTimeHelper {
  public static readonly europeLondonTimeZoneId = 'Europe/London';

  public static todayStartOfDayUk(): Date {
    return this.toUkStartOfDay(new Date());
  }

  public static todayEndOfDayUk(): Date {
    return this.toUkEndOfDay(new Date());
  }

  public static toUkStartOfDay(date1: Date | string): Date {
    return new Date(this.toUkStartOfDayString(date1));
  }

  public static toUkEndOfDay(date1: Date | string): Date {
    return new Date(this.toUkEndOfDayString(date1));
  }

  public static toUkStartOfDayString(date1: Date | string): string {
    return this.dateTimeInLondonTimeZone(date1, true);
  }

  public static toUkEndOfDayString(date1: Date | string): string {
    return this.dateTimeInLondonTimeZone(date1, false);
  }

  /**
   * Returns a string like 'YYYY-MM-DDTHH:mm:ss±HH:MM' representing
   * the requested London wall time (midnight or last second) on the
   * same London calendar day as `dateTime`.
   *
   * Safe across DST because we construct the *London wall time* and
   * convert it to a real UTC instant via fromZonedTime.
   */
  public static dateTimeInLondonTimeZone(
    dateTime: Date | string,
    startOfDayRatherThanEndOfDay: boolean,
  ): string {
    const ymdLondon = formatInTimeZone(
      dateTime,
      this.europeLondonTimeZoneId,
      'yyyy-MM-dd',
    );

    const time = startOfDayRatherThanEndOfDay ? 'T00:00:00' : 'T23:59:59.000';

    const utcInstant = fromZonedTime(
      `${ymdLondon}${time}`,
      this.europeLondonTimeZoneId,
    );

    return formatInTimeZone(
      utcInstant,
      this.europeLondonTimeZoneId,
      "yyyy-MM-dd'T'HH:mm:ssXXX",
    );
  }
}
