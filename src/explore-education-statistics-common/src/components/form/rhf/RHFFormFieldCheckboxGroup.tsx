import FormCheckboxGroup, {
  CheckboxOption,
  FormCheckboxGroupProps,
} from '@common/components/form/FormCheckboxGroup';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import useRegister from '@common/components/form/rhf/hooks/useRegister';
import handleAllRHFCheckboxChange from '@common/components/form/rhf/util/handleAllRHFCheckboxChange';
import getErrorMessage from '@common/components/form/rhf/util/getErrorMessage';
import React from 'react';
import { FieldValues, Path, useFormContext, useWatch } from 'react-hook-form';

export interface Props<TFormValues extends FieldValues>
  extends Omit<FormCheckboxGroupProps, 'name' | 'value' | 'id'> {
  name: Path<TFormValues>;
  id?: string;
  options: CheckboxOption[];
  showError?: boolean;
}

export default function RHFFormFieldCheckboxGroup<
  TFormValues extends FieldValues,
>({
  name,
  id: customId,
  options,
  showError = true,
  ...props
}: Props<TFormValues>) {
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
  const { onAllChange, onChange } = props;

  return (
    <FormCheckboxGroup
      {...props}
      {...field}
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

        handleAllRHFCheckboxChange({
          checked,
          name,
          options,
          selectedValues,
          setValue,
          submitCount,
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
