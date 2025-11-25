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

  public static getDateRangeFromToday(daysToAdd: number): DateRange {
    const TZ = UkTimeHelper.europeLondonTimeZoneId;

    const startDate = new Date();
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

  public static todayStartOfDayUk(): Date {
    return this.toUkStartOfDay(new Date());
  }

  public static todayEndOfDayUk(): Date {
    return this.toUkEndOfDay(new Date());
  }
}
