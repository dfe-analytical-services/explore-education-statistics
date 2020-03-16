import ErrorSummary, {
  ErrorSummaryMessage,
} from '@common/components/ErrorSummary';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { connect, FormikContext } from 'formik';
import camelCase from 'lodash/camelCase';
import get from 'lodash/get';
import isEqual from 'lodash/isEqual';
import React, {
  ReactNode,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';

interface Props {
  children: ReactNode;
  id: string;
  submitId?: string;
  showSubmitError?: boolean;
}

/**
 * Form wrapper to integrate with Formik.
 *
 * This provides a bunch of conveniences for displaying errors
 * and linking to them correctly (in error message hrefs) as
 * long as certain conventions are followed.
 *
 * Fields with errors should have ids which share the form's
 * id and camelCased value key in Formik
 * e.g. if form id is `timePeriodForm`, then a Formik value with a
 * key of `startDate` should have a field with an id of
 * `timePeriodForm-startDate`.
 *
 * Additional errors upon submitting the form e.g. from server
 * requests will also be added to the error summary. These
 * will link to the `submitId` prop.
 */
const Form = ({
  children,
  id,
  submitId = `${id}-submit`,
  formik,
  showSubmitError = false,
}: Props & { formik: FormikContext<{}> }) => {
  const { errors, touched, values, submitCount, submitForm } = formik;

  const previousValues = useRef(values);

  const { getAllErrors } = createErrorHelper({
    errors,
    touched,
  });

  const [submitted, setSubmitted] = useState(false);
  const [submitError, setSubmitError] = useState<ErrorSummaryMessage>();

  useEffect(() => {
    // If form has changed at all, we should remove the submit error
    if (!submitCount || !isEqual(values, previousValues.current)) {
      setSubmitError(undefined);
    }

    previousValues.current = values;
  }, [submitError, submitCount, values]);

  const allErrors = useMemo(() => {
    const summaryErrors: ErrorSummaryMessage[] = Object.entries(getAllErrors())
      .filter(([errorName]) => get(touched, errorName))
      .map(([errorName, message]) => ({
        id: `${id}-${camelCase(errorName)}`,
        message: typeof message === 'string' ? message : '',
      }));

    return submitError ? [...summaryErrors, submitError] : summaryErrors;
  }, [getAllErrors, id, submitError, touched]);

  const handleSubmit = useCallback(
    async event => {
      setSubmitted(true);
      setSubmitError(undefined);
      event.preventDefault();

      try {
        await submitForm();
      } catch (error) {
        if (error) {
          if (showSubmitError) {
            setSubmitError({
              id: submitId,
              message: error.message,
            });
          } else {
            throw error;
          }
        }
      }
    },
    [submitForm, showSubmitError, submitId],
  );

  return (
    <form id={id} onSubmit={handleSubmit}>
      <ErrorSummary
        errors={allErrors}
        id={`${id}-summary`}
        focusOnError={submitted}
      />

      {children}
    </form>
  );
};

export default connect<Props, {}>(Form);
