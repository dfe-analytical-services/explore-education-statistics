import { isToday } from 'date-fns';
import { zonedTimeToUtc } from 'date-fns-tz';

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
      const startDate = !isToday(activates)
        ? this.ukToUtcAtTime(activates, '00:00:00')
        : this.ukToUtcAtTime(new Date());
      const endDate = this.ukToUtcAtTime(expires, '23:59:59');
      return { startDate, endDate };
    }
    const startDate = this.ukToUtcAtTime(new Date());
    const endDate = new Date(this.ukToUtcAtTime(new Date()));
    endDate.setUTCDate(endDate.getUTCDate() + 1); // 24 hours
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

    const endAnchor = new Date(this.ukToUtcAtTime(new Date()));
    endAnchor.setUTCDate(endAnchor.getUTCDate() + datePresetSpan); // add pre-set days
    const endDate = this.ukToUtcAtTime(endAnchor, '23:59:59');

    // use current time as start date
    return { startDate: this.ukToUtcAtTime(new Date()), endDate };
  }

  /** Convert a given Date's calendar day (in 'Europe/London') plus HH:mm:ss into UTC Date. */
  private ukToUtcAtTime(
    dateParam: Date,
    timeParam: string | null = null,
  ): Date {
    let time = timeParam;
    if (!timeParam) {
      time = this.hhmmss(dateParam);
    }
    const localIso = `${dateParam.toISOString().substring(0, 10)}T${time}`;
    return zonedTimeToUtc(localIso, 'Europe/London');
  }

  /** Formats HH:mm:ss from a Date's local time portion. */
  private hhmmss(d: Date): string {
    // toTimeString() => "HH:mm:ss GMT±.. (...)"; first 8 chars are HH:mm:ss
    return d.toTimeString().substring(0, 8);
  }
}
