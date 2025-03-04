import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormProvider from '@common/components/form/FormProvider';
import Modal from '@common/components/Modal';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode } from 'react';
import { FieldValues, UseFormProps } from 'react-hook-form';
import { ObjectSchema, Schema } from 'yup';

interface Props<TFormValues extends FieldValues> {
  children: ReactNode;
  formId: string;
  title: string;
  triggerButton: ReactNode;
  initialValues?: UseFormProps<TFormValues>['defaultValues'];
  validationSchema?: ObjectSchema<TFormValues> & Schema<TFormValues>;
  submitText?: string;
  cancelText?: string;
  onSubmit: (formValues: TFormValues) => Promise<void>;
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
  onSubmit,
}: Props<TFormValues>) {
  const isMounted = useMountedRef();

  const [open, toggleOpen] = useToggle(false);
  const [isSubmitting, toggleSubmitting] = useToggle(false);

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
      open={open}
      closeOnOutsideClick={!isSubmitting}
      closeOnEsc={!isSubmitting}
      title={title}
      triggerButton={triggerButton}
      onExit={toggleOpen.off}
      onToggleOpen={toggleOpen}
    >
      <FormProvider
        initialValues={initialValues}
        validationSchema={validationSchema}
      >
        <Form id={formId} onSubmit={handleSubmit}>
          {children}
          <ButtonGroup>
            <Button
              type="submit"
              className="govuk-button govuk-!-margin-right-1"
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
          </ButtonGroup>
        </Form>
      </FormProvider>
    </Modal>
  );
}
