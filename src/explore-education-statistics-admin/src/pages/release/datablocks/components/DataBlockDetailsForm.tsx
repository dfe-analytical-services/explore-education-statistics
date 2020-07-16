import useFormSubmit from '@admin/hooks/useFormSubmit';
import Button from '@common/components/Button';
import {
  Form,
  FormFieldset,
  FormFieldTextInput,
  FormGroup,
  FormFieldTextArea,
  FormFieldCheckbox,
} from '@common/components/form';
import { OmitStrict } from '@common/types';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

interface Props {
  initialValues?: DataBlockDetailsFormValues;
  onTitleChange?: (title: string) => void;
  onSubmit: (dataBlock: DataBlockDetailsFormValues) => void;
}

interface FormValues {
  heading: string;
  isHighlight?: boolean;
  highlightName: string;
  source: string;
  name: string;
}

export type DataBlockDetailsFormValues = OmitStrict<FormValues, 'isHighlight'>;

const formId = 'dataBlockDetailsForm';

const DataBlockDetailsForm = ({
  initialValues = {
    heading: '',
    name: '',
    highlightName: '',
    source: '',
  },
  onTitleChange,
  onSubmit,
}: Props) => {
  const handleSubmit = useFormSubmit<FormValues>(
    ({ highlightName, isHighlight, ...values }) => {
      onSubmit({
        ...values,
        highlightName: isHighlight ? highlightName : '',
      });
    },
    [],
  );

  return (
    <Formik<FormValues>
      initialValues={{
        ...initialValues,
        isHighlight: !!initialValues?.highlightName ?? false,
      }}
      validationSchema={Yup.object<FormValues>({
        name: Yup.string().required('Enter a data block name'),
        isHighlight: Yup.boolean(),
        highlightName: Yup.string().when('isHighlight', {
          is: true,
          then: Yup.string().required('Enter a table highlight name'),
        }),
        heading: Yup.string().required('Enter a table title'),
        source: Yup.string(),
      })}
      onSubmit={handleSubmit}
    >
      {form => {
        return (
          <Form id={formId}>
            <h2>Data block details</h2>

            <FormGroup>
              <FormFieldTextInput<FormValues>
                id={`${formId}-name`}
                name="name"
                label="Name"
                hint="Name of the data block"
                className="govuk-!-width-one-half"
              />

              <FormFieldTextArea<FormValues>
                id={`${formId}-heading`}
                name="heading"
                className="govuk-!-width-two-thirds"
                label="Table title"
                rows={2}
                onChange={e => {
                  if (onTitleChange) onTitleChange(e.target.value);
                }}
              />

              <FormFieldTextInput<FormValues>
                id={`${formId}-source`}
                name="source"
                label="Source"
                className="govuk-!-width-two-thirds"
              />

              <FormFieldset
                id={`${formId}-highlight`}
                legend="Would you like to make this a table highlight?"
                legendSize="s"
                hint="Checking this option will make this table available as a fast track link when the publication is selected via the table builder"
              >
                <FormFieldCheckbox<FormValues>
                  name="isHighlight"
                  id={`${formId}-isHighlight`}
                  label="Set as a table highlight for this publication"
                  conditional={
                    <FormFieldTextInput<FormValues>
                      name="highlightName"
                      id={`${formId}-highlightName`}
                      label="Table highlight name"
                      hint="We will show this name to table builder users"
                      className="govuk-!-width-one-half"
                    />
                  }
                />
              </FormFieldset>

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
