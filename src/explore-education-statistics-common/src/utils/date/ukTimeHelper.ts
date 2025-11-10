import { formatInTimeZone } from 'date-fns-tz';

export default class UkTimeHelper {
  public static ukStartOfDayUtc(isoDate: string): Date {
    return new Date(UkTimeHelper.dateMidnightUk(isoDate));
  }

  // End of day 23:59:59.999 in London, as a UTC instant
  public static ukEndOfDayUtc(isoDate: string | Date): Date {
    return new Date(UkTimeHelper.dateLastTickUk(isoDate));
  }

  public static todayMidnightUk(): string {
    return UkTimeHelper.dateMidnightUk(new Date());
  }

  public static dateMidnightUk(date1: Date | string): string {
    return UkTimeHelper.dateTimeInLondonTimeZone(date1, TimeOption.MIDNIGHT);
  }

  public static dateLastTickUk(date1: Date | string): string {
    return UkTimeHelper.dateTimeInLondonTimeZone(date1, TimeOption.LASTTICK);
  }

  public static dateTimeInLondonTimeZone(
    dateTime: Date | string,
    option: TimeOption,
  ): string {
    const formattedTimeNowInUk: string | Date = formatInTimeZone(
      dateTime,
      'Europe/London',
      'yyyy-MM-dd HH:mm:ssXXX',
    );
    if (formattedTimeNowInUk === undefined) {
      throw new Error('Could not format date');
    }
    const selectedOptionForTime =
      option === TimeOption.MIDNIGHT ? 'T00:00:00' : 'T23:59:59';

    const match = formattedTimeNowInUk.match(/([+-]\d{2}:\d{2}|Z)/i);
    if (match) {
      const offset = match[1] === 'Z' ? '+00:00' : match[1];
      return `${formattedTimeNowInUk.substring(
        0,
        10,
      )}${selectedOptionForTime}${offset}`;
    }

    throw new Error('Could not find timezone offset');
  }
}
enum TimeOption {
  MIDNIGHT = 'midnight',
  LASTTICK = 'lastTick',
}
