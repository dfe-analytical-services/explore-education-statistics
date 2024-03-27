import RHFFormField, {
  FormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';
import FormCheckbox, {
  FormCheckboxProps,
} from '@common/components/form/FormCheckbox';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import useRegister from '@common/components/form/rhf/hooks/useRegister';
import classNames from 'classnames';
import React from 'react';
import { FieldValues, useFormContext, useWatch } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = {
  name: string;
  small?: boolean;
  formGroup?: boolean;
} & FormFieldComponentProps<FormCheckboxProps, TFormValues>;

export default function RHFFormFieldCheckbox<TFormValues extends FieldValues>({
  formGroup,
  name,
  small,
  ...props
}: Props<TFormValues>) {
  const { register } = useFormContext<TFormValues>();
  const { ref: inputRef, ...field } = useRegister(name, register);
  const { fieldId } = useFormIdContext();
  const id = fieldId(name, props.id);
  const value = useWatch({ name }) || '';

  return (
    <RHFFormField {...props} name={name} formGroup={formGroup}>
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
          value={value}
        />
      </div>
    </RHFFormField>
  );
}
