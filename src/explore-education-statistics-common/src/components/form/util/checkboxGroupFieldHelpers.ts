import { FieldArrayRenderProps } from 'formik';
import difference from 'lodash/difference';
import get from 'lodash/get';
import { ChangeEvent } from 'react';
import {
  CheckboxGroupAllChangeEvent,
  CheckboxOption,
} from '../FormCheckboxGroup';

export const onAllChange = (
  { form, name }: FieldArrayRenderProps,
  options: CheckboxOption[],
) => {
  return (event: CheckboxGroupAllChangeEvent) => {
    if (event.isDefaultPrevented()) {
      return;
    }

    const currentValues: string[] = get(form.values, name);

    const allOptionValues = options.map(option => option.value);
    const restValues = difference(currentValues, allOptionValues);

    if (currentValues.length < allOptionValues.length) {
      form.setFieldValue(name, [...restValues, ...allOptionValues]);
    } else {
      form.setFieldValue(name, restValues);
    }
  };
};

export const onChange = ({ form, name, ...helpers }: FieldArrayRenderProps) => {
  return (event: ChangeEvent<HTMLInputElement>) => {
    if (event.isDefaultPrevented()) {
      return;
    }

    const currentValues = get(form.values, name);

    if (event.target.checked) {
      helpers.push(event.target.value);
    } else {
      const index = currentValues.indexOf(event.target.value);

      if (index > -1) {
        helpers.remove(index);
      }
    }
  };
};
