import FormCheckboxSearchSubGroups, {
  FormCheckboxSearchSubGroupsProps,
} from '@common/components/form/FormCheckboxSearchSubGroups';
import { OtherCheckboxChangeProps } from '@common/components/form/FormCheckbox';
import useRegister from '@common/components/form/hooks/useRegister';
import handleAllCheckboxChange from '@common/components/form/util/handleAllCheckboxChange';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import get from 'lodash/get';
import React, { ChangeEvent, memo, useCallback } from 'react';
import { FieldValues, Path, useFormContext, useWatch } from 'react-hook-form';

export interface FormFieldCheckboxSearchSubGroupsProps<
  TFormValues extends FieldValues,
> extends Omit<
    FormCheckboxSearchSubGroupsProps,
    'name' | 'value' | 'inputRef' | 'id'
  > {
  name: Path<TFormValues>;
  id?: string;
  showError?: boolean;
}

function FormFieldCheckboxSearchSubGroups<TFormValues extends FieldValues>({
  name,
  id: customId,
  showError = true,
  ...props
}: FormFieldCheckboxSearchSubGroupsProps<TFormValues>) {
  const {
    formState: { errors },
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
    async (
      event: ChangeEvent<HTMLInputElement>,
      option: OtherCheckboxChangeProps,
    ) => {
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
        handleAllCheckboxChange({
          checked,
          name,
          options: options.flatMap(group => group.options),
          selectedValues,
          setValue,
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

        handleAllCheckboxChange({
          checked,
          name,
          options: groupOptions,
          selectedValues,
          setValue,
          trigger,
        });
      }}
    />
  );
}

export default memo(FormFieldCheckboxSearchSubGroups);
