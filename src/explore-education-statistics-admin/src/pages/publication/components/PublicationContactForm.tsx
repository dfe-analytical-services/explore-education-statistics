import publicationService, {
  Contact,
} from '@admin/services/publicationService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

interface FormValues {
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo?: string;
}

interface Props {
  initialValues: FormValues;
  publicationId: string;
  onCancel: () => void;
  onSubmit: (nextContact: Contact) => Promise<void> | void;
}

export default function PublicationContactForm({
  initialValues,
  publicationId,
  onCancel,
  onSubmit,
}: Props) {
  const [showConfirmModal, toggleConfirmModal] = useToggle(false);

  const handleSubmit = async (values: FormValues) => {
    const contact = values;

    if (!contact.contactTelNo?.trim()) {
      contact.contactTelNo = undefined;
    }

    const nextContact = await publicationService.updateContact(
      publicationId,
      contact,
    );

    await onSubmit(nextContact);
  };

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      teamName: Yup.string().required('Enter a team name'),
      teamEmail: Yup.string()
        .required('Enter a team email')
        .email('Enter a valid team email'),
      contactName: Yup.string().required('Enter a contact name'),
      contactTelNo: Yup.string()
        .trim()
        .matches(
          /^0[0-9\s]*$/,
          'Contact telephone must start with a "0" and only contain numeric or whitespace characters',
        )
        .matches(
          /^(?!^0\s*3\s*7\s*0\s*0\s*0\s*0\s*2\s*2\s*8\s*8$)/,
          'Contact telephone cannot be the DfE enquiries number',
        )
        .min(8, 'Contact telephone must be 8 characters or more'),
    });
  }, []);

  return (
    <FormProvider
      initialValues={initialValues}
      validationSchema={validationSchema}
    >
      {({ getValues }) => {
        return (
          <>
            <RHFForm
              id="publicationContactForm"
              onSubmit={async () => toggleConfirmModal.on()}
            >
              <RHFFormFieldTextInput<FormValues>
                name="teamName"
                label="Team name"
                className="govuk-!-width-one-half"
              />

              <RHFFormFieldTextInput<FormValues>
                name="teamEmail"
                label="Team email"
                className="govuk-!-width-one-half"
              />

              <RHFFormFieldTextInput<FormValues>
                name="contactName"
                label="Contact name"
                className="govuk-!-width-one-half"
              />

              <RHFFormFieldTextInput<FormValues>
                name="contactTelNo"
                label="Contact telephone (optional)"
                className="govuk-!-width-one-half"
              />

              <ButtonGroup>
                <Button type="submit">Update contact details</Button>
                <ButtonText onClick={onCancel}>Cancel</ButtonText>
              </ButtonGroup>
            </RHFForm>

            <ModalConfirm
              title="Confirm contact changes"
              onConfirm={async () => {
                const values = getValues();
                await handleSubmit(values);
              }}
              onExit={toggleConfirmModal.off}
              onCancel={toggleConfirmModal.off}
              open={showConfirmModal}
            >
              <p>
                Any changes made here will appear on the public site
                immediately.
              </p>
              <p>Are you sure you want to save the changes?</p>
            </ModalConfirm>
          </>
        );
      }}
    </FormProvider>
  );
}
