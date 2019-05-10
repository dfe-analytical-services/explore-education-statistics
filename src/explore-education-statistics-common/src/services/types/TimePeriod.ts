import { Comparison } from '@common/types/util';

type TimePeriodCode = 'AY' | 'HT6' | 'HT5';

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

    if (this.code === other.code) {
      return Comparison.EqualTo;
    }

    if (this.code === 'HT5' || this.code === 'HT6') {
      if (other.code === 'HT5' || other.code === 'HT6') {
        return Comparison.EqualTo;
      }
    }

    throw new Error('Could not compare TimePeriods');
  }
}

export default TimePeriod;
