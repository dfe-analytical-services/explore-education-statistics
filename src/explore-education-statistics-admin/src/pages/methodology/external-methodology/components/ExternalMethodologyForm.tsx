import { FormFieldset, FormGroup } from '@common/components/form';
import Form from '@common/components/form/Form';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { ExternalMethodology } from '@admin/services/publicationService';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useMemo } from 'react';

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
    <Formik<ExternalMethodology>
      enableReinitialize
      initialValues={
        initialValues ?? {
          title: '',
          url: 'https://',
        }
      }
      onSubmit={values => {
        onSubmit(values);
      }}
      validationSchema={validationSchema}
    >
      {form => (
        <Form id="methodology-external">
          <FormFieldset
            id="methodology-external-fieldset"
            legend="Link to an externally hosted methodology"
            legendHidden
          >
            <FormGroup>
              <FormFieldTextInput
                label="Link title"
                name="title"
                className="govuk-!-width-two-thirds"
              />
              <FormFieldTextInput
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
                  form.resetForm();
                  onCancel();
                }}
              >
                Cancel
              </Button>
            </ButtonGroup>
          </FormFieldset>
        </Form>
      )}
    </Formik>
  );
};

export default ExternalMethodologyForm;
