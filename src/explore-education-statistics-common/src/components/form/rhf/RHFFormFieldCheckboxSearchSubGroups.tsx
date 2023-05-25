import FormCheckboxSearchSubGroups, {
  FormCheckboxSearchSubGroupsProps,
} from '@common/components/form/FormCheckboxSearchSubGroups';
import useRegister from '@common/components/form/rhf/hooks/useRegister';
import handleAllRHFCheckboxChange from '@common/components/form/rhf/util/handleAllRHFCheckboxChange';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import get from 'lodash/get';
import React, { memo, useCallback } from 'react';
import { FieldValues, Path, useFormContext, useWatch } from 'react-hook-form';

export interface RHFFormFieldCheckboxSearchSubGroupsProps<
  TFormValues extends FieldValues
> extends Omit<
    FormCheckboxSearchSubGroupsProps,
    'name' | 'value' | 'inputRef' | 'id'
  > {
  name: Path<TFormValues>;
  id?: string;
  showError?: boolean;
}

function RHFFormFieldCheckboxSearchSubGroups<TFormValues extends FieldValues>({
  name,
  id: customId,
  showError = true,
  ...props
}: RHFFormFieldCheckboxSearchSubGroupsProps<TFormValues>) {
  const {
    formState: { errors, submitCount },
    register,
    setValue,
    trigger,
  } = useFormContext<TFormValues>();

  const { ref: inputRef, ...field } = useRegister(name, register);
  const { fieldId } = useFormIdContext();
  const id = fieldId(name, customId);

  const selectedValues = useWatch({ name }) || [];

  // We only do the following to avoid the dependency array
  // lint in `handleChange`. We need this for feature
  // parity with `FormFieldCheckboxSearchSubGroups`.
  const propsOnChange = props.onChange;
  const propsOnSubGroupAllChange = props.onSubGroupAllChange;
  const fieldOnChange = field.onChange;

  const handleChange = useCallback(
    async (event, option) => {
      if (propsOnChange) {
        propsOnChange(event, option);
      }

      if (event.isDefaultPrevented()) {
        return;
      }

      await fieldOnChange(event);
    },
    [propsOnChange, fieldOnChange],
  );

  return (
    <FormCheckboxSearchSubGroups
      {...props}
      {...field}
      error={showError ? (get(errors, name)?.message as string) : ''}
      id={id}
      inputRef={inputRef}
      small
      value={selectedValues}
      onAllChange={(event, checked, options) => {
        if (event.isDefaultPrevented()) {
          return;
        }
        handleAllRHFCheckboxChange({
          checked,
          name,
          options: options.flatMap(group => group.options),
          selectedValues,
          setValue,
          submitCount,
          trigger,
        });
      }}
      onChange={handleChange}
      onSubGroupAllChange={(event, checked, groupOptions) => {
        if (propsOnSubGroupAllChange) {
          propsOnSubGroupAllChange(event, checked, groupOptions);
        }

        if (event.isDefaultPrevented()) {
          return;
        }

        handleAllRHFCheckboxChange({
          checked,
          name,
          options: groupOptions,
          selectedValues,
          setValue,
          submitCount,
          trigger,
        });
      }}
    />
  );
}

export default memo(RHFFormFieldCheckboxSearchSubGroups);
