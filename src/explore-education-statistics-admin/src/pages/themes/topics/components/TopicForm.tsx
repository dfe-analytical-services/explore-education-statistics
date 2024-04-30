import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { ReactNode, useMemo } from 'react';
import { ObjectSchema } from 'yup';

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
  const validationSchema = useMemo<ObjectSchema<TopicFormValues>>(() => {
    return Yup.object({
      title: Yup.string().required('Enter a title'),
    });
  }, []);

  return (
    <FormProvider
      enableReinitialize
      errorMappings={errorMappings}
      initialValues={
        initialValues ?? {
          title: '',
        }
      }
      validationSchema={validationSchema}
    >
      {({ formState }) => {
        return (
          <Form id={id} onSubmit={onSubmit}>
            <FormFieldTextInput<TopicFormValues>
              label="Title"
              name="title"
              className="govuk-!-width-two-thirds"
            />

            <ButtonGroup>
              <Button type="submit" disabled={formState.isSubmitting}>
                Save topic
              </Button>
              {cancelButton}
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
};

export default TopicForm;
