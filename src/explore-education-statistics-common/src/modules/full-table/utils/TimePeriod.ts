import { TimePeriodCode } from '@common/services/types/TimePeriod';
import {TimeIdentifier} from "@common/modules/full-table/services/tableBuilderService";

export default function parseYearCodeTuple(value: string): [number, TimeIdentifier] {
  const [year, code] = value.split('_');

  const parsedYear = Number(year);
  const parsedCode = code as TimePeriodCode;

  if (Number.isNaN(parsedYear)) {
    throw new TypeError('Could not parse time period year');
  }

  return [parsedYear, parsedCode];
}
