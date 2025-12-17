import { isToday } from 'date-fns';
import UkTimeHelper, { DateRange } from '@common/utils/date/ukTimeHelper';

export default class PreviewTokenDateHelper {
  public setDateRangeBasedOnCustomDates(
    activates?: Date,
    expires?: Date,
  ): DateRange {
    if (!!activates !== !!expires) {
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
    const { startDate, endDate } = UkTimeHelper.getDateRangeFromDate(1);

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
    return UkTimeHelper.getDateRangeFromDate(datePresetSpan);
  }
}
