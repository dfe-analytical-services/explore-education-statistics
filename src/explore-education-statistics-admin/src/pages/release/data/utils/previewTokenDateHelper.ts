import { isToday } from 'date-fns';
import { zonedTimeToUtc } from 'date-fns-tz';

export type DateRange = { startDate: Date; endDate: Date };

export default class PreviewTokenDateHelper {
  constructor(
    private readonly timezone: string = 'Europe/London',
    private readonly nowUk: Date = this.toUtcAtTime(new Date()),
  ) {}

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
        ? this.toUtcAtTime(activates, '00:00:00')
        : this.nowUk;
      const endDate = this.toUtcAtTime(expires, '23:59:59'); // set custom dates
      return { startDate, endDate };
    }
    const startDate = this.nowUk;
    const endDate = new Date(this.nowUk);
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

    const endAnchor = new Date(this.nowUk);
    endAnchor.setUTCDate(endAnchor.getUTCDate() + datePresetSpan); // add pre set days
    const endDate = this.toUtcAtTime(endAnchor, '23:59:59');

    // use current time as start date
    return { startDate: this.nowUk, endDate };
  }

  /** Convert a given Date's calendar day (in this.timezone) plus HH:mm:ss into UTC Date. */
  private toUtcAtTime(dateParam: Date, timeParam: string | null = null): Date {
    const yyyy = dateParam.getFullYear();
    const mm = String(dateParam.getMonth() + 1).padStart(2, '0');
    const dd = String(dateParam.getDate()).padStart(2, '0');
    let time = timeParam;
    if (!timeParam) {
      time = this.hhmmss(dateParam);
    }
    const localIso = `${yyyy}-${mm}-${dd}T${time}`;
    return zonedTimeToUtc(localIso, this.timezone);
  }

  /** Formats HH:mm:ss from a Date's local time portion. */
  private hhmmss(d: Date): string {
    // toTimeString() => "HH:mm:ss GMT±.. (...)"; first 8 chars are HH:mm:ss
    return d.toTimeString().substring(0, 8);
  }
}
