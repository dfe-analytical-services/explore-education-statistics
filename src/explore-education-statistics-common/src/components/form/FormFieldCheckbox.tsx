import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormCheckbox, {
  FormCheckboxProps,
} from '@common/components/form/FormCheckbox';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import classNames from 'classnames';
import React from 'react';
import { FieldValues, useFormContext, useWatch } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = {
  name: string;
  small?: boolean;
  formGroup?: boolean;
} & FormFieldComponentProps<FormCheckboxProps, TFormValues>;

export default function FormFieldCheckbox<TFormValues extends FieldValues>({
  formGroup,
  name,
  small,
  ...props
}: Props<TFormValues>) {
  const { register } = useFormContext<TFormValues>();

  // Use standard register instead of `useRegister` as the memoisation
  // there causes the field to not work after reseting the form.
  const { ref: inputRef, ...field } = register(name);
  const { fieldId } = useFormIdContext();
  const id = fieldId(name, props.id);
  const value = useWatch({ name }) || '';

  return (
    <FormField {...props} name={name} formGroup={formGroup}>
      <div
        className={classNames('govuk-checkboxes', {
          'govuk-checkboxes--small': small,
        })}
      >
        <FormCheckbox
          {...props}
          {...field}
          checked={!!value}
          id={id}
          inputRef={inputRef}
        />
      </div>
    </FormField>
  );
}
