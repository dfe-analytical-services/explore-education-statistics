import ErrorSummary, {
  ErrorSummaryMessage,
} from '@common/components/ErrorSummary';
import { FormIdContextProvider } from '@common/components/form/contexts/FormIdContext';
import createErrorHelper from '@common/components/form/validation/createErrorHelper';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import { isEqual } from 'lodash';
import camelCase from 'lodash/camelCase';
import React, {
  FormEvent,
  ReactNode,
  useCallback,
  useEffect,
  useMemo,
  useRef,
} from 'react';
import { FieldValues, Path, useFormContext, useWatch } from 'react-hook-form';

interface Props<TFormValues extends FieldValues> {
  children: ReactNode;
  id: string;
  initialTouched?: Path<TFormValues>[];
  noValidate?: boolean;
  submitId?: string;
  showErrorSummary?: boolean;
  visuallyHiddenErrorSummary?: boolean;
  onChange?: (values: Partial<TFormValues>) => Promise<void> | void;
  onSubmit: (values: TFormValues) => Promise<void> | void;
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
export default function Form<TFormValues extends FieldValues>({
  children,
  id,
  initialTouched,
  noValidate = true,
  submitId = `${id}-submit`,
  showErrorSummary = true,
  visuallyHiddenErrorSummary = false,
  onChange,
  onSubmit,
}: Props<TFormValues>) {
  const isMounted = useMountedRef();

  const {
    formState: {
      errors,
      submitCount,
      touchedFields,
      isSubmitted,
      isSubmitting,
    },
    handleSubmit: submit,
  } = useFormContext<TFormValues>();

  const values = useWatch<TFormValues>();
  const previousValues = useRef(values);
  const previousSubmitCount = useRef(submitCount);

  useEffect(() => {
    if (!isEqual(previousValues.current, values)) {
      onChange?.(values);
    }
  }, [previousValues, values, onChange]);

  const { getAllErrors } = createErrorHelper({
    errors,
    initialTouched,
    isSubmitted,
    touchedFields,
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
  }, [isSubmitted, submitCount, values, isMounted]);

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

      if (isSubmitting) {
        return;
      }

      toggleSummaryFocus.off();

      await submit(async data => onSubmit(data))(event);
    },
    [isSubmitting, toggleSummaryFocus, submit, onSubmit],
  );

  return (
    <FormIdContextProvider id={id}>
      <form id={id} noValidate={noValidate} onSubmit={handleSubmit}>
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
}
