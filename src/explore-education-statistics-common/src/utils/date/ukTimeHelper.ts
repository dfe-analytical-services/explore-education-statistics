import { fromZonedTime, formatInTimeZone } from 'date-fns-tz';

export default class UkTimeHelper {
  private static readonly TZ = 'Europe/London';

  public static ukStartOfDayUtc(isoDate: string): Date {
    return new Date(UkTimeHelper.dateMidnightUk(isoDate));
  }

  // End of day 23:59:59.999 in London, as a UTC instant
  public static ukEndOfDayUtc(isoDate: string | Date): Date {
    return new Date(UkTimeHelper.dateLastSecondUk(isoDate));
  }

  public static todayMidnightUk(): string {
    return UkTimeHelper.dateMidnightUk(new Date());
  }

  public static dateMidnightUk(date1: Date | string): string {
    return UkTimeHelper.dateTimeInLondonTimeZone(date1, TimeOption.MIDNIGHT);
  }

  public static dateLastSecondUk(date1: Date | string): string {
    return UkTimeHelper.dateTimeInLondonTimeZone(date1, TimeOption.LASTSECOND);
  }

  /**
   * Returns a string like 'YYYY-MM-DDTHH:mm:ss±HH:MM' representing
   * the requested London wall time (midnight or last tick) on the
   * same London calendar day as `dateTime`.
   *
   * Safe across DST because we construct the *London wall time* and
   * convert it to a real UTC instant via fromZonedTime.
   */
  public static dateTimeInLondonTimeZone(
    dateTime: Date | string,
    option: TimeOption,
  ): string {
    const ymdLondon = formatInTimeZone(dateTime, this.TZ, 'yyyy-MM-dd');

    const wall = option === TimeOption.MIDNIGHT ? 'T00:00:00' : 'T23:59:59.999';

    const utcInstant = fromZonedTime(`${ymdLondon}${wall}`, this.TZ);

    return formatInTimeZone(utcInstant, this.TZ, "yyyy-MM-dd'T'HH:mm:ssXXX");
  }
}
enum TimeOption {
  MIDNIGHT = 'midnight',
  LASTSECOND = 'lastSecond',
}
