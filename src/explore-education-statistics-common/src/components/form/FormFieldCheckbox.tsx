import { FormCheckboxProps } from '@common/components/form/FormCheckbox';
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import { FormCheckbox } from '@common/components/form/index';
import classNames from 'classnames';
import React from 'react';

type Props<FormValues> = {
  small?: boolean;
  formGroup?: boolean;
} & FormFieldComponentProps<FormCheckboxProps, FormValues>;

function FormFieldCheckbox<FormValues>({
  small,
  formGroup,
  ...props
}: Props<FormValues>) {
  return (
    <FormField<string> {...props} type="checkbox" formGroup={formGroup}>
      {({ field, id }) => (
        <div
          className={classNames('govuk-checkboxes', {
            'govuk-checkboxes--small': small,
          })}
        >
          <FormCheckbox {...props} {...field} id={id} />
        </div>
      )}
    </FormField>
  );
}

export default FormFieldCheckbox;
