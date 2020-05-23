import { dayMonthYearIsComplete } from '@common/utils/date/dayMonthYear';
import Yup from '@common/validation/yup';
import { isValid, parse } from 'date-fns';

const yupNumberOrUndefinedIfBlank = Yup.mixed().transform((val: string) =>
  val === '' ? undefined : parseInt(val, 10),
);

interface ParseDateParams {
  value: { year: number; month: number; day: number };
  backupDate?: Date;
}

export const parseDate = ({
  value,
  backupDate = new Date(value.year, value.month, value.day),
}: ParseDateParams) => {
  return parse(
    `${value.year}-${value.month}-${value.day}`,
    'yyyy-M-d',
    backupDate,
  );
};

/**
 * A validator that allows for a user to only enter certain fields of a day-month-year field group, but if all
 * fields are entered, validates that it is a valid date.
 */
export const validateOptionalPartialDayMonthYearField = Yup.object({
  day: yupNumberOrUndefinedIfBlank.nullable(),
  month: yupNumberOrUndefinedIfBlank.nullable(),
  year: yupNumberOrUndefinedIfBlank.nullable(),
}).test('validDateIfAllFieldsDefined', 'Enter a valid date', value => {
  // if not all fields have yet been filled in, this is valid
  if (!dayMonthYearIsComplete(value)) {
    return true;
  }

  // but if all fields have been filled in, they should represent a valid date
  return isValid(parseDate({ value }));
});
