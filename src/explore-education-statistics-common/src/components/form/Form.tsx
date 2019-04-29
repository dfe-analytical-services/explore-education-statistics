import ErrorSummary, {
  ErrorSummaryMessage,
} from '@common/components/ErrorSummary';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { Form as FormikForm, FormikErrors, FormikTouched } from 'formik';
import camelCase from 'lodash/camelCase';
import get from 'lodash/get';
import React, { ReactNode } from 'react';

interface Props<FormValues> {
  children: ReactNode;
  errors: FormikErrors<FormValues>;
  id: string;
  touched: FormikTouched<FormValues>;
  otherErrors?: ErrorSummaryMessage[];
}

const Form = <FormValues extends {}>({
  children,
  errors,
  id,
  touched,
  otherErrors = [],
}: Props<FormValues>) => {
  const { getAllErrors } = createErrorHelper({
    errors,
    touched,
  });

  const summaryErrors: ErrorSummaryMessage[] = Object.entries(getAllErrors())
    .filter(([errorName]) => get(touched, errorName))
    .map(([errorName, message]) => ({
      id: `${id}-${camelCase(errorName)}`,
      message: typeof message === 'string' ? message : '',
    }))
    .concat(...otherErrors);

  return (
    <FormikForm id={id}>
      <ErrorSummary errors={summaryErrors} id={`${id}-summary`} />

      {children}
    </FormikForm>
  );
};

export default Form;
