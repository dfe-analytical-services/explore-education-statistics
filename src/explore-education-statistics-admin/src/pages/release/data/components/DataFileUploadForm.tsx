import { DataFile } from '@admin/services/releaseDataFileService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import {
  FieldMessageMapper,
  FieldName,
  mapFieldErrors,
} from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import React, { useMemo, useState } from 'react';
import { ObjectSchema } from 'yup';

type FileType = 'csv' | 'zip' | 'bulkZip';

export interface DataFileUploadFormValues {
  dataFile?: File | null;
  metadataFile?: File | null;
  subjectTitle?: string;
  uploadType: FileType;
  zipFile?: File | null;
}

const MAX_FILENAME_SIZE = 150;

const subjectErrorMappings = [
  mapFieldErrors<DataFileUploadFormValues>({
    target: 'subjectTitle',
    messages: {
      SubjectTitleMustBeUnique: 'Subject title must be unique',
      SubjectTitleCannotContainSpecialCharacters:
        'Subject title cannot contain special characters',
    },
  }),
];

function baseErrorMappings(
  fileType: FileType,
): FieldMessageMapper<DataFileUploadFormValues>[] {
  if (fileType === 'bulkZip') {
    return [
      mapFieldErrors<DataFileUploadFormValues>({
        target: 'zipFile' as FieldName<DataFileUploadFormValues>,
        messages: {
          DataZipMustBeZipFile: 'Choose a valid ZIP file',
          DataBulkZipFileMustHaveManifest: 'ZIP file must contain a manifest',
          // @MarkFix add errors here
          // DataZipFileDoesNotContainCsvFiles:
          //   'ZIP file does not contain any CSV files',
          // DataFilenameNotUnique: 'Choose a unique ZIP data file name',
          // DataZipFilenameTooLong: `Maximum ZIP data filename cannot exceed ${MAX_FILENAME_SIZE} characters`,
          // DataFilenameTooLong: `Maximum data filename cannot exceed ${MAX_FILENAME_SIZE} characters`,
          // MetaFilenameTooLong: `Maximum metadata filename cannot exceed ${MAX_FILENAME_SIZE} characters`,
          // DataZipContentFilenamesTooLong: `Maximum data and metadata filenames cannot exceed ${MAX_FILENAME_SIZE} characters`,
          // DataAndMetadataFilesCannotHaveTheSameName:
          //   'ZIP data and metadata filenames cannot be the same',
          // DataFileCannotBeEmpty: 'Choose a ZIP data file that is not empty',
          // DataFilenameCannotContainSpacesOrSpecialCharacters:
          //   'ZIP data filename cannot contain spaces or special characters',
          // MetadataFileCannotBeEmpty:
          //   'Choose a ZIP metadata file that is not empty',
          // MetaFilenameCannotContainSpacesOrSpecialCharacters:
          //   'ZIP metadata filename cannot contain spaces or special characters',
          // MetaFileIsIncorrectlyNamed:
          //   'ZIP metadata filename must end with .meta.csv',
        },
      }),
    ];
  }

  if (fileType === 'zip') {
    return [
      mapFieldErrors<DataFileUploadFormValues>({
        target: 'zipFile' as FieldName<DataFileUploadFormValues>,
        messages: {
          DataZipMustBeZipFile: 'Choose a valid ZIP file',
          DataZipFileCanOnlyContainTwoFiles:
            'ZIP file can only contain two CSV files',
          DataZipFileDoesNotContainCsvFiles:
            'ZIP file does not contain any CSV files',
          DataFilenameNotUnique: 'Choose a unique ZIP data file name',
          DataZipFilenameTooLong: `Maximum ZIP data filename cannot exceed ${MAX_FILENAME_SIZE} characters`,
          DataFilenameTooLong: `Maximum data filename cannot exceed ${MAX_FILENAME_SIZE} characters`,
          MetaFilenameTooLong: `Maximum metadata filename cannot exceed ${MAX_FILENAME_SIZE} characters`,
          DataZipContentFilenamesTooLong: `Maximum data and metadata filenames cannot exceed ${MAX_FILENAME_SIZE} characters`,
          DataAndMetadataFilesCannotHaveTheSameName:
            'ZIP data and metadata filenames cannot be the same',
          DataFileCannotBeEmpty: 'Choose a ZIP data file that is not empty',
          DataFilenameCannotContainSpacesOrSpecialCharacters:
            'ZIP data filename cannot contain spaces or special characters',
          MetadataFileCannotBeEmpty:
            'Choose a ZIP metadata file that is not empty',
          MetaFilenameCannotContainSpacesOrSpecialCharacters:
            'ZIP metadata filename cannot contain spaces or special characters',
          MetaFileIsIncorrectlyNamed:
            'ZIP metadata filename must end with .meta.csv',
        },
      }),
    ];
  }

  return [
    mapFieldErrors<DataFileUploadFormValues>({
      target: 'dataFile' as FieldName<DataFileUploadFormValues>,
      messages: {
        DataFilenameNotUnique: 'Choose a unique data file name',
        DataAndMetadataFilesCannotHaveTheSameName:
          'Choose a different file name for data and metadata files',
        DataFileCannotBeEmpty: 'Choose a data file that is not empty',
        DataFileMustBeCsvFile: 'Data file must be a CSV with UTF-8 encoding',
        DataFilenameCannotContainSpacesOrSpecialCharacters:
          'Data filename cannot contain spaces or special characters',
        DataFilenameTooLong: `Maximum data filename cannot exceed ${MAX_FILENAME_SIZE} characters`,
      },
    }),
    mapFieldErrors<DataFileUploadFormValues>({
      target: 'metadataFile' as FieldName<DataFileUploadFormValues>,
      messages: {
        MetadataFileCannotBeEmpty: 'Choose a metadata file that is not empty',
        MetaFileMustBeCsvFile:
          'Metadata file must be a CSV with UTF-8 encoding',
        MetaFilenameCannotContainSpacesOrSpecialCharacters:
          'Metadata filename cannot contain spaces or special characters',
        MetaFileIsIncorrectlyNamed: 'Metadata filename is incorrectly named',
        MetaFilenameTooLong: `Maximum metadata filename cannot exceed ${MAX_FILENAME_SIZE} characters`,
      },
    }),
  ];
}

