import { TimePeriodOption } from '@common/services/tableBuilderService';
import { TimePeriodCode } from '@common/services/types/TimePeriod';
import { Filter } from './filters';

export default class TimePeriod extends Filter {
  public readonly year: number;

  public readonly code: string;

  public constructor({ year, code, label }: TimePeriodOption) {
    super({ label, value: `${year}_${code}` });

    this.code = code;
    this.year = year;
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
