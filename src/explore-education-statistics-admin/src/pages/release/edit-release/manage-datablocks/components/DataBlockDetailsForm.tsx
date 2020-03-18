import useFormSubmit from '@admin/hooks/useFormSubmit';
import Button from '@common/components/Button';
import {
  Form,
  FormFieldTextInput,
  FormGroup,
  Formik,
} from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import Yup from '@common/lib/validation/yup';
import { FormikProps } from 'formik';
import React, { useRef } from 'react';

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
  const formikRef = useRef<Formik<DataBlockDetailsFormValues>>(null);

  const handleSubmit = useFormSubmit(onSubmit, []);

  return (
    <Formik<DataBlockDetailsFormValues>
      ref={formikRef}
      initialValues={initialValues}
      validationSchema={Yup.object<DataBlockDetailsFormValues>({
        name: Yup.string().required('Enter a data block name'),
        heading: Yup.string().required('Enter a table title'),
        source: Yup.string(),
      })}
      onSubmit={handleSubmit}
      render={(form: FormikProps<DataBlockDetailsFormValues>) => {
        return (
          <Form {...form} id="dataBlockDetails">
            <h2>Data block details</h2>

            <FormGroup>
              <FormFieldTextInput<DataBlockDetailsFormValues>
                id="data-block-name"
                name="name"
                label="Data block name"
                hint=" Name and save your data block before viewing it under the
                    'View data blocks' tab at the top of this page."
                percentageWidth="one-half"
              />

              <FormFieldTextArea<DataBlockDetailsFormValues>
                id="data-block-title"
                name="heading"
                label="Table title"
                onChange={e => {
                  if (onTitleChange) onTitleChange(e.target.value);
                }}
                additionalClass="govuk-!-width-two-thirds"
                rows={2}
              />

              <FormFieldTextInput<DataBlockDetailsFormValues>
                id="data-block-source"
                name="source"
                label="Source"
                percentageWidth="two-thirds"
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
    />
  );
};

export default DataBlockDetailsForm;
