import { fromZonedTime, formatInTimeZone } from 'date-fns-tz';

export default class UkTimeHelper {
  private static readonly TZ = 'Europe/London';

  public static todayStartOfDayUk(): string {
    return this.toUkStartOfDayString(new Date());
  }

  public static toUkStartOfDay(date1: Date | string): Date {
    return new Date(this.toUkStartOfDayString(date1));
  }

  public static toUkEndOfDay(date1: Date | string): Date {
    return new Date(this.toUkEndOfDayString(date1));
  }

  public static toUkStartOfDayString(date1: Date | string): string {
    return this.dateTimeInLondonTimeZone(date1, DayOption.STARTOFDAY);
  }

  public static toUkEndOfDayString(date1: Date | string): string {
    return this.dateTimeInLondonTimeZone(date1, DayOption.ENDOFDAY);
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
    option: DayOption,
  ): string {
    const ymdLondon = formatInTimeZone(dateTime, this.TZ, 'yyyy-MM-dd');

    const wall =
      option === DayOption.STARTOFDAY ? 'T00:00:00' : 'T23:59:59.999';

    const utcInstant = fromZonedTime(`${ymdLondon}${wall}`, this.TZ);

    return formatInTimeZone(utcInstant, this.TZ, "yyyy-MM-dd'T'HH:mm:ssXXX");
  }
}
enum DayOption {
  STARTOFDAY = 'StartOfDay',
  ENDOFDAY = 'EndOfDay',
}
