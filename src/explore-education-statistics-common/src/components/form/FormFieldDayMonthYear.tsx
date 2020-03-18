import FormFieldset from '@common/components/form/FormFieldset';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import createErrorHelper from '@common/validation/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React, { ReactNode } from 'react';

interface DayMonthYearValues {
  day: string;
  month: string;
  year: string;
}

interface Props<FormValues> extends DayMonthYearValues {
  error?: ReactNode | string;
  fieldsetLegend: string;
  fieldsetLegendSize?: 'xl' | 'l' | 'm' | 's';
  formId: string;
  fieldName: keyof FormValues;
  showError?: boolean;
}

const FormFieldDayMonthYear = <FormValues extends {}>({
  fieldsetLegend,
  formId,
  fieldName,
  showError = true,
  fieldsetLegendSize = 'l',
}: Props<FormValues>) => {
  return (
    <Field name={fieldName}>
      {({ form }: FieldProps) => {
        const { getError } = createErrorHelper<FormValues>(form);
        return (
          <FormFieldset
            id={`${formId}-${fieldName}`}
            legend={fieldsetLegend}
            legendSize={fieldsetLegendSize}
            error={showError ? getError(fieldName) : ''}
          >
            <FormFieldTextInput<DayMonthYearValues>
              id={`${fieldName}.day`}
              name={`${fieldName}.day`}
              label="Day"
              type="number"
              pattern="[0-9]*"
              width={2}
              formGroupClass="govuk-date-input__item"
            />
            <FormFieldTextInput<DayMonthYearValues>
              id={`${fieldName}.month`}
              name={`${fieldName}.month`}
              type="number"
              pattern="[0-9]*"
              label="Month"
              width={2}
              formGroupClass="govuk-date-input__item"
            />
            <FormFieldTextInput<DayMonthYearValues>
              id={`${fieldName}.year`}
              name={`${fieldName}.year`}
              type="number"
              pattern="[0-9]*"
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
