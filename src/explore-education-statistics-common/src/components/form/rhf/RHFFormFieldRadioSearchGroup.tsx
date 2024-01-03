import FormRadioSearchGroup, {
  FormRadioSearchGroupProps,
} from '@common/components/form/FormRadioSearchGroup';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import useRegister from '@common/components/form/rhf/hooks/useRegister';
import React from 'react';
import { FieldValues, Path, useFormContext, useWatch } from 'react-hook-form';

export interface RHFFormFieldRadioSearchGroupProps<
  TFormValues extends FieldValues,
> extends Omit<FormRadioSearchGroupProps, 'name' | 'value' | 'id'> {
  name: Path<TFormValues>;
  id?: string;
  showError?: boolean;
}

export default function RHFFormFieldRadioSearchGroup<
  TFormValues extends FieldValues,
>({
  name,
  id: customId,
  ...props
}: RHFFormFieldRadioSearchGroupProps<TFormValues>) {
  const { register } = useFormContext<TFormValues>();
  const { ref: inputRef, ...field } = useRegister(name, register);
  const { fieldId } = useFormIdContext();
  const id = fieldId(name, customId);
  const selectedValue = useWatch({ name });
  const { onChange } = props;

  return (
    <FormRadioSearchGroup
      {...props}
      {...field}
      id={id}
      inputRef={inputRef}
      value={selectedValue}
      onChange={(event, option) => {
        onChange?.(event, option);
        if (!event.isDefaultPrevented()) {
          field.onChange(event);
        }
      }}
    />
  );
}
