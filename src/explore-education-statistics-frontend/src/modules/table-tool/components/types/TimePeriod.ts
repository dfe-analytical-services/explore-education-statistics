import { TimePeriodOption } from '@common/services/tableBuilderService';
import { TimePeriodCode } from '@common/services/types/TimePeriod';

export default class TimePeriod {
  public readonly year: number;

  public readonly code: string;

  public readonly label: string;

  public constructor({ year, code, label }: TimePeriodOption) {
    this.code = code;
    this.year = year;
    this.label = label;
  }

  public get value(): string {
    return `${this.year}_${this.code}`;
  }
}

export function parseYearCodeTuple(value: string): [number, string] {
  const [year, code] = value.split('_');

  const parsedYear = Number(year);
  const parsedCode = code as TimePeriodCode;

  if (Number.isNaN(parsedYear)) {
    throw new TypeError('Could not parse time period year');
  }

  return [parsedYear, parsedCode];
}
