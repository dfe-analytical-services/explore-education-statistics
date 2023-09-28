import publicationService from '@admin/services/publicationService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { ReactNode, useMemo } from 'react';
import { ObjectSchema } from 'yup';

interface FormValues {
  title: string;
  summary: string;
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo?: string;
  supersededById?: string;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'title',
    messages: {
      SlugNotUnique: 'Choose a unique title',
    },
  }),
];

interface Props {
  cancelButton?: ReactNode;
  id?: string;
  topicId: string;
  onSubmit: () => void | Promise<void>;
}

export default function PublicationForm({
  cancelButton,
  id = 'publicationForm',
  topicId,
  onSubmit,
}: Props) {
  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      title: Yup.string().required('Enter a publication title'),
      summary: Yup.string()
        .required('Enter a publication summary')
        .max(160, 'Summary must be 160 characters or less'),
      teamName: Yup.string().required('Enter a team name'),
      teamEmail: Yup.string()
        .required('Enter a team email address')
        .email('Enter a valid team email address'),
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
      supersededById: Yup.string(),
    });
  }, []);

  const handleSubmit = async ({
    contactName,
    contactTelNo,
    summary,
    teamEmail,
    teamName,
    title,
  }: FormValues) => {
    const contact = {
      teamName,
      teamEmail,
      contactName,
      contactTelNo,
    };

    if (!contact.contactTelNo?.trim()) {
      contact.contactTelNo = undefined;
    }

    await publicationService.createPublication({
      summary,
      title,
      topicId,
      contact,
    });

    await onSubmit();
  };

  return (
    <FormProvider
      enableReinitialize
      initialValues={{
        title: '',
        summary: '',
        teamName: '',
        teamEmail: '',
        contactName: '',
        contactTelNo: '',
      }}
      validationSchema={validationSchema}
    >
      <RHFForm
        id={id}
        showSubmitError
        onSubmit={handleSubmit}
        errorMappers={errorMappings}
      >
        <RHFFormFieldTextInput<FormValues>
          label="Publication title"
          name="title"
          className="govuk-!-width-two-thirds"
        />

        <RHFFormFieldTextArea<FormValues>
          label="Publication summary"
          name="summary"
          className="govuk-!-width-one-half"
          maxLength={160}
        />

        <FormFieldset
          id="contact"
          legend="Contact for this publication"
          legendSize="m"
          hint="They will be the main point of contact for data and methodology enquiries for this publication and its releases."
        >
          <RHFFormFieldTextInput<FormValues>
            name="teamName"
            label="Team name"
            className="govuk-!-width-one-half"
          />

          <RHFFormFieldTextInput<FormValues>
            name="teamEmail"
            label="Team email address"
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
            width={10}
          />
        </FormFieldset>

        <ButtonGroup>
          <Button type="submit">Save publication</Button>

          {cancelButton}
        </ButtonGroup>
      </RHFForm>
    </FormProvider>
  );
}
