import { FormFieldset, FormGroup } from '@common/components/form';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import { ExternalMethodology } from '@admin/services/publicationService';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';

interface Props {
  initialValues?: ExternalMethodology;
  onCancel: () => void;
  onSubmit: (values: ExternalMethodology) => void;
}

interface FormValues {
  title: string;
  url: string;
}

const ExternalMethodologyForm = ({
  initialValues,
  onCancel,
  onSubmit,
}: Props) => {
  const validationSchema = useMemo(() => {
    const schema = Yup.object<FormValues>({
      title: Yup.string().required('Enter an external methodology link title'),
      url: Yup.string()
        .required('Enter an external methodology URL')
        .url('Enter a valid external methodology URL')
        .test({
          name: 'currentHostUrl',
          message: 'External methodology URL cannot be for this website',
          test: (value: string) =>
            Boolean(value && !value.includes(window.location.host)),
        }),
    });
    return schema;
  }, []);

  return (
    <FormProvider
      enableReinitialize
      initialValues={
        initialValues ?? {
          title: '',
          url: 'https://',
        }
      }
      validationSchema={validationSchema}
    >
      <RHFForm id="methodology-external" onSubmit={onSubmit}>
        <FormFieldset
          id="methodology-external-fieldset"
          legend="Link to an externally hosted methodology"
          legendHidden
        >
          <FormGroup>
            <RHFFormFieldTextInput
              label="Link title"
              name="title"
              className="govuk-!-width-two-thirds"
            />
            <RHFFormFieldTextInput
              label="URL"
              name="url"
              className="govuk-!-width-two-thirds"
            />
          </FormGroup>
          <ButtonGroup>
            <Button type="submit">Save</Button>
            <Button
              type="reset"
              variant="secondary"
              onClick={() => {
                onCancel();
              }}
            >
              Cancel
            </Button>
          </ButtonGroup>
        </FormFieldset>
      </RHFForm>
    </FormProvider>
  );
};

export default ExternalMethodologyForm;
