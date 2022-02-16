import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldTextInput } from '@common/components/form';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { ReactNode } from 'react';

export interface TopicFormValues {
  title: string;
}

const errorMappings = [
  mapFieldErrors<TopicFormValues>({
    target: 'title',
    messages: {
      SlugNotUnique: 'Enter a unique title',
    },
  }),
];

interface Props {
  cancelButton?: ReactNode;
  id?: string;
  initialValues?: TopicFormValues;
  onSubmit: (values: TopicFormValues) => void;
}

const TopicForm = ({
  cancelButton,
  id = 'topicForm',
  initialValues,
  onSubmit,
}: Props) => {
  const handleSubmit = useFormSubmit<TopicFormValues>(values => {
    onSubmit(values);
  }, errorMappings);

  return (
    <Formik<TopicFormValues>
      initialValues={
        initialValues ?? {
          title: '',
        }
      }
      validationSchema={Yup.object<TopicFormValues>({
        title: Yup.string().required('Enter a title'),
      })}
      onSubmit={handleSubmit}
    >
      {form => (
        <Form id={id}>
          <FormFieldTextInput<TopicFormValues>
            label="Title"
            name="title"
            className="govuk-!-width-two-thirds"
          />

          <ButtonGroup>
            <Button type="submit" disabled={form.isSubmitting}>
              Save topic
            </Button>
            {cancelButton}
          </ButtonGroup>
        </Form>
      )}
    </Formik>
  );
};

export default TopicForm;
