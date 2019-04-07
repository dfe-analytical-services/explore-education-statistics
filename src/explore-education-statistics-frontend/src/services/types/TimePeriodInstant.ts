import { Comparable } from 'src/types/util';

type TimePeriodCode = 'ACADEMIC';

const allowedTimePeriodCodes: TimePeriodCode[] = ['ACADEMIC'];

class TimePeriodInstant {
  public constructor(
    public readonly year: number,
    public readonly code: TimePeriodCode,
  ) {}

  public static fromString(value: string): TimePeriodInstant {
    const [year, code] = value.split('_');

    if (allowedTimePeriodCodes.indexOf(code as TimePeriodCode) === -1) {
      throw new TypeError('Could not parse time period code');
    }

    const parsedYear = Number(year);
    const parsedCode = code as TimePeriodCode;

    if (Number.isNaN(parsedYear)) {
      throw new TypeError('Could not parse time period year');
    }

    return new TimePeriodInstant(parsedYear, parsedCode);
  }

  public toString(): string {
    return `${this.year}_${this.code}`;
  }

  public compare(other: TimePeriodInstant): Comparable {
    if (this.year > other.year) {
      return Comparable.GreaterThan;
    }

    if (this.year < other.year) {
      return Comparable.LessThan;
    }

    // TODO: Implement logic for other codes
    if (this.code === other.code) {
      return Comparable.Equal;
    }

    throw new Error('Could not compare TimePeriods');
  }
}

export default TimePeriodInstant;
