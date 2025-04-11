import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormFieldset from '@common/components/form/FormFieldset';
import FormNumberInput from '@common/components/form/FormNumberInput';
import { FormGroup } from '@common/components/form/index';
import { PartialDate } from '@common/utils/date/partialDate';
import parseNumber from '@common/utils/number/parseNumber';
import { isValid, parse } from 'date-fns';
import last from 'lodash/last';
import React, { ChangeEvent, useCallback, useState } from 'react';
import {
  FieldValues,
  Path,
  PathValue,
  useFormContext,
  useWatch,
} from 'react-hook-form';
import getErrorMessage from './util/getErrorMessage';

interface Props<TFormValues> {
  hint?: string;
  id?: string;
  legend: string;
  legendSize?: 'xl' | 'l' | 'm' | 's';
  name: Path<TFormValues>;
  partialDateType?: 'dayMonthYear' | 'monthYear';
  showError?: boolean;
  type?: 'date' | 'partialDate';
}

export default function FormFieldDateInput<TFormValues extends FieldValues>({
  id: customId,
  name,
  showError = true,
  hint,
  legend,
  legendSize = 'l',
  partialDateType = 'dayMonthYear',
  type = 'date',
}: Props<TFormValues>) {
  const { fieldId } = useFormIdContext();
  const id = fieldId(name as string, customId);

  const {
    formState: { errors, isSubmitted },
    setValue,
    trigger,
  } = useFormContext<TFormValues>();

  const value = useWatch({ name });

  const [values, setValues] = useState<Partial<PartialDate>>(() => {
    const dateValue = value as Date;
    if (value && typeof dateValue.getDate === 'function') {
      return {
        day: dateValue.getDate(),
        month:
          typeof dateValue.getMonth() === 'number'
            ? dateValue.getMonth() + 1
            : undefined,
        year: dateValue.getFullYear(),
      };
    }

    const partialDateValue = value as PartialDate;

    return {
      day: partialDateValue?.day,
      month: partialDateValue?.month,
      year: partialDateValue?.year,
    };
  });

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

        const newDateValue = isValid(date) ? date : undefined;
        setValue(
          name,
          newDateValue as PathValue<TFormValues, Path<TFormValues>>,
          { shouldTouch: true },
        );
      } else {
        const nextValue =
          !day && !month && !year ? undefined : { day, month, year };

        setValue(name, nextValue as PathValue<TFormValues, Path<TFormValues>>, {
          shouldTouch: true,
        });
      }

      setValues({
        ...values,
        [key]: inputValue,
      });

      if (isSubmitted) {
        trigger(name);
      }
    },
    [name, isSubmitted, setValue, trigger, type, values],
  );

  return (
    <FormFieldset
      id={id}
      legend={legend}
      legendSize={legendSize}
      hint={hint}
      error={getErrorMessage(errors, name, showError)}
      useFormId={false}
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
