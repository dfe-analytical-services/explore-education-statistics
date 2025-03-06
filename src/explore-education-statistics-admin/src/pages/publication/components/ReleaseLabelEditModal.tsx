import FormModal from '@admin/components/FormModal';
import { useConfig } from '@admin/contexts/ConfigContext';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import UrlContainer from '@common/components/UrlContainer';
import WarningMessage from '@common/components/WarningMessage';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { ReactNode } from 'react';
import { ObjectSchema } from 'yup';

export type ReleaseLabelFormValues = {
  label?: string;
};

interface Props {
  currentReleaseSlug: string;
  publicationSlug: string;
  initialValues?: ReleaseLabelFormValues;
  triggerButton: ReactNode;
  onSubmit: (releaseDetailsFormValues: ReleaseLabelFormValues) => Promise<void>;
}

const validationSchema: ObjectSchema<ReleaseLabelFormValues> = Yup.object({
  label: Yup.string().max(
    20,
    /* eslint-disable no-template-curly-in-string */
    'Release label must be no longer than ${max} characters',
  ),
});

const errorMappings = [
  mapFieldErrors<ReleaseLabelFormValues>({
    target: 'label',
    messages: {
      SlugNotUnique:
        'A release with this label already exists in this publication for the same time period and year. Please choose a unique label.',
      ReleaseUndergoingPublishing:
        'Sorry, this release is currently undergoing publishing. Please wait until this has finished before changing the label.',
      ReleaseSlugUsedByRedirect:
        'This label previously been used by this release. Unfortunately, you cannot choose an historic label. Please choose another one.',
    },
  }),
];

const releaseSlugLabelSuffix = (label?: string) => {
  const lowercaseLabel = label?.toLowerCase();

  return lowercaseLabel ? `-${lowercaseLabel}` : '';
};

export default function ReleaseLabelEditModal({
  currentReleaseSlug,
  publicationSlug,
  initialValues,
  triggerButton,
  onSubmit,
}: Props) {
  const { publicAppUrl } = useConfig();

  const confirmationWarningText = (
    formValues?: ReleaseLabelFormValues,
  ): ReactNode => {
    const currentReleaseSlugLabelSuffix = releaseSlugLabelSuffix(
      initialValues?.label,
    );
    const newReleaseSlugLabelSuffix = releaseSlugLabelSuffix(formValues?.label);

    const newReleaseSlug = currentReleaseSlug.replace(
      new RegExp(`${currentReleaseSlugLabelSuffix}$`),
      newReleaseSlugLabelSuffix,
    );

    return (
      <>
        <WarningMessage className="govuk-!-font-weight-regular">
          Changing this release's label to <strong>{formValues?.label}</strong>{' '}
          will result in the release's public URL changing from
        </WarningMessage>
        <UrlContainer
          id="before-url"
          label="Before URL"
          url={`${publicAppUrl}/find-statistics/${publicationSlug}/${currentReleaseSlug}`}
        />
        to
        <UrlContainer
          id="after-url"
          label="After URL"
          url={`${publicAppUrl}/find-statistics/${publicationSlug}/${newReleaseSlug}`}
        />
        <br />
        <p>Any users visiting the old URL will be redirected to the new one.</p>
        <p>Are you sure you would like to proceed?</p>
      </>
    );
  };

  return (
    <FormModal
      className="govuk-!-width-two-thirds"
      formId="editReleaseLabelForm"
      title="Edit release label"
      triggerButton={triggerButton}
      initialValues={initialValues ?? { label: undefined }}
      validationSchema={validationSchema}
      errorMappings={errorMappings}
      onSubmit={onSubmit}
      withConfirmationWarning
      confirmationWarningText={confirmationWarningText}
    >
      <FormFieldTextInput<ReleaseLabelFormValues> label="Label" name="label" />
    </FormModal>
  );
}
