import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormProvider, {
  FormProviderProps,
} from '@common/components/form/FormProvider';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Modal, { ModalProps } from '@common/components/Modal';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode, useCallback, useState } from 'react';
import { DeepPartial, FieldValues } from 'react-hook-form';

interface Props<TFormValues extends FieldValues>
  extends Pick<
      ModalProps,
      'className' | 'underlayClass' | 'triggerButton' | 'title'
    >,
    Pick<FormProviderProps<TFormValues>, 'validationSchema' | 'errorMappings'> {
  children: ReactNode;
  formId: string;
  initialValues?: TFormValues;
  submitText?: string;
  hiddenSubmittingText?: string;
  onSubmit: (formValues: TFormValues) => Promise<void>;
  confirmationWarningText?:
    | ReactNode
    | ((formValues: TFormValues) => ReactNode);
}

export default function FormModal<TFormValues extends FieldValues>({
  children,
  className,
  underlayClass,
  formId,
  title,
  triggerButton,
  initialValues,
  validationSchema,
  errorMappings,
  submitText = 'Save',
  hiddenSubmittingText = 'Submitting',
  onSubmit,
  confirmationWarningText,
}: Props<TFormValues>) {
  const isMounted = useMountedRef();

  const [open, toggleOpen] = useToggle(false);
  const [showConfirmationWarning, toggleConfirmationWarning] = useToggle(false);
  const [isSubmitting, toggleSubmitting] = useToggle(false);

  const [formValues, setFormValues] = useState(initialValues);

  const formHasConfirmationWarning = !!confirmationWarningText;

  const confirmationWarningTextWrapper = useCallback((): ReactNode => {
    if (typeof confirmationWarningText === 'function') {
      return confirmationWarningText(formValues!);
    }

    return confirmationWarningText;
  }, [formValues, confirmationWarningText]);

  const onChange = useCallback(
    (values: Partial<TFormValues>) => {
      setFormValues(values as TFormValues);
    },
    [setFormValues],
  );

  const handleSubmit = useCallback(
    async (values: TFormValues) => {
      if (formHasConfirmationWarning && !showConfirmationWarning) {
        toggleConfirmationWarning.on();

        return;
      }

      if (isSubmitting || !isMounted.current) {
        return;
      }

      toggleSubmitting.on();

      try {
        await onSubmit(values);
      } catch (exception) {
        toggleSubmitting.off();
        toggleConfirmationWarning.off();

        throw exception;
      }

      if (isMounted.current) {
        toggleSubmitting.off();
        toggleConfirmationWarning.off();
        toggleOpen.off();
      }
    },
    [
      onSubmit,
      isMounted,
      formHasConfirmationWarning,
      showConfirmationWarning,
      isSubmitting,
      toggleSubmitting,
      toggleConfirmationWarning,
      toggleOpen,
    ],
  );

  const onExit = () => {
    if (showConfirmationWarning) {
      toggleConfirmationWarning.off();

      return;
    }

    toggleOpen.off();
    toggleConfirmationWarning.off();
    toggleSubmitting.off();
    setFormValues(initialValues);
  };

  return (
    <Modal
      className={className}
      underlayClass={underlayClass}
      open={open}
      closeOnOutsideClick={!isSubmitting}
      closeOnEsc={!isSubmitting}
      title={title}
      triggerButton={triggerButton}
      onExit={onExit}
      onToggleOpen={toggleOpen}
    >
      <FormProvider
        initialValues={formValues as DeepPartial<TFormValues>}
        validationSchema={validationSchema}
        errorMappings={errorMappings}
        mode="onBlur"
      >
        <Form id={formId} onSubmit={handleSubmit} onChange={onChange}>
          <div role="alert">
            {showConfirmationWarning ? (
              <>{confirmationWarningTextWrapper()}</>
            ) : (
              children
            )}
            <ButtonGroup>
              <Button
                type="submit"
                className="govuk-button govuk-!-margin-right-1"
                disabled={isSubmitting}
              >
                {showConfirmationWarning ? 'Confirm' : submitText}
              </Button>
              <Button
                className="govuk-button govuk-button--secondary"
                variant="secondary"
                onClick={onExit}
                disabled={isSubmitting}
              >
                Cancel
              </Button>
              <LoadingSpinner
                alert
                inline
                hideText
                loading={isSubmitting}
                size="sm"
                text={hiddenSubmittingText}
              />
            </ButtonGroup>
          </div>
        </Form>
      </FormProvider>
    </Modal>
  );
}
