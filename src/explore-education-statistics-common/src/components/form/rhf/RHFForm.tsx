import ErrorSummary, {
  ErrorSummaryMessage,
} from '@common/components/ErrorSummary';
import { FormIdContextProvider } from '@common/components/form/contexts/FormIdContext';
import createRHFErrorHelper from '@common/components/form/rhf/validation/createRHFErrorHelper';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import isErrorLike from '@common/utils/error/isErrorLike';
import camelCase from 'lodash/camelCase';
import isEqual from 'lodash/isEqual';
import React, {
  FormEvent,
  ReactNode,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { FieldValues, useFormContext, useWatch } from 'react-hook-form';

interface Props<TFormValues extends FieldValues> {
  children: ReactNode;
  id: string;
  submitId?: string;
  showErrorSummary?: boolean;
  showSubmitError?: boolean;
  onSubmit: (values: TFormValues) => Promise<void>;
}

/**
 * Form wrapper to integrate with React Hook Form.
 *
 * This provides a bunch of conveniences for displaying errors
 * and linking to them correctly (in error message hrefs) as
 * long as certain conventions are followed.
 *
 * Fields with errors should have ids which share the form's
 * id and camelCased value key in the form
 * e.g. if form id is `timePeriodForm`, then a form value with a
 * key of `startDate` should have a field with an id of
 * `timePeriodForm-startDate`.
 *
 * Additional errors upon submitting the form e.g. from server
 * requests will also be added to the error summary. These
 * will link to the `submitId` prop.
 */
export default function RHFForm<TFormValues extends FieldValues>({
  children,
  id,
  submitId = `${id}-submit`,
  showErrorSummary = true,
  showSubmitError = false,
  onSubmit,
}: Props<TFormValues>) {
  const isMounted = useMountedRef();

  const {
    formState: { errors, submitCount, touchedFields, isSubmitted },
    handleSubmit: submit,
  } = useFormContext();

  const values = useWatch();
  const previousValues = useRef(values);
  const previousSubmitCount = useRef(submitCount);

  const { getAllErrors } = createRHFErrorHelper({
    errors,
    touchedFields,
    isSubmitted,
  });
  const [hasSummaryFocus, toggleSummaryFocus] = useToggle(false);
  const [submitError, setSubmitError] = useState<ErrorSummaryMessage>();

  useEffect(() => {
    if (!isMounted.current) {
      return;
    }

    if (submitError && !isEqual(values, previousValues.current)) {
      setSubmitError(undefined);
    }

    previousValues.current = values;
  }, [isSubmitted, submitError, submitCount, values, isMounted]);

  const allErrors = useMemo(() => {
    const summaryErrors: ErrorSummaryMessage[] = Object.entries(
      getAllErrors(),
    ).map(([errorName, message]) => ({
      id: `${id}-${camelCase(errorName)}`,
      message: typeof message === 'string' ? message : '',
    }));

    return submitError && isSubmitted
      ? [...summaryErrors, submitError]
      : summaryErrors;
  }, [getAllErrors, id, submitError, isSubmitted]);

  useEffect(() => {
    if (
      (isMounted && allErrors.length,
      submitCount !== previousSubmitCount.current)
    ) {
      toggleSummaryFocus.on();
      previousSubmitCount.current = submitCount;
    }
  }, [allErrors, isMounted, submitCount, toggleSummaryFocus]);

  const handleSubmit = useCallback(
    async (event: FormEvent) => {
      event.preventDefault();
      toggleSummaryFocus.off();

      await submit(async () => {
        setSubmitError(undefined);

        try {
          await onSubmit(values as TFormValues);
        } catch (error) {
          if (!error) {
            return;
          }

          if (showSubmitError) {
            if (isMounted.current && isErrorLike(error)) {
              setSubmitError({
                id: submitId,
                message: error.message ?? error,
              });
            }
          } else {
            throw error;
          }
        }
      })(event);
    },
    [
      toggleSummaryFocus,
      submit,
      onSubmit,
      values,
      showSubmitError,
      isMounted,
      submitId,
    ],
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
}
