import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormProvider from '@common/components/form/FormProvider';
import Modal from '@common/components/Modal';
import useMountedRef from '@common/hooks/useMountedRef';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import React, { ReactNode } from 'react';

export type ReleaseLabelFormValues = {
  label?: string;
};

interface Props {
  triggerButton: ReactNode;
  initialValues?: ReleaseLabelFormValues;
  onSubmit: (releaseDetailsFormValues: ReleaseLabelFormValues) => Promise<void>;
}

export default function ReleaseLabelEditModal({
  triggerButton,
  initialValues,
  onSubmit,
}: Props) {
  const isMounted = useMountedRef();

  const [open, toggleOpen] = useToggle(false);
  const [isSaving, toggleSaving] = useToggle(false);

  const handleSubmit = async (values: ReleaseLabelFormValues) => {
    if (isSaving || !isMounted.current) {
      return;
    }

    toggleSaving.on();

    await onSubmit(values);

    if (isMounted.current) {
      toggleSaving.off();
      toggleOpen.off();
    }
  };

  return (
    <Modal
      open={open}
      closeOnOutsideClick={!isSaving}
      closeOnEsc={!isSaving}
      title="Edit release label"
      triggerButton={triggerButton}
      onExit={toggleOpen.off}
      onToggleOpen={toggleOpen}
    >
      <FormProvider
        initialValues={initialValues ?? { label: undefined }}
        validationSchema={Yup.object({
          label: Yup.string().max(
            20,
            /* eslint-disable no-template-curly-in-string */
            'Release label must be no longer than ${max} characters',
          ),
        })}
      >
        <Form id="relatedPageForm" onSubmit={handleSubmit}>
          <FormFieldTextInput<ReleaseLabelFormValues>
            label="Label"
            name="label"
          />
          <ButtonGroup>
            <Button
              type="submit"
              className="govuk-button govuk-!-margin-right-1"
            >
              Save
            </Button>
            <Button
              className="govuk-button govuk-button--secondary"
              variant="secondary"
              onClick={toggleOpen.off}
            >
              Cancel
            </Button>
          </ButtonGroup>
        </Form>
      </FormProvider>
    </Modal>
  );
}
