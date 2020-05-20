import useFormSubmit from '@admin/hooks/useFormSubmit';
import Button from '@common/components/Button';
import { Form, FormFieldTextInput, FormGroup } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

interface Props {
  initialValues?: DataBlockDetailsFormValues;
  onTitleChange?: (title: string) => void;
  onSubmit: (dataBlock: DataBlockDetailsFormValues) => void;
}

export interface DataBlockDetailsFormValues {
  heading: string;
  source: string;
  name: string;
}

const DataBlockDetailsForm = ({
  initialValues = { heading: '', name: '', source: '' },
  onTitleChange,
  onSubmit,
}: Props) => {
  const handleSubmit = useFormSubmit(onSubmit, []);

  return (
    <Formik<DataBlockDetailsFormValues>
      initialValues={initialValues}
      validationSchema={Yup.object<DataBlockDetailsFormValues>({
        name: Yup.string().required('Enter a data block name'),
        heading: Yup.string().required('Enter a table title'),
        source: Yup.string(),
      })}
      onSubmit={handleSubmit}
    >
      {form => {
        return (
          <Form {...form} id="dataBlockDetails">
            <h2>Data block details</h2>

            <FormGroup>
              <FormFieldTextInput<DataBlockDetailsFormValues>
                id="data-block-name"
                name="name"
                label="Name"
                hint="Name of the data block"
                className="govuk-!-width-one-half"
              />

              <FormFieldTextArea<DataBlockDetailsFormValues>
                name="heading"
                id="data-block-title"
                className="govuk-!-width-two-thirds"
                label="Table title"
                rows={2}
                onChange={e => {
                  if (onTitleChange) onTitleChange(e.target.value);
                }}
              />

              <FormFieldTextInput<DataBlockDetailsFormValues>
                id="data-block-source"
                name="source"
                label="Source"
                className="govuk-!-width-two-thirds"
              />

              <Button
                type="submit"
                className="govuk-!-margin-top-6"
                disabled={form.isSubmitting}
              >
                Save data block
              </Button>
            </FormGroup>
          </Form>
        );
      }}
    </Formik>
  );
};

export default DataBlockDetailsForm;
