import FormCheckboxGroup, {
  CheckboxOption,
  FormCheckboxGroupProps,
} from '@common/components/form/FormCheckboxGroup';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import useRegister from '@common/components/form/hooks/useRegister';
import handleAllCheckboxChange from '@common/components/form/util/handleAllCheckboxChange';
import getErrorMessage from '@common/components/form/util/getErrorMessage';
import React from 'react';
import { FieldValues, Path, useFormContext, useWatch } from 'react-hook-form';

export interface Props<TFormValues extends FieldValues>
  extends Omit<FormCheckboxGroupProps, 'name' | 'value' | 'id'> {
  name: Path<TFormValues>;
  id?: string;
  options: CheckboxOption[];
  showError?: boolean;
  disabled?: boolean;
}

export default function FormFieldCheckboxGroup<
  TFormValues extends FieldValues,
>({
  name,
  id: customId,
  options,
  showError = true,
  disabled = false,
  ...props
}: Props<TFormValues>) {
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
  const { onAllChange, onChange } = props;

  return (
    <FormCheckboxGroup
      {...props}
      {...field}
      disabled={disabled}
      error={getErrorMessage(errors, name, showError)}
      id={id}
      inputRef={inputRef}
      options={options}
      value={selectedValues}
      onAllChange={(event, checked) => {
        onAllChange?.(event, checked, options);

        if (event.isDefaultPrevented()) {
          return;
        }

        handleAllCheckboxChange({
          checked,
          name,
          options,
          selectedValues,
          setValue,
          trigger,
        });
      }}
      onChange={(event, option) => {
        onChange?.(event, option);

        if (event.isDefaultPrevented()) {
          return;
        }

        field.onChange(event);
      }}
    />
  );
}
