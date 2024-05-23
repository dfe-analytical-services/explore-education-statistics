import FormCheckboxSearchGroup, {
  FormCheckboxSearchGroupProps,
} from '@common/components/form/FormCheckboxSearchGroup';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import useRegister from '@common/components/form/hooks/useRegister';
import handleAllCheckboxChange from '@common/components/form/util/handleAllCheckboxChange';
import getErrorMessage from '@common/components/form/util/getErrorMessage';
import React from 'react';
import { FieldValues, Path, useFormContext, useWatch } from 'react-hook-form';

export interface FormFieldCheckboxSearchGroupProps<
  TFormValues extends FieldValues,
> extends Omit<FormCheckboxSearchGroupProps, 'name' | 'value' | 'id'> {
  name: Path<TFormValues>;
  id?: string;
  showError?: boolean;
}

export default function FormFieldCheckboxSearchGroup<
  TFormValues extends FieldValues,
>({
  name,
  id: customId,
  showError = true,
  ...props
}: FormFieldCheckboxSearchGroupProps<TFormValues>) {
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
    <FormCheckboxSearchGroup
      {...props}
      {...field}
      error={getErrorMessage(errors, name, showError)}
      id={id}
      inputRef={inputRef}
      value={selectedValues}
      onAllChange={(event, checked, options) => {
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
