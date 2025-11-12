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
        ? new Date(
            UkTimeHelper.ukStartOfDayUtc(
              activates.toISOString().substring(0, 10),
            ),
          )
        : new Date();
      const endDate = UkTimeHelper.ukEndOfDayUtc(
        expires.toISOString().substring(0, 10),
      );
      return { startDate, endDate };
    }

    const TZ = 'Europe/London';
    const startDate = new Date();

    const plusOneDayUtc = addDays(startDate, 1);
    const tomorrowYmdLondon = formatInTimeZone(plusOneDayUtc, TZ, 'yyyy-MM-dd');

    const endDate = fromZonedTime(`${tomorrowYmdLondon}T23:59:59.999`, TZ);

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
    const startDate = new Date();
    let endDate = addDays(startDate, datePresetSpan);
    endDate = UkTimeHelper.ukEndOfDayUtc(endDate.toISOString());
    return { startDate, endDate };
  }
}
