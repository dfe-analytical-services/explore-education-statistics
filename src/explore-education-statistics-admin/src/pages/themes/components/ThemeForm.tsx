import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldTextInput } from '@common/components/form';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { ReactNode } from 'react';

export interface ThemeFormValues {
  title: string;
  summary: string;
}

const errorMappings = [
  mapFieldErrors<ThemeFormValues>({
    target: 'title',
    messages: {
      SlugNotUnique: 'Enter a unique title',
    },
  }),
];

interface Props {
  cancelButton?: ReactNode;
  id?: string;
  initialValues?: ThemeFormValues;
  onSubmit: (values: ThemeFormValues) => void;
}

const ThemeForm = ({
  cancelButton,
  id = 'themeForm',
  initialValues,
  onSubmit,
}: Props) => {
  const handleSubmit = useFormSubmit<ThemeFormValues>(values => {
    onSubmit(values);
  }, errorMappings);

  return (
    <Formik<ThemeFormValues>
      initialValues={
        initialValues ?? {
          title: '',
          summary: '',
        }
      }
      validationSchema={Yup.object<ThemeFormValues>({
        title: Yup.string().required('Enter a title'),
        summary: Yup.string().required('Enter a summary'),
      })}
      onSubmit={handleSubmit}
    >
      {form => (
        <Form id={id}>
          <FormFieldTextInput<ThemeFormValues>
            label="Title"
            name="title"
            className="govuk-!-width-two-thirds"
          />

          <FormFieldTextInput<ThemeFormValues>
            label="Summary"
            name="summary"
            className="govuk-!-width-two-thirds"
          />

          <ButtonGroup>
            <Button type="submit" disabled={form.isSubmitting}>
              Save theme
            </Button>
            {cancelButton}
          </ButtonGroup>
        </Form>
      )}
    </Formik>
  );
};

export default ThemeForm;
