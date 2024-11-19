import Button from '@common/components/Button';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import { BasicLink } from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import ButtonGroup from '@common/components/ButtonGroup';
import React from 'react';

export type RelatedPageFormValues = Omit<BasicLink, 'id'>;

interface Props {
  initialValues?: RelatedPageFormValues;
  onCancel: () => void;
  onSubmit: (link: RelatedPageFormValues) => Promise<void>;
}

export default function RelatedPageForm({
  initialValues,
  onCancel,
  onSubmit,
}: Props) {
  return (
    <FormProvider
      initialValues={initialValues ?? { description: '', url: '' }}
      validationSchema={Yup.object({
        description: Yup.string().required('Enter a link title'),
        url: Yup.string()
          .url('Enter a valid link URL')
          .required('Enter a link URL'),
      })}
    >
      <Form id="relatedPageForm" onSubmit={onSubmit}>
        <FormFieldTextInput<RelatedPageFormValues>
          label="Title"
          name="description"
        />
        <FormFieldTextInput<RelatedPageFormValues>
          label="Link URL"
          name="url"
        />
        <ButtonGroup>
          <Button type="submit" className="govuk-button govuk-!-margin-right-1">
            Save
          </Button>
          <Button
            className="govuk-button govuk-button--secondary"
            variant="secondary"
            onClick={onCancel}
          >
            Cancel
          </Button>
        </ButtonGroup>
      </Form>
    </FormProvider>
  );
}
