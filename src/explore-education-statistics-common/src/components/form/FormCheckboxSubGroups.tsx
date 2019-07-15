import { Overwrite } from '@common/types/util';
import camelCase from 'lodash/camelCase';
import React from 'react';
import FormCheckboxGroup, {
  CheckboxGroupAllChangeEvent,
  CheckboxOption,
  FormCheckboxGroupProps,
} from './FormCheckboxGroup';
import FormFieldset, { FormFieldsetProps } from './FormFieldset';

export type FormCheckboxSubGroupsProps = Overwrite<
  FormCheckboxGroupProps,
  {
    options: {
      id?: string;
      legend: string;
      options: CheckboxOption[];
    }[];
    onAllChange?: (
      event: CheckboxGroupAllChangeEvent,
      checked: boolean,
      options: CheckboxOption[],
    ) => void;
  }
>;

const FormCheckboxSubGroups = ({
  legendHidden,
  legendSize = 's',
  legend,
  hint,
  error,
  ...props
}: FormCheckboxSubGroupsProps) => {
  const { id, onAllChange, options } = props;
  const fieldsetProps: FormFieldsetProps = {
    id,
    legend,
    legendSize,
    legendHidden,
    hint,
    error,
  };

  return (
    <FormFieldset {...fieldsetProps}>
      {options.map(optionGroup => {
        return (
          <FormCheckboxGroup
            {...props}
            id={
              optionGroup.id
                ? optionGroup.id
                : `${id}-${camelCase(optionGroup.legend)}`
            }
            key={optionGroup.legend}
            legend={optionGroup.legend}
            legendSize="s"
            options={optionGroup.options}
            onAllChange={(event, checked) => {
              if (onAllChange) {
                onAllChange(event, checked, optionGroup.options);
              }
            }}
          />
        );
      })}
    </FormFieldset>
  );
};

export default FormCheckboxSubGroups;
