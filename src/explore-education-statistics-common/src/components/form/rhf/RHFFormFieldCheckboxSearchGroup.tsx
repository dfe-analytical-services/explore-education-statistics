import FormCheckboxSearchGroup, {
  FormCheckboxSearchGroupProps,
} from '@common/components/form/FormCheckboxSearchGroup';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import useRegister from '@common/components/form/rhf/hooks/useRegister';
import handleAllRHFCheckboxChange from '@common/components/form/rhf/util/handleAllRHFCheckboxChange';
import getErrorMessage from '@common/components/form/rhf/util/getErrorMessage';
import React from 'react';
import { FieldValues, Path, useFormContext, useWatch } from 'react-hook-form';

export interface RHFFormFieldCheckboxSearchGroupProps<
  TFormValues extends FieldValues,
> extends Omit<FormCheckboxSearchGroupProps, 'name' | 'value' | 'id'> {
  name: Path<TFormValues>;
  id?: string;
  showError?: boolean;
}

export default function RHFFormFieldCheckboxSearchGroup<
  TFormValues extends FieldValues,
>({
  name,
  id: customId,
  showError = true,
  ...props
}: RHFFormFieldCheckboxSearchGroupProps<TFormValues>) {
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
