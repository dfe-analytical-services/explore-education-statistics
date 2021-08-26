import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup } from '@common/components/form';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useFormSubmit from '@common/hooks/useFormSubmit';
import {
  FieldMessageMapper,
  mapFieldErrors,
} from '@common/validation/serverValidations';
import useMountedRef from '@common/hooks/useMountedRef';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { ReactNode } from 'react';
import { ObjectSchema } from 'yup';

export interface DataFileUploadFormValues {
  uploadType: 'csv' | 'zip';
  dataFile: File | null;
  metadataFile: File | null;
  zipFile: File | null;
}

const baseErrorMappings = (
  values: DataFileUploadFormValues,
): FieldMessageMapper<DataFileUploadFormValues>[] => {
  if (values.uploadType === 'zip') {
    return [
      mapFieldErrors<DataFileUploadFormValues>({
        target: 'zipFile',
        messages: {
          DATA_FILE_MUST_BE_ZIP_FILE: 'Choose a valid ZIP file',
          DATA_ZIP_FILE_CAN_ONLY_CONTAIN_TWO_FILES:
            'ZIP file can only contain two CSV files',
          DATA_ZIP_FILE_DOES_NOT_CONTAIN_CSV_FILES:
            'ZIP file does not contain any CSV files',
          DATA_ZIP_FILE_ALREADY_EXISTS: 'ZIP file already exists',
          CANNOT_OVERWRITE_DATA_FILE: 'Choose a unique ZIP data file name',
          DATA_AND_METADATA_FILES_CANNOT_HAVE_THE_SAME_NAME:
            'ZIP data and metadata filenames cannot be the same',
          DATA_FILE_CANNOT_BE_EMPTY: 'Choose a ZIP data file that is not empty',
          DATA_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
            'ZIP data filename cannot contain spaces or special characters',
          METADATA_FILE_CANNOT_BE_EMPTY:
            'Choose a ZIP metadata file that is not empty',
          META_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
            'ZIP metadata filename cannot contain spaces or special characters',
          META_FILE_IS_INCORRECTLY_NAMED:
            'ZIP metadata filename must end with .meta.csv',
        },
      }),
    ];
  }

  return [
    mapFieldErrors<DataFileUploadFormValues>({
      target: 'dataFile',
      messages: {
        CANNOT_OVERWRITE_DATA_FILE: 'Choose a unique data file name',
        DATA_AND_METADATA_FILES_CANNOT_HAVE_THE_SAME_NAME:
          'Choose a different file name for data and metadata files',
        DATA_FILE_CANNOT_BE_EMPTY: 'Choose a data file that is not empty',
        DATA_FILE_MUST_BE_CSV_FILE:
          'Data file must be a CSV with UTF-8 encoding',
        DATA_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
          'Data filename cannot contain spaces or special characters',
        DATA_FILE_ALREADY_UPLOADED: 'Data file has already been uploaded',
      },
    }),
    mapFieldErrors<DataFileUploadFormValues>({
      target: 'metadataFile',
      messages: {
        METADATA_FILE_CANNOT_BE_EMPTY:
          'Choose a metadata file that is not empty',
        META_FILE_MUST_BE_CSV_FILE:
          'Metadata file must be a CSV with UTF-8 encoding',
        META_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
          'Metadata filename cannot contain spaces or special characters',
        META_FILE_IS_INCORRECTLY_NAMED:
          'Metadata filename is incorrectly named',
      },
    }),
  ];
};

interface Props<FormValues extends DataFileUploadFormValues> {
  beforeFields?: ReactNode;
  errorMappings?: FieldMessageMapper<FormValues>[];
  id?: string;
  initialValues?: FormValues;
  validationSchema?: (
    baseSchema: ObjectSchema<DataFileUploadFormValues>,
  ) => ObjectSchema<FormValues>;
  submitText?: string;
  onSubmit: (values: FormValues) => void;
}

const DataFileUploadForm = <FormValues extends DataFileUploadFormValues>({
  beforeFields,
  errorMappings = [],
  id = 'dataFileUploadForm',
  initialValues,
  submitText = 'Upload data files',
  validationSchema,
  onSubmit,
}: Props<FormValues>) => {
  const isMounted = useMountedRef();

  const handleSubmit = useFormSubmit<FormValues>(
    async (values, actions) => {
      await onSubmit(values);
      if (isMounted.current) {
        actions.resetForm();
      }
    },
    values => {
      return [...baseErrorMappings(values), ...errorMappings];
    },
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={
        initialValues ??
        ({
          uploadType: 'csv',
          dataFile: null,
          metadataFile: null,
          zipFile: null,
        } as FormValues)
      }
      onReset={() => {
        document
          .querySelectorAll(`#${id} input[type='file']`)
          .forEach(input => {
            const fileInput = input as HTMLInputElement;
            fileInput.value = '';
          });
      }}
      onSubmit={handleSubmit}
      validationSchema={() => {
        const baseSchema = Yup.object<DataFileUploadFormValues>({
          uploadType: Yup.mixed<
            DataFileUploadFormValues['uploadType']
          >().oneOf(['csv', 'zip']),
          dataFile: Yup.file().when('uploadType', {
            is: 'csv',
            then: Yup.file()
              .required('Choose a data file')
              .minSize(0, 'Choose a data file that is not empty'),
          }),
          metadataFile: Yup.file().when('uploadType', {
            is: 'csv',
            then: Yup.file()
              .required('Choose a metadata file')
              .minSize(0, 'Choose a metadata file that is not empty'),
          }),
          zipFile: Yup.file().when('uploadType', {
            is: 'zip',
            then: Yup.file()
              .required('Choose a zip file')
              .minSize(0, 'Choose a ZIP file that is not empty'),
          }),
        });

        return validationSchema ? validationSchema(baseSchema) : baseSchema;
      }}
    >
      {form => (
        <>
          <Form id={id}>
            <div style={{ position: 'relative' }}>
              {form.isSubmitting && (
                <LoadingSpinner text="Uploading files" overlay />
              )}

              {beforeFields}

              <FormFieldRadioGroup<DataFileUploadFormValues>
                name="uploadType"
                legend="Choose upload method"
                options={[
                  {
                    label: 'CSV files',
                    value: 'csv',
                    conditional: (
                      <>
                        <FormFieldFileInput<DataFileUploadFormValues>
                          name="dataFile"
                          label="Upload data file"
                          accept=".csv"
                        />

                        <FormFieldFileInput<DataFileUploadFormValues>
                          name="metadataFile"
                          label="Upload metadata file"
                          accept=".csv"
                        />
                      </>
                    ),
                  },
                  {
                    label: 'ZIP file',
                    hint: 'Recommended for larger data files',
                    value: 'zip',
                    conditional: (
                      <FormFieldFileInput<DataFileUploadFormValues>
                        hint="Must contain both the data and metadata CSV files"
                        name="zipFile"
                        label="Upload ZIP file"
                        accept=".zip"
                      />
                    ),
                  },
                ]}
              />

              <ButtonGroup>
                <Button type="submit" disabled={form.isSubmitting}>
                  {submitText}
                </Button>

                <ButtonText
                  disabled={form.isSubmitting}
                  onClick={() => {
                    form.resetForm();
                  }}
                >
                  Cancel
                </ButtonText>
              </ButtonGroup>
            </div>
          </Form>
        </>
      )}
    </Formik>
  );
};

export default DataFileUploadForm;
