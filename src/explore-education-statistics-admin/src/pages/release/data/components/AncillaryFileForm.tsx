import { AncillaryFile } from '@admin/services/releaseAncillaryFileService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFFormFieldFileInput from '@common/components/form/rhf/RHFFormFieldFileInput';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
import RHFForm from '@common/components/form/rhf/RHFForm';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

export interface AncillaryFileFormValues {
  title: string;
  summary: string;
  file?: File | null;
}

const formId = 'ancillaryFileForm';
const MAX_FILE_SIZE = 2147483647; // 2GB

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
      FileSizeLimitExceeded: 'Choose a file that is under 2GB',
    },
  }),
];

export interface AncillaryFileFormProps {
  files?: AncillaryFile[];
  initialValues?: AncillaryFileFormValues;
  isEditing?: boolean;
  onCancel?: () => void;
  onSubmit: (values: AncillaryFileFormValues) => void;
}

export default function AncillaryFileForm({
  files = [],
  initialValues,
  isEditing = false,
  onCancel,
  onSubmit,
}: AncillaryFileFormProps) {
  const validationSchema = useMemo<
    ObjectSchema<AncillaryFileFormValues>
  >(() => {
    const schema = Yup.object({
      title: Yup.string()
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
      file: Yup.file()
        .minSize(0, 'Choose a file that is not empty')
        .maxSize(MAX_FILE_SIZE, 'Choose a file that is under 2GB')
        .notRequired(),
    });

    if (!isEditing) {
      return schema.shape({
        file: Yup.file()
          .required('Choose a file')
          .minSize(0, 'Choose a file that is not empty')
          .maxSize(MAX_FILE_SIZE, 'Choose a file that is under 2GB'),
      });
    }

    return schema;
  }, [files, isEditing]);

  return (
    <FormProvider
      errorMappings={errorMappings}
      initialValues={
        initialValues ?? {
          title: '',
          summary: '',
          file: null,
        }
      }
      resetAfterSubmit
      validationSchema={validationSchema}
    >
      {({ formState, reset }) => {
        return (
          <RHFForm id={formId} onSubmit={onSubmit}>
            <RHFFormFieldTextInput<AncillaryFileFormValues>
              className="govuk-!-width-one-half"
              disabled={formState.isSubmitting}
              label="Title"
              name="title"
            />

            <RHFFormFieldTextArea<AncillaryFileFormValues>
              className="govuk-!-width-one-half"
              disabled={formState.isSubmitting}
              label="Summary"
              name="summary"
            />

            <RHFFormFieldFileInput<AncillaryFileFormValues>
              disabled={formState.isSubmitting}
              hint="Maximum file size 2GB"
              label={isEditing ? 'Upload new file' : 'Upload file'}
              name="file"
            />

            <ButtonGroup>
              <Button type="submit" disabled={formState.isSubmitting}>
                {isEditing ? 'Save file' : 'Add file'}
              </Button>

              <ButtonText
                disabled={formState.isSubmitting}
                onClick={() => {
                  onCancel?.();
                  reset();
                }}
              >
                Cancel
              </ButtonText>

              <LoadingSpinner
                alert
                className="govuk-!-margin-left-2"
                inline
                loading={formState.isSubmitting}
                size="sm"
                text="Saving file"
              />
            </ButtonGroup>
          </RHFForm>
        );
      }}
    </FormProvider>
  );
}
