import publicationService from '@admin/services/publicationService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { ReactNode, useMemo } from 'react';
import { ObjectSchema } from 'yup';

const titleMaxLength = 65;
const summaryMaxLength = 160;

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
      PublicationSlugNotUnique: 'Choose a unique title',
      PublicationSlugUsedByRedirect: 'Choose a previously unused title',
      MethodologySlugNotUnique:
        'This title also changes the methodology title to one used by an existing methodology. Choose a unique methodology title',
      MethodologySlugUsedByRedirect:
        'This title also changes the methodology title to one that has been live previously. Choose a unique methodology title',
    },
  }),
];

interface Props {
  cancelButton?: ReactNode;
  id?: string;
  themeId: string;
  onSubmit: () => void | Promise<void>;
}

export default function PublicationForm({
  cancelButton,
  id = 'publicationForm',
  themeId,
  onSubmit,
}: Props) {
  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      title: Yup.string()
        .required('Enter a publication title')
        .max(
          titleMaxLength,
          `Title must be ${titleMaxLength} characters or fewer`,
        ),
      summary: Yup.string()
        .required('Enter a publication summary')
        .max(
          summaryMaxLength,
          `Summary must be ${summaryMaxLength} characters or fewer`,
        ),
      teamName: Yup.string().required('Enter a team name'),
      teamEmail: Yup.string()
        .required('Enter a team email address')
        .email('Enter a valid team email address'),
      contactName: Yup.string().required('Enter a contact name'),
      contactTelNo: Yup.string()
        .matches(/^0[0-9\s]*$/, {
          excludeEmptyString: true,
          message:
            'Contact telephone must start with a "0" and only contain numeric or whitespace characters',
        })
        .matches(
          /^(?!^0\s*3\s*7\s*0\s*0\s*0\s*0\s*2\s*2\s*8\s*8$)/,
          'Contact telephone cannot be the DfE enquiries number',
        )
        .matches(/.{8,}/, {
          excludeEmptyString: true,
          message: 'Contact telephone must be 8 characters or more',
        }),
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

    if (!contact.contactTelNo) {
      contact.contactTelNo = undefined;
    }

    await publicationService.createPublication({
      summary,
      title,
      themeId,
      contact,
    });

    await onSubmit();
  };

  return (
    <FormProvider
      enableReinitialize
      errorMappings={errorMappings}
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
      <Form id={id} onSubmit={handleSubmit}>
        <FormFieldTextInput<FormValues>
          label="Publication title"
          name="title"
          className="govuk-!-width-two-thirds"
          maxLength={titleMaxLength}
        />

        <FormFieldTextArea<FormValues>
          label="Publication summary"
          name="summary"
          className="govuk-!-width-one-half"
          maxLength={summaryMaxLength}
        />

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
            label="Team email address"
            className="govuk-!-width-one-half"
          />

          <FormFieldTextInput<FormValues>
            name="contactName"
            label="Contact name"
            className="govuk-!-width-one-half"
          />

          <FormFieldTextInput<FormValues>
            name="contactTelNo"
            label="Contact telephone (optional)"
            width={10}
          />
        </FormFieldset>

        <ButtonGroup>
          <Button type="submit">Save publication</Button>

          {cancelButton}
        </ButtonGroup>
      </Form>
    </FormProvider>
  );
}
