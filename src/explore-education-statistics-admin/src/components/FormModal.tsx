import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormProvider from '@common/components/form/FormProvider';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Modal from '@common/components/Modal';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import { FieldMessageMapper } from '@common/validation/serverValidations';
import React, { ReactNode, useState } from 'react';
import { DeepPartial, FieldValues } from 'react-hook-form';
import { ObjectSchema, Schema } from 'yup';

interface Props<TFormValues extends FieldValues> {
  children?: ReactNode;
  className?: string;
  underlayClass?: string;
  formId: string;
  title: string;
  triggerButton: ReactNode;
  initialValues?: TFormValues;
  validationSchema?: ObjectSchema<TFormValues> & Schema<TFormValues>;
  errorMappings?:
    | FieldMessageMapper<TFormValues>[]
    | ((values: TFormValues) => FieldMessageMapper<TFormValues>[]);
  submitText?: string;
  cancelText?: string;
  hiddenSubmittingText?: string;
  onSubmit: (formValues: TFormValues) => Promise<void>;
  withConfirmationWarning?: boolean;
  confirmationWarningText?:
    | ReactNode
    | ((formValues?: TFormValues) => ReactNode);
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

    if (isMounted.current) {
      toggleSubmitting.off();
      toggleOpen.off();
    }
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
      onExit={toggleOpen.off}
      onToggleOpen={toggleOpen}
    >
      <FormProvider
        initialValues={formValues as DeepPartial<TFormValues>}
        validationSchema={validationSchema}
        errorMappings={errorMappings}
        mode="onBlur"
      >
        {({ formState }) => {
          if (!formState.isValid) {
            toggleSubmitting.off();
            toggleConfirmationWarning.off();
          }

          return (
            <Form id={formId} onSubmit={handleSubmit} onChange={onChange}>
              {!confirmationWarning && children}
              {confirmationWarning && <>{confirmationWarningTextWrapper()}</>}
              <ButtonGroup>
                {confirmationWarning ? (
                  <>
                    <Button
                      type="submit"
                      className="govuk-button govuk-button--warning govuk-!-margin-right-1"
                      disabled={isSubmitting}
                    >
                      Confirm
                    </Button>
                    <Button
                      className="govuk-button govuk-button--secondary"
                      variant="secondary"
                      onClick={toggleConfirmationWarning.off}
                      disabled={isSubmitting}
                    >
                      Cancel
                    </Button>
                  </>
                ) : (
                  <>
                    <Button
                      type={withConfirmationWarning ? 'button' : 'submit'}
                      className="govuk-button govuk-!-margin-right-1"
                      onClick={e => {
                        if (
                          !formState.isValid ||
                          Object.keys(formState.errors).length
                        ) {
                          return;
                        }

                        if (withConfirmationWarning) {
                          e.preventDefault();
                          toggleConfirmationWarning.on();
                        }
                      }}
                      disabled={isSubmitting}
                    >
                      {submitText}
                    </Button>
                    <Button
                      className="govuk-button govuk-button--secondary"
                      variant="secondary"
                      onClick={toggleOpen.off}
                      disabled={isSubmitting}
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
          );
        }}
      </FormProvider>
    </Modal>
  );
}
