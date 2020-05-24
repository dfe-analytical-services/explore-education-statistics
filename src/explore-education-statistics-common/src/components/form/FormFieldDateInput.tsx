import FormFieldset from '@common/components/form/FormFieldset';
import FormNumberInput from '@common/components/form/FormNumberInput';
import { FormGroup } from '@common/components/form/index';
import {
  DayMonthYear,
  isValidDayMonthYear,
} from '@common/utils/date/dayMonthYear';
import parseNumber from '@common/utils/number/parseNumber';
import { isValid, parse } from 'date-fns';
import { FormikErrors, useField } from 'formik';
import last from 'lodash/last';
import React, { ChangeEvent, useCallback, useState } from 'react';

interface Props<FormValues> {
  hint?: string;
  id: string;
  legend: string;
  legendSize?: 'xl' | 'l' | 'm' | 's';
  name: keyof FormValues | string;
  showError?: boolean;
  type?: 'date' | 'dayMonthYear';
}

const FormFieldDateInput = <FormValues extends {}>({
  id,
  name,
  showError = true,
  hint,
  legend,
  legendSize = 'l',
  type = 'date',
}: Props<FormValues>) => {
  const [field, meta, helpers] = useField<
    Date | Partial<DayMonthYear> | undefined
  >(name as string);

  const [values, setValues] = useState<Partial<DayMonthYear>>(() => {
    if (field.value instanceof Date) {
      return {
        day: field.value?.getDate(),
        month: field.value?.getMonth()
          ? field.value?.getMonth() + 1
          : undefined,
        year: field.value?.getFullYear(),
      };
    }

    return {
      day: field.value?.day,
      month: field.value?.month,
      year: field.value?.year,
    };
  });

  const error =
    showError && meta.error && meta.touched
      ? (meta.error as FormikErrors<DayMonthYear> | string)
      : {};

  const errorMessage =
    typeof error === 'string' ? error : Object.values(error)[0] ?? '';

  const handleChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      const key = last(event.target.name.split('.')) as keyof DayMonthYear;

      if (!['day', 'month', 'year'].includes(key)) {
        throw new Error(`Invalid key for day/month/year field: ${key}`);
      }

      const inputValue = parseNumber(event.target.value);

      const day = key === 'day' ? inputValue : values.day;
      const month = key === 'month' ? inputValue : values.month;
      const year = key === 'year' ? inputValue : values.year;

      if (type === 'date') {
        const date = parse(`${year}-${month}-${day}`, 'yyyy-M-d', new Date());

        helpers.setValue(isValid(date) ? date : undefined);
      } else {
        helpers.setValue({ day, month, year });
      }

      setValues({
        ...values,
        [key]: inputValue,
      });
    },
    [helpers, type, values],
  );

  return (
    <FormFieldset
      id={id}
      legend={legend}
      legendSize={legendSize}
      hint={hint}
      error={errorMessage}
    >
      <FormGroup className="govuk-date-input__item">
        <FormNumberInput
          {...field}
          id={`${id}-day`}
          name={`${name}.day`}
          label="Day"
          width={2}
          value={values.day}
          onChange={handleChange}
        />
      </FormGroup>

      <FormGroup className="govuk-date-input__item">
        <FormNumberInput
          {...field}
          id={`${id}-month`}
          name={`${name}.month`}
          label="Month"
          width={2}
          value={values.month}
          onChange={handleChange}
        />
      </FormGroup>

      <FormGroup className="govuk-date-input__item">
        <FormNumberInput
          {...field}
          id={`${id}-year`}
          name={`${name}.year`}
          label="Year"
          width={4}
          value={values.year}
          onChange={handleChange}
        />
      </FormGroup>
    </FormFieldset>
  );
};

export default FormFieldDateInput;
