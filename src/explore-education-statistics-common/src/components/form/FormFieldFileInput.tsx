import FormFileInput, {
  FormFileInputProps,
} from '@common/components/form/FormFileInput';
import createErrorHelper from '@common/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React from 'react';
import FormGroup from './FormGroup';

interface Props<FormValues> extends FormFileInputProps {
  name: keyof FormValues & string;
  formGroupClass?: string;
}

const FormFieldFileInput = <T extends {}>({
  name,
  error,
  formGroupClass,
  ...props
}: Props<T>) => {
  return (
    <Field name={name}>
      {({ form }: FieldProps) => {
        const { getError } = createErrorHelper(form);

        const errorMessage = error || getError(name);

        return (
          <FormGroup hasError={!!errorMessage} className={formGroupClass}>
            <FormFileInput
              {...props}
              name={name.toString()}
              onChange={event => {
                if (props.onChange) {
                  props.onChange(event);
                }

                if (event.isDefaultPrevented()) {
                  return;
                }

                const file =
                  event.target.files && event.target.files.length > 0
                    ? event.target.files[0]
                    : null;

                form.setFieldValue(name, file);
              }}
              error={errorMessage}
            />
          </FormGroup>
        );
      }}
    </Field>
  );
};

export default FormFieldFileInput;
