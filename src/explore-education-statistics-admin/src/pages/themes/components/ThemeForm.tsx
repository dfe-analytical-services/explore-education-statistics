import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { mapFieldErrors } from '@common/validation/serverValidations';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import Yup from '@common/validation/yup';
import React, { ReactNode, useMemo } from 'react';
import { ObjectSchema } from 'yup';

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
  const validationSchema = useMemo<ObjectSchema<ThemeFormValues>>(() => {
    return Yup.object({
      title: Yup.string().required('Enter a title'),
      summary: Yup.string().required('Enter a summary'),
    });
  }, []);

  return (
    <FormProvider
      enableReinitialize
      errorMappings={errorMappings}
      initialValues={
        initialValues ?? {
          title: '',
          summary: '',
        }
      }
      validationSchema={validationSchema}
    >
      {({ formState }) => {
        return (
          <RHFForm id={id} onSubmit={onSubmit}>
            <RHFFormFieldTextInput<ThemeFormValues>
              label="Title"
              name="title"
              className="govuk-!-width-two-thirds"
            />

            <RHFFormFieldTextInput<ThemeFormValues>
              label="Summary"
              name="summary"
              className="govuk-!-width-two-thirds"
            />

            <ButtonGroup>
              <Button type="submit" disabled={formState.isSubmitting}>
                Save theme
              </Button>
              {cancelButton}
            </ButtonGroup>
          </RHFForm>
        );
      }}
    </FormProvider>
  );
};

export default ThemeForm;
