import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormFieldset from '@common/components/form/FormFieldset';
import FormNumberInput from '@common/components/form/FormNumberInput';
import { FormGroup } from '@common/components/form/index';
import { PartialDate } from '@common/utils/date/partialDate';
import delay from '@common/utils/delay';
import parseNumber from '@common/utils/number/parseNumber';
import { isValid, parse } from 'date-fns';
import { FormikErrors, useField } from 'formik';
import last from 'lodash/last';
import React, { ChangeEvent, useCallback, useRef, useState } from 'react';

interface Props<FormValues> {
  hint?: string;
  id?: string;
  legend: string;
  legendSize?: 'xl' | 'l' | 'm' | 's';
  name: keyof FormValues | string;
  partialDateType?: 'dayMonthYear' | 'monthYear';
  showError?: boolean;
  type?: 'date' | 'partialDate';
}

function FormFieldDateInput<FormValues>({
  id: customId,
  name,
  showError = true,
  hint,
  legend,
  legendSize = 'l',
  partialDateType = 'dayMonthYear',
  type = 'date',
}: Props<FormValues>) {
  const { fieldId } = useFormIdContext();
  const id = fieldId(name as string, customId);

  const isFocused = useRef(false);

  const [field, meta, helpers] = useField<
    Date | Partial<PartialDate> | undefined
  >(name as string);

  const [values, setValues] = useState<Partial<PartialDate>>(() => {
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
      ? (meta.error as FormikErrors<PartialDate> | string)
      : {};

  const errorMessage =
    typeof error === 'string' ? error : Object.values(error)[0] ?? '';

  const handleChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      const key = last(event.target.name.split('.')) as keyof PartialDate;

      if (!['day', 'month', 'year'].includes(key)) {
        throw new Error(`Invalid key for day/month/year field: ${key}`);
      }

      const inputValue = parseNumber(event.target.value);

      const day = key === 'day' ? inputValue : values.day;
      const month = key === 'month' ? inputValue : values.month;
      const year = key === 'year' ? inputValue : values.year;

      if (type === 'date') {
        const date = parse(`${year}-${month}-${day}Z`, 'yyyy-M-dX', new Date());

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
      useFormId={false}
      onBlur={async event => {
        event.persist();

        if (isFocused.current) {
          isFocused.current = false;
        }

        await delay();

        if (!isFocused.current) {
          field.onBlur(event);
        }
      }}
      onFocus={() => {
        isFocused.current = true;
      }}
    >
      {(partialDateType === 'dayMonthYear' || type === 'date') && (
        <FormGroup className="govuk-date-input__item">
          <FormNumberInput
            id={`${id}-day`}
            name={`${name as string}.day`}
            label="Day"
            width={2}
            value={parseNumber(values.day)}
            onChange={handleChange}
          />
        </FormGroup>
      )}

      <FormGroup className="govuk-date-input__item">
        <FormNumberInput
          id={`${id}-month`}
          name={`${name as string}.month`}
          label="Month"
          width={2}
          value={parseNumber(values.month)}
          onChange={handleChange}
        />
      </FormGroup>

      <FormGroup className="govuk-date-input__item">
        <FormNumberInput
          id={`${id}-year`}
          name={`${name as string}.year`}
          label="Year"
          width={4}
          value={parseNumber(values.year)}
          onChange={handleChange}
        />
      </FormGroup>
    </FormFieldset>
  );
}

export default FormFieldDateInput;
