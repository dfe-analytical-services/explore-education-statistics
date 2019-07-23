import { dayMonthYearIsComplete } from '@admin/services/common/types';
import Yup from '@common/lib/validation/yup';
import { isValid } from 'date-fns';

const yupNumberOrUndefinedIfBlank = Yup.mixed().transform((val: string) =>
  val === '' ? undefined : parseInt(val, 10),
);

/**
 * Validator that validates that all fields of a day-month-year field are entered, and that the combination
 * is a valid date.
 */
export const validateMandatoryDayMonthYearField = Yup.object({
  day: yupNumberOrUndefinedIfBlank.required('Enter a day'),
  month: yupNumberOrUndefinedIfBlank.required('Enter a month'),
  year: yupNumberOrUndefinedIfBlank.required('Enter a year'),
}).test('validDate', 'Enter a valid date', value =>
  isValid(new Date(value.year, value.month, value.day)),
);

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
  return isValid(new Date(value.year, value.month, value.day));
});

export const shapeOfDayMonthYearField = Yup.object({
  day: Yup.number(),
  month: Yup.number(),
  year: Yup.number(),
});
