import ErrorSummary, {
  ErrorSummaryMessage,
} from '@common/components/ErrorSummary';
import { FormIdContextProvider } from '@common/components/form/contexts/FormIdContext';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import isErrorLike from '@common/utils/error/isErrorLike';
import createErrorHelper from '@common/validation/createErrorHelper';
import { useFormikContext } from 'formik';
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
  showErrorSummary?: boolean;
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
  showErrorSummary = true,
  showSubmitError = false,
}: Props) => {
  const isMounted = useMountedRef();

  const formik = useFormikContext();
  const { errors, touched, values, submitCount, submitForm } = formik;

  const previousValues = useRef(values);

  const { getAllErrors } = createErrorHelper({
    errors,
    touched,
  });

  const [hasSummaryFocus, toggleSummaryFocus] = useToggle(false);
  const [submitError, setSubmitError] = useState<ErrorSummaryMessage>();

  useEffect(() => {
    if (!isMounted.current) {
      return;
    }

    // If form has changed at all, we should remove the submit error
    if (!submitCount || !isEqual(values, previousValues.current)) {
      setSubmitError(undefined);
    }

    previousValues.current = values;
  }, [submitError, submitCount, values, isMounted]);

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
      toggleSummaryFocus.off();
      setSubmitError(undefined);
      event.preventDefault();

      try {
        await submitForm();
      } catch (error) {
        if (!error) {
          return;
        }

        if (showSubmitError) {
          if (isMounted.current && isErrorLike(error)) {
            setSubmitError({
              id: submitId,
              message: error.message,
            });
          }
        } else {
          throw error;
        }
      } finally {
        if (isMounted.current) {
          toggleSummaryFocus.on();
        }
      }
    },
    [toggleSummaryFocus, submitForm, showSubmitError, isMounted, submitId],
  );

  return (
    <FormIdContextProvider id={id}>
      <form id={id} onSubmit={handleSubmit}>
        {showErrorSummary && (
          <ErrorSummary
            errors={allErrors}
            id={`${id}-summary`}
            focusOnError={hasSummaryFocus}
            onFocus={toggleSummaryFocus.off}
          />
        )}

        {children}
      </form>
    </FormIdContextProvider>
  );
};

export default Form;
