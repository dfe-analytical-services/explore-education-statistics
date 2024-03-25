import RHFFormField, {
  FormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';
import RHFFormCheckbox, {
  RHFFormCheckboxProps,
} from '@common/components/form/rhf/RHFFormCheckbox';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import useRegister from '@common/components/form/rhf/hooks/useRegister';
import classNames from 'classnames';
import React from 'react';
import { FieldValues, useFormContext } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = {
  name: string;
  small?: boolean;
  formGroup?: boolean;
} & FormFieldComponentProps<RHFFormCheckboxProps, TFormValues>;

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

  return (
    <RHFFormField {...props} name={name} formGroup={formGroup}>
      <div
        className={classNames('govuk-checkboxes', {
          'govuk-checkboxes--small': small,
        })}
      >
        <RHFFormCheckbox {...props} {...field} id={id} inputRef={inputRef} />
      </div>
    </RHFFormField>
  );
}
