import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldTextInput } from '@common/components/form';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import useFormSubmit from '@common/hooks/useFormSubmit';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { ReactNode } from 'react';

interface FormValues {
  description: string;
  url: string;
  order?: number;
}

const formId = 'legacyReleaseForm';

interface Props {
  cancelButton?: ReactNode;
  initialValues?: FormValues;
  onSubmit: (values: FormValues) => void;
}

const LegacyReleaseForm = ({
  cancelButton,
  initialValues = {
    description: '',
    url: '',
  },
  onSubmit,
}: Props) => {
  const handleSubmit = useFormSubmit(onSubmit);

  return (
    <Formik
      initialValues={initialValues}
      onSubmit={handleSubmit}
      validationSchema={Yup.object<FormValues>({
        description: Yup.string().required('Enter a description'),
        url: Yup.string().required('Enter a URL').url('Enter a valid URL'),
        order:
          typeof initialValues?.order === 'number'
            ? Yup.number()
                .required('Enter an order')
                .min(0, 'Enter an order greater than 0')
            : Yup.number(),
      })}
    >
      {form => (
        <Form id={formId}>
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

          {typeof initialValues?.order !== 'undefined' && (
            <FormFieldNumberInput<FormValues>
              name="order"
              label="Order"
              width={2}
            />
          )}

          <ButtonGroup>
            <Button type="submit" disabled={form.isSubmitting}>
              Save legacy release
            </Button>
            {cancelButton}
          </ButtonGroup>
        </Form>
      )}
    </Formik>
  );
};

export default LegacyReleaseForm;
