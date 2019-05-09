import { Comparison } from '@common/types/util';

export type TimePeriodCode = 'AY' | 'HT6' | 'HT5';

const allowedTimePeriodCodes: TimePeriodCode[] = ['AY', 'HT6', 'HT5'];

class TimePeriod {
  public readonly year: number;

  public readonly code: TimePeriodCode;

  public constructor(year: number, code: TimePeriodCode) {
    this.code = code;
    this.year = year;
  }

  public static fromString(value: string): TimePeriod {
    const [year, code] = value.split('_');

    if (allowedTimePeriodCodes.indexOf(code as TimePeriodCode) === -1) {
      throw new TypeError('Could not parse time period code');
    }

    const parsedYear = Number(year);
    const parsedCode = code as TimePeriodCode;

    if (Number.isNaN(parsedYear)) {
      throw new TypeError('Could not parse time period year');
    }

    return new TimePeriod(parsedYear, parsedCode);
  }

  public static createRange(start: TimePeriod, end: TimePeriod): TimePeriod[] {
    const instants: TimePeriod[] = [];

    let next = start;

    while (next.compare(end) < Comparison.GreaterThan) {
      instants.push(next);
      next = next.nextPeriod();
    }

    return instants;
  }

  public get label(): string {
    switch (this.code) {
      case 'AY':
      case 'HT6':
      case 'HT5': {
        const yearString = this.year.toString();
        return `${yearString}/${Number(yearString.substring(2, 4)) + 1}`;
      }
      default:
        throw new Error('Cold not parse label');
    }
  }

  public get value(): string {
    return `${this.year}_${this.code}`;
  }

  public previousPeriod(): TimePeriod {
    switch (this.code) {
      case 'AY':
      case 'HT6':
      case 'HT5':
        return new TimePeriod(this.year - 1, this.code);
      default:
        throw new Error('Could not parse previous time period');
    }
  }

  public nextPeriod(): TimePeriod {
    switch (this.code) {
      case 'AY':
      case 'HT6':
      case 'HT5':
        return new TimePeriod(this.year + 1, this.code);
      default:
        throw new Error('Could not parse next time period');
    }
  }

  public compare(other: TimePeriod): Comparison {
    if (this.year > other.year) {
      return Comparison.GreaterThan;
    }

    if (this.year < other.year) {
      return Comparison.LessThan;
    }

    // TODO: Implement logic for other codes
    if (this.code === other.code) {
      return Comparison.EqualTo;
    }

    throw new Error('Could not compare TimePeriods');
  }
}

export default TimePeriod;
