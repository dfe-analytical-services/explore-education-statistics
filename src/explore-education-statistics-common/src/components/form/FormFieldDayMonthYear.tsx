import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldset from '@common/components/form/FormFieldset';
import createErrorHelper from '@common/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React, { ReactNode } from 'react';

interface DayMonthYearValues {
  day: string;
  month: string;
  year: string;
}

interface Props<FormValues> {
  error?: ReactNode | string;
  legend: string;
  legendSize?: 'xl' | 'l' | 'm' | 's';
  id: string;
  name: keyof FormValues | string;
  showError?: boolean;
}

const FormFieldDayMonthYear = <FormValues extends {}>({
  id,
  name,
  showError = true,
  legend,
  legendSize = 'l',
}: Props<FormValues>) => {
  return (
    <Field name={name}>
      {({ form }: FieldProps) => {
        const { getError } = createErrorHelper<FormValues>(form);
        return (
          <FormFieldset
            id={id}
            legend={legend}
            legendSize={legendSize}
            error={showError ? getError(name) : ''}
          >
            <FormFieldNumberInput<DayMonthYearValues>
              id={`${name}.day`}
              name={`${name}.day`}
              label="Day"
              width={2}
              formGroupClass="govuk-date-input__item"
            />
            <FormFieldNumberInput<DayMonthYearValues>
              id={`${name}.month`}
              name={`${name}.month`}
              label="Month"
              width={2}
              formGroupClass="govuk-date-input__item"
            />
            <FormFieldNumberInput<DayMonthYearValues>
              id={`${name}.year`}
              name={`${name}.year`}
              label="Year"
              width={4}
              formGroupClass="govuk-date-input__item"
            />
          </FormFieldset>
        );
      }}
    </Field>
  );
};

export default FormFieldDayMonthYear;
