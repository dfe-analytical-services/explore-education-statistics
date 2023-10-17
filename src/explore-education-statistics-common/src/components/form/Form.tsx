import ErrorSummary, {
  ErrorSummaryMessage,
} from '@common/components/ErrorSummary';
import { FormIdContextProvider } from '@common/components/form/contexts/FormIdContext';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import createErrorHelper from '@common/validation/createErrorHelper';
import { useFormikContext } from 'formik';
import camelCase from 'lodash/camelCase';
import React, {
  FormEvent,
  ReactNode,
  useCallback,
  useEffect,
  useMemo,
  useRef,
} from 'react';

interface Props {
  children: ReactNode;
  id: string;
  submitId?: string;
  showErrorSummary?: boolean;
  visuallyHiddenErrorSummary?: boolean;
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
  showErrorSummary = true,
  submitId = `${id}-submit`,
  visuallyHiddenErrorSummary = false,
}: Props) => {
  const isMounted = useMountedRef();

  const formik = useFormikContext();
  const { errors, touched, values, submitCount, submitForm } = formik;

  const previousValues = useRef(values);
  const previousSubmitCount = useRef(submitCount);

  const { getAllErrors } = createErrorHelper({
    errors,
    touched,
    submitCount,
  });

  const [hasSummaryFocus, toggleSummaryFocus] = useToggle(false);

  const allErrors = useMemo<ErrorSummaryMessage[]>(() => {
    return Object.entries(getAllErrors()).map(([field, message]) => {
      return {
        id:
          field !== 'root' && !field.startsWith('root.')
            ? `${id}-${camelCase(field)}`
            : submitId,
        message: typeof message === 'string' ? message : '',
      };
    });
  }, [getAllErrors, id, submitId]);

  useEffect(() => {
    if (!isMounted.current) {
      return;
    }

    previousValues.current = values;
  }, [submitCount, values, isMounted]);

  useEffect(() => {
    if (!isMounted.current) {
      return;
    }

    if (allErrors.length && submitCount !== previousSubmitCount.current) {
      toggleSummaryFocus.on();
      previousSubmitCount.current = submitCount;
    }
  }, [allErrors, isMounted, submitCount, toggleSummaryFocus]);

  const handleSubmit = useCallback(
    async (event: FormEvent) => {
      event.preventDefault();
      toggleSummaryFocus.off();

      await submitForm();
    },
    [toggleSummaryFocus, submitForm],
  );

  return (
    <FormIdContextProvider id={id}>
      <form id={id} onSubmit={handleSubmit}>
        {showErrorSummary && (
          <ErrorSummary
            errors={allErrors}
            focusOnError={hasSummaryFocus && !visuallyHiddenErrorSummary}
            visuallyHidden={visuallyHiddenErrorSummary}
            onFocus={toggleSummaryFocus.off}
          />
        )}

        {children}
      </form>
    </FormIdContextProvider>
  );
};

export default Form;