interface Props {
  dataFiles?: DataFile[];
  isDataReplacement?: boolean;
  onSubmit: (values: DataFileUploadFormValues) => void | Promise<void>;
}

export default function DataFileUploadForm({
  dataFiles,
  isDataReplacement = false,
  onSubmit,
}: Props) {
  const [selectedFileType, setSelectedFileType] = useState<FileType>('csv');

  const getErrorMappings = () => {
    return isDataReplacement
      ? baseErrorMappings(selectedFileType)
      : [...baseErrorMappings(selectedFileType), ...subjectErrorMappings];
  };

  const validationSchema = useMemo<
    ObjectSchema<DataFileUploadFormValues>
  >(() => {
    const schema = Yup.object({
      dataFile: Yup.file().when('uploadType', {
        is: 'csv',
        then: s =>
          s
            .required('Choose a data file')
            .minSize(0, 'Choose a data file that is not empty'),
        otherwise: s => s.nullable(),
      }),
      metadataFile: Yup.file().when('uploadType', {
        is: 'csv',
        then: s =>
          s
            .required('Choose a metadata file')
            .minSize(0, 'Choose a metadata file that is not empty'),
        otherwise: s => s.nullable(),
      }),
      subjectTitle: Yup.string(),
      uploadType: Yup.string().oneOf(['csv', 'zip', 'bulkZip']).defined(),
      zipFile: Yup.file().when('uploadType', {
        is: 'zip',
        then: s =>
          s
            .required('Choose a zip file')
            .minSize(0, 'Choose a ZIP file that is not empty'),
        otherwise: s => s.nullable(),
      }),
    });

    if (!isDataReplacement) {
      return schema.shape({
        subjectTitle: Yup.string()
          .required('Enter a subject title')
          .test({
            name: 'unique',
            message: 'Enter a unique subject title',
            test(value: string) {
              if (!value) {
                return true;
              }

              return (
                dataFiles?.find(
                  f => f.title.toUpperCase() === value.toUpperCase(),
                ) === undefined
              );
            },
          }),
      });
    }

    return schema;
  }, [dataFiles, isDataReplacement]);

  const defaultInitialValues: DataFileUploadFormValues = {
    uploadType: 'csv',
    dataFile: null,
    metadataFile: null,
    zipFile: null,
  };

  return (
    <FormProvider
      errorMappings={getErrorMappings()}
      initialValues={
        isDataReplacement
          ? defaultInitialValues
          : { ...defaultInitialValues, subjectTitle: '' }
      }
      resetAfterSubmit
      validationSchema={validationSchema}
    >
      {({ formState, reset }) => {
        return (
          <Form id="dataFileUploadForm" onSubmit={onSubmit}>
            <div style={{ position: 'relative' }}>
              {formState.isSubmitting && (
                <LoadingSpinner text="Uploading files" overlay />
              )}
              {!isDataReplacement && ( // @MarkFix hide if bulkZip?
                <FormFieldTextInput<DataFileUploadFormValues>
                  name="subjectTitle"
                  label="Subject title"
                  className="govuk-!-width-two-thirds"
                />
              )}

              <FormFieldRadioGroup<DataFileUploadFormValues>
                name="uploadType"
                legend="Choose upload method"
                hint={`Filenames must be under ${MAX_FILENAME_SIZE} characters in length`}
                order={[]}
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
                  {
                    label: 'Bulk ZIP upload',
                    hint: 'To import multiple data files at once',
                    value: 'bulkZip',
                    hidden: isDataReplacement,
                    conditional: (
                      <FormFieldFileInput<DataFileUploadFormValues>
                        hint="Must contain dataset_names.csv and pairs of csv/meta.csv data files"
                        name="zipFile"
                        label="Upload ZIP file"
                        accept=".zip"
                      />
                    ),
                  },
                ]}
                onChange={event => {
                  setSelectedFileType(event.target.value as FileType);
                }}
              />

              <ButtonGroup>
                <Button type="submit" disabled={formState.isSubmitting}>
                  Upload data files
                </Button>

                <ButtonText
                  disabled={formState.isSubmitting}
                  onClick={() => {
                    reset();
                  }}
                >
                  Cancel
                </ButtonText>
              </ButtonGroup>
            </div>
          </Form>
        );
      }}
    </FormProvider>
  );
}
