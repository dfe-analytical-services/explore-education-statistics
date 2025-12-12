import FormColourInput, {
  FormColourInputProps,
} from '@common/components/form/FormColourInput';
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import React from 'react';
import { FieldValues, Path, PathValue, useFormContext } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormColourInputProps,
  TFormValues
>;

export default function FormFieldColourInput<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  const { getValues, setValue } = useFormContext<TFormValues>();
  const { name } = props;
  const value = getValues(name);
  return (
    <FormField
      {...props}
      initialValue={value}
      as={FormColourInput}
      onConfirm={(updatedValue: string) =>
        setValue(
          name,
          updatedValue as PathValue<TFormValues, Path<TFormValues>>,
        )
      }
    />
  );
}
