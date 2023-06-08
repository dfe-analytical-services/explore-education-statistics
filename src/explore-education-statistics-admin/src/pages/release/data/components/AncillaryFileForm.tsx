import { AncillaryFile } from '@admin/services/releaseAncillaryFileService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldTextArea } from '@common/components/form';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import FormFieldTextInput from 'explore-education-statistics-common/src/components/form/FormFieldTextInput';
import { Formik } from 'formik';
import React from 'react';
import { ObjectSchema } from 'yup';

export interface AncillaryFileFormValues {
  title: string;
  summary: string;
  file: File | null;
}

const formId = 'ancillaryFileForm';

const errorMappings = [
  mapFieldErrors<AncillaryFileFormValues>({
    target: 'title',
    messages: {
      FileUploadNameCannotContainSpecialCharacters:
        'File upload name cannot contain special characters',
    },
  }),
  mapFieldErrors<AncillaryFileFormValues>({
    target: 'file',
    messages: {
      CannotOverwriteFile: 'Choose a unique file name',
      FileCannotBeEmpty: 'Choose a file that is not empty',
      FileTypeInvalid: 'Choose a file of an allowed format',
      FilenameCannotContainSpacesOrSpecialCharacters:
        'Filename cannot contain spaces or special characters',
    },
  }),
];

export interface AncillaryFileFormProps {
  files?: AncillaryFile[];
  fileFieldLabel?: string;
  initialValues?: AncillaryFileFormValues;
  resetAfterSubmit?: boolean;
  submitText?: string;
  submittingText?: string;
  validationSchema?: ObjectSchema<Partial<AncillaryFileFormValues>>;
  onCancel?: () => void;
  onSubmit: (values: AncillaryFileFormValues) => void;
}

export default function AncillaryFileForm({
  files = [],
  fileFieldLabel = 'Upload file',
  initialValues,
  resetAfterSubmit,
  submitText = 'Save file',
  submittingText = 'Saving file',
  validationSchema,
  onCancel,
  onSubmit,
}: AncillaryFileFormProps) {
  const handleSubmit = useFormSubmit<AncillaryFileFormValues>(
    async (values, actions) => {
      await onSubmit(values);

      if (resetAfterSubmit) {
        actions.resetForm();
      }
    },
    errorMappings,
  );

  return (
    <Formik<AncillaryFileFormValues>
      enableReinitialize
      initialValues={
        initialValues ?? {
          title: '',
          summary: '',
          file: null,
        }
      }
      onReset={() => {
        document
          .querySelectorAll(`#${formId} input[type='file']`)
          .forEach(input => {
            const fileInput = input as HTMLInputElement;
            fileInput.value = '';
          });
      }}
      onSubmit={handleSubmit}
      validationSchema={Yup.object<AncillaryFileFormValues>({
        title: Yup.string()
          .trim()
          .required('Enter a title')
          .test({
            name: 'unique',
            message: 'Enter a unique title',
            test(value: string) {
              if (!value) {
                return true;
              }

              return files.every(
                f => f.title.toUpperCase() !== value.toUpperCase(),
              );
            },
          }),
        summary: Yup.string().required('Enter a summary'),
        file: Yup.file().minSize(0, 'Choose a file that is not empty'),
      }).concat(validationSchema ?? Yup.object())}
    >
      {form => (
        <Form id={formId}>
          <FormFieldTextInput<AncillaryFileFormValues>
            className="govuk-!-width-one-half"
            disabled={form.isSubmitting}
            label="Title"
            name="title"
          />

          <FormFieldTextArea<AncillaryFileFormValues>
            className="govuk-!-width-one-half"
            disabled={form.isSubmitting}
            label="Summary"
            name="summary"
          />

          <FormFieldFileInput<AncillaryFileFormValues>
            disabled={form.isSubmitting}
            hint={initialValues?.file?.name}
            label={fileFieldLabel}
            name="file"
          />

          <ButtonGroup>
            <Button type="submit" disabled={form.isSubmitting}>
              {submitText}
            </Button>

            <ButtonText
              disabled={form.isSubmitting}
              onClick={() => {
                onCancel?.();
                form.resetForm();
              }}
            >
              Cancel
            </ButtonText>

            <LoadingSpinner
              alert
              className="govuk-!-margin-left-2"
              inline
              loading={form.isSubmitting}
              size="sm"
              text={submittingText}
            />
          </ButtonGroup>
        </Form>
      )}
    </Formik>
  );
}
