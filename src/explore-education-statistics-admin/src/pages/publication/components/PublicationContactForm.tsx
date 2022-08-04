import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import ModalConfirm from '@common/components/ModalConfirm';
import useFormSubmit from '@common/hooks/useFormSubmit';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

export interface PublicationContactFormValues {
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo: string;
}

interface Props {
  initialValues: PublicationContactFormValues;
  onCancel: () => void;
  onSubmit: (values: PublicationContactFormValues) => void;
}

const PublicationContactForm = ({
  initialValues,
  onCancel,
  onSubmit,
}: Props) => {
  const [showConfirmModal, toggleConfirmModal] = useToggle(false);

  const validationSchema = Yup.object<PublicationContactFormValues>({
    teamName: Yup.string().required('Enter a team name'),
    teamEmail: Yup.string()
      .required('Enter a team email')
      .email('Enter a valid team email'),
    contactName: Yup.string().required('Enter a contact name'),
    contactTelNo: Yup.string().required('Enter a contact telephone'),
  });

  return (
    <Formik<PublicationContactFormValues>
      enableReinitialize
      initialValues={initialValues}
      validationSchema={validationSchema}
      onSubmit={useFormSubmit(onSubmit)}
    >
      {form => (
        <>
          <Form id="publicationContactForm">
            <FormFieldTextInput<PublicationContactFormValues>
              name="teamName"
              label="Team name"
              className="govuk-!-width-one-half"
            />

            <FormFieldTextInput<PublicationContactFormValues>
              name="teamEmail"
              label="Team email"
              className="govuk-!-width-one-half"
            />

            <FormFieldTextInput<PublicationContactFormValues>
              name="contactName"
              label="Contact name"
              className="govuk-!-width-one-half"
            />

            <FormFieldTextInput<PublicationContactFormValues>
              name="contactTelNo"
              label="Contact telephone"
              className="govuk-!-width-one-half"
            />

            <ButtonGroup>
              <Button
                type="submit"
                onClick={async e => {
                  e.preventDefault();
                  if (form.isValid) {
                    toggleConfirmModal.on();
                  } else {
                    await form.submitForm();
                  }
                }}
              >
                Update contact details
              </Button>
              <ButtonText onClick={onCancel}>Cancel</ButtonText>
            </ButtonGroup>
          </Form>

          <ModalConfirm
            title="Confirm contact changes"
            onConfirm={async () => {
              await form.submitForm();
            }}
            onExit={toggleConfirmModal.off}
            onCancel={toggleConfirmModal.off}
            open={showConfirmModal}
          >
            <p>
              Any changes made here will appear on the public site immediately.
            </p>
            <p>Are you sure you want to save the changes?</p>
          </ModalConfirm>
        </>
      )}
    </Formik>
  );
};

export default PublicationContactForm;
