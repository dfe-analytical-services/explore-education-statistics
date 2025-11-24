import { isToday, addDays } from 'date-fns';
import UkTimeHelper from '@common/utils/date/ukTimeHelper';
import { formatInTimeZone, fromZonedTime } from 'date-fns-tz';

export type DateRange = { startDate: Date; endDate: Date };

export default class PreviewTokenDateHelper {
  public setDateRangeBasedOnCustomDates(
    activates?: Date | null,
    expires?: Date | null,
  ): DateRange {
    // Validate that either both parameters have values or both are null/undefined
    const hasActivates = activates != null;
    const hasExpires = expires != null;

    if (hasActivates !== hasExpires) {
      throw new Error(
        'Both activates and expires dates must be provided together, or both must be null/undefined',
      );
    }

    if (activates && expires) {
      const isActivatesToday = isToday(activates);
      const startDate = !isActivatesToday
        ? UkTimeHelper.toUkStartOfDay(activates)
        : new Date();
      const endDate = UkTimeHelper.toUkEndOfDay(expires);
      return { startDate, endDate };
    }

    const TZ = UkTimeHelper.europeLondonTimeZoneId;
    // When no custom dates are provided: start = now, end = end of *tomorrow* (London)
    const startDate = new Date();

    // 1) “Today” in London, as a calendar date string
    const todayYmdLondon = formatInTimeZone(startDate, TZ, 'yyyy-MM-dd');

    // 2) Midnight today in London → UTC instant
    const todayMidnightUtc = fromZonedTime(`${todayYmdLondon}T00:00:00`, TZ);

    // 3) Add one *absolute* day, then re-resolve what “tomorrow” is in London
    const tomorrowMidnightUtc = addDays(todayMidnightUtc, 1);
    const tomorrowYmdLondon = formatInTimeZone(
      tomorrowMidnightUtc,
      TZ,
      'yyyy-MM-dd',
    );

    // 4) End of *tomorrow* in London, as a UTC instant
    const endDate = fromZonedTime(`${tomorrowYmdLondon}T23:59:59`, TZ);

    return { startDate, endDate };
  }

  public setDateRangeBasedOnPresets(datePresetSpan: number): DateRange {
    if (
      !(
        Number.isInteger(datePresetSpan) &&
        datePresetSpan > 0 &&
        datePresetSpan < 8
      )
    ) {
      throw new Error(
        `The number of days (${datePresetSpan}) selected is not allowed, please select between 1 to 7 days.`,
      );
    }
    const TZ = UkTimeHelper.europeLondonTimeZoneId;

    const startDate = new Date();
    // 1) "Today" in London as a calendar date (no time)
    const todayYmdLondon = formatInTimeZone(startDate, TZ, 'yyyy-MM-dd');

    // 2) Today at 00:00 London → UTC instant
    const todayMidnightUtc = fromZonedTime(`${todayYmdLondon}T00:00:00`, TZ);

    // 3) Add N *calendar* days from that midnight
    const plusNMidnightUtc = addDays(todayMidnightUtc, datePresetSpan);

    // 4) What calendar day is that in London?
    const plusNYmdLondon = formatInTimeZone(plusNMidnightUtc, TZ, 'yyyy-MM-dd');

    // 5) End of that day in London → UTC instant
    const endDate = fromZonedTime(`${plusNYmdLondon}T23:59:59`, TZ);

    return { startDate, endDate };
  }
}
