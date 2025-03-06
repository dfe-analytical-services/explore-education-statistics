import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormProvider from '@common/components/form/FormProvider';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Modal from '@common/components/Modal';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode, useState } from 'react';
import { DeepPartial, FieldValues } from 'react-hook-form';
import { ObjectSchema, Schema } from 'yup';

interface Props<TFormValues extends FieldValues> {
  children: ReactNode;
  formId: string;
  title: string;
  triggerButton: ReactNode;
  initialValues?: TFormValues;
  validationSchema?: ObjectSchema<TFormValues> & Schema<TFormValues>;
  submitText?: string;
  cancelText?: string;
  hiddenSubmittingText?: string;
  onSubmit: (formValues: TFormValues) => Promise<void>;
  withConfirmationWarning?: boolean;
  confirmationWarningText?:
    | ReactNode
    | ((formValues?: TFormValues) => ReactNode);
}

function sleep(ms: number) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

export default function FormModal<TFormValues extends FieldValues>({
  children,
  formId,
  title,
  triggerButton,
  initialValues,
  validationSchema,
  submitText = 'Save',
  cancelText = 'Cancel',
  hiddenSubmittingText = 'Submitting',
  onSubmit,
  withConfirmationWarning = false,
  confirmationWarningText,
}: Props<TFormValues>) {
  const isMounted = useMountedRef();

  const [open, toggleOpen] = useToggle(false);
  const [confirmationWarning, toggleConfirmationWarning] = useToggle(false);
  const [isSubmitting, toggleSubmitting] = useToggle(false);

  const [formValues, setFormValues] = useState(initialValues);

  const confirmationWarningTextWrapper = (): ReactNode => {
    if (typeof confirmationWarningText === 'function') {
      return confirmationWarningText(formValues);
    }

    return confirmationWarningText;
  };

  const onChange = (values: Partial<TFormValues>) => {
    setFormValues(values as TFormValues);
  };

  const handleSubmit = async (values: TFormValues) => {
    if (isSubmitting || !isMounted.current) {
      return;
    }

    toggleSubmitting.on();

    await onSubmit(values);

    await sleep(10000);

    if (isMounted.current) {
      toggleSubmitting.off();
      toggleOpen.off();
    }
  };

  return (
    <Modal
      open={open}
      closeOnOutsideClick={!isSubmitting}
      closeOnEsc={!isSubmitting}
      title={title}
      triggerButton={triggerButton}
      onExit={toggleOpen.off}
      onToggleOpen={toggleOpen}
    >
      <FormProvider
        initialValues={formValues as DeepPartial<TFormValues>}
        validationSchema={validationSchema}
      >
        <Form id={formId} onSubmit={handleSubmit} onChange={onChange}>
          {children}
          {confirmationWarning && <>{confirmationWarningTextWrapper()}</>}
          <ButtonGroup>
            {confirmationWarning ? (
              <>
                <Button
                  type="submit"
                  className="govuk-button govuk-button--warning govuk-!-margin-right-1"
                >
                  Confirm
                </Button>
                <Button
                  className="govuk-button govuk-button--secondary"
                  variant="secondary"
                  onClick={toggleConfirmationWarning.off}
                >
                  Abort
                </Button>
              </>
            ) : (
              <>
                <Button
                  type={withConfirmationWarning ? 'button' : 'submit'}
                  className="govuk-button govuk-!-margin-right-1"
                  onClick={
                    withConfirmationWarning
                      ? toggleConfirmationWarning.on
                      : undefined
                  }
                >
                  {submitText}
                </Button>
                <Button
                  className="govuk-button govuk-button--secondary"
                  variant="secondary"
                  onClick={toggleOpen.off}
                >
                  {cancelText}
                </Button>
              </>
            )}
            <LoadingSpinner
              alert
              inline
              hideText
              loading={isSubmitting}
              size="sm"
              text={hiddenSubmittingText}
            />
          </ButtonGroup>
        </Form>
      </FormProvider>
    </Modal>
  );
}
