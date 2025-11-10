import { isToday } from 'date-fns';
import UkTimeHelper from '@common/utils/date/ukTimeHelper';

export type DateRange = { startDate: Date; endDate: Date };

export default class PreviewTokenDateHelper {
  public setDateRangeBasedOnCustomDates(
    activates: Date | null | undefined,
    expires: Date | null | undefined,
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

    const startDate = new Date();
    const endDate = new Date(startDate.getTime() + 24 * 60 * 60 * 1000);
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

    // Create the endDate object as a copy of the start date
    let endDate = new Date(startDate);
    endDate.setUTCDate(endDate.getUTCDate() + datePresetSpan);
    endDate = UkTimeHelper.ukEndOfDayUtc(endDate.toISOString()); // Sets to 23:59:59 in UK X days from the current start date
    return { startDate, endDate };
  }
}
