import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import ModalConfirm from '@common/components/ModalConfirm';
import useFormSubmit from '@common/hooks/useFormSubmit';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

export interface FormValues {
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo: string;
}

interface Props {
  initialValues: FormValues;
  onCancel: () => void;
  onSubmit: (values: FormValues) => void;
}

const PublicationContactForm = ({
  initialValues,
  onCancel,
  onSubmit,
}: Props) => {
  const [showConfirmSubmitModal, toggleShowConfirmSubmitModal] = useToggle(
    false,
  );

  const validationSchema = Yup.object<FormValues>({
    teamName: Yup.string().required('Enter a team name'),
    teamEmail: Yup.string()
      .required('Enter a team email')
      .email('Enter a valid team email'),
    contactName: Yup.string().required('Enter a contact name'),
    contactTelNo: Yup.string().required('Enter a contact telephone'),
  });

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        ...(initialValues ?? {
          teamName: '',
          teamEmail: '',
          contactName: '',
          contactTelNo: '',
        }),
      }}
      validationSchema={validationSchema}
      onSubmit={useFormSubmit((values: FormValues) => {
        onSubmit(values);
      })}
    >
      {form => (
        <>
          <Form id="publicationContactForm">
            <FormFieldset
              id="contact"
              legend="Contact for this publication"
              legendSize="m"
              hint="They will be the main point of contact for data and methodology enquiries for this publication and its releases."
            >
              <FormFieldTextInput<FormValues>
                name="teamName"
                label="Team name"
                className="govuk-!-width-one-half"
              />

              <FormFieldTextInput<FormValues>
                name="teamEmail"
                label="Team email"
                className="govuk-!-width-one-half"
              />

              <FormFieldTextInput<FormValues>
                name="contactName"
                label="Contact name"
                className="govuk-!-width-one-half"
              />

              <FormFieldTextInput<FormValues>
                name="contactTelNo"
                label="Contact telephone"
                className="govuk-!-width-one-half"
              />
            </FormFieldset>

            <ButtonGroup>
              <Button
                type="submit"
                onClick={async e => {
                  e.preventDefault();
                  if (form.isValid) {
                    toggleShowConfirmSubmitModal.on();
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
            onExit={toggleShowConfirmSubmitModal.off}
            onCancel={toggleShowConfirmSubmitModal.off}
            open={showConfirmSubmitModal}
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
