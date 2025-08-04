import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { mapFieldErrors } from '@common/validation/serverValidations';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import Yup from '@common/validation/yup';
import React, { ReactNode, useMemo } from 'react';
import { ObjectSchema } from 'yup';

export interface EducationInNumbersSummaryFormValues {
  title: string;
  description: string;
  slug?: string;
}

const errorMappings = [
  mapFieldErrors<EducationInNumbersSummaryFormValues>({
    target: 'title',
    messages: {
      SlugNotUnique: 'Enter a unique title',
    },
  }),
];

interface Props {
  cancelButton?: ReactNode;
  id?: string;
  initialValues?: EducationInNumbersSummaryFormValues;
  isEditForm?: boolean;
  onSubmit: (values: EducationInNumbersSummaryFormValues) => void;
}

const EducationInNumbersSummaryForm = ({
  cancelButton,
  id = 'educationInNumbersSummaryForm',
  initialValues,
  isEditForm = false,
  onSubmit,
}: Props) => {
  const validationSchema = useMemo<
    ObjectSchema<EducationInNumbersSummaryFormValues>
  >(() => {
    return Yup.object({
      title: Yup.string().required('Enter a title'),
      description: Yup.string().required('Enter a description'),
      slug: Yup.string().when([], {
        is: () => isEditForm,
        then: () =>
          Yup.string()
            .required('Enter a slug')
            .matches(/^[a-z0-9]+(?:-[a-z0-9]+)*$/, {
              message: 'Slug must be lowercase and can only contain hyphens',
            }),
        otherwise: () => Yup.string().notRequired(),
      }),
    });
  }, [isEditForm]);

  return (
    <FormProvider
      enableReinitialize
      errorMappings={errorMappings}
      initialValues={
        initialValues ?? {
          title: '',
          description: '',
        }
      }
      validationSchema={validationSchema}
    >
      {({ formState }) => {
        return (
          <Form id={id} onSubmit={onSubmit}>
            <FormFieldTextInput<EducationInNumbersSummaryFormValues>
              label="Title"
              name="title"
              className="govuk-!-width-two-thirds"
            />

            <FormFieldTextInput<EducationInNumbersSummaryFormValues>
              label="Slug"
              name="slug"
              className="govuk-!-width-two-thirds"
            />

            <FormFieldTextInput<EducationInNumbersSummaryFormValues>
              label="Description"
              name="description"
              className="govuk-!-width-two-thirds"
            />

            <ButtonGroup>
              <Button type="submit" disabled={formState.isSubmitting}>
                {isEditForm ? 'Update' : 'Create'} page
              </Button>
              {cancelButton}
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
};

export default EducationInNumbersSummaryForm;
