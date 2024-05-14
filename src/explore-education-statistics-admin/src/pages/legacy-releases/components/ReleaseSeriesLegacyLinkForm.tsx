import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import Yup from '@common/validation/yup';
import React, { ReactNode } from 'react';

interface FormValues {
  description: string;
  url: string;
}

const formId = 'releaseSeriesLegacyLinkForm';

interface Props {
  cancelButton?: ReactNode;
  initialValues?: FormValues;
  onSubmit: (values: FormValues) => void;
}

const ReleaseSeriesLegacyLinkForm = ({
  cancelButton,
  initialValues = {
    description: '',
    url: '',
  },
  onSubmit,
}: Props) => {
  return (
    <FormProvider
      initialValues={initialValues}
      validationSchema={Yup.object<FormValues>({
        description: Yup.string().required('Enter a description'),
        url: Yup.string().required('Enter a URL').url('Enter a valid URL'),
      })}
    >
      {({ formState }) => {
        return (
          <Form id={formId} onSubmit={onSubmit}>
            <FormFieldTextInput<FormValues>
              name="description"
              label="Description"
              className="govuk-!-width-two-thirds"
            />

            <FormFieldTextInput<FormValues>
              name="url"
              label="URL"
              className="govuk-!-width-two-thirds"
            />

            <ButtonGroup>
              <Button type="submit" disabled={formState.isSubmitting}>
                Save legacy release
              </Button>
              {cancelButton}
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
};

export default ReleaseSeriesLegacyLinkForm;
