import createErrorHelper from '@common/lib/validation/createErrorHelper';
import {
  connect,
  FieldArray,
  FieldArrayRenderProps,
  FormikContext,
} from 'formik';
import get from 'lodash/get';
import React, { ReactNode } from 'react';

interface InjectedProps<FormValues> extends FieldArrayRenderProps {
  error: string;
  value: FormValues[keyof FormValues];
}

interface FieldCheckboxArrayProps<FormValues> {
  children: (props: InjectedProps<FormValues>) => ReactNode;
  error?: string;
  name: string;
  showError?: boolean;
  validateOnChange?: boolean;
}

const FieldCheckboxArray = <T extends {}>({
  children,
  error,
  name,
  formik,
  showError = true,
  validateOnChange = false,
}: FieldCheckboxArrayProps<T> & { formik: FormikContext<T> }) => {
  return (
    <FieldArray
      name={name}
      validateOnChange={validateOnChange || formik.validateOnChange}
    >
      {({ form, ...arrayHelpers }) => {
        const { getError } = createErrorHelper(form);

        let errorMessage = error || getError(name);

        if (!showError) {
          errorMessage = '';
        }

        const value = get(form.values, name);

        return children({
          ...arrayHelpers,
          error: errorMessage,
          form,
          name,
          value,
        });
      }}
    </FieldArray>
  );
};

export default connect<FieldCheckboxArrayProps<{}>>(FieldCheckboxArray);
