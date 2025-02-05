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
  title?: string;
  uploadType: FileType;
  zipFile?: File | null;
  bulkZipFile?: File | null;
}

const MAX_FILENAME_SIZE = 150;
const titleMaxLength = 120;

const subjectErrorMappings = [
  mapFieldErrors<DataFileUploadFormValues>({
    target: 'title',
    messages: {
      SubjectTitleMustBeUnique: 'Title must be unique',
      SubjectTitleCannotContainSpecialCharacters:
        'Title cannot contain special characters',
    },
  }),
];

// Error messages are returned by the backend so don't need to
// define them here, but can't leave them blank in the mapping.
const fileErrorMappings = {
  DataSetTitleCannotBeEmpty: 'DataSetTitleCannotBeEmpty',
  DataSetTitleShouldNotContainSpecialCharacters:
    'DataSetTitleShouldNotContainSpecialCharacters',
  DataSetTitleShouldBeUnique: 'DataTitleShouldBeUnique',
  DataAndMetaFilesCannotHaveSameName: 'DataAndMetaFilesCannotHaveSameName',
  FilenameCannotContainSpacesOrSpecialCharacters:
    'FilenameCannotContainSpacesOrSpecialCharacters',
  FilenameMustEndDotCsv: 'FilenameMustEndDotCsv',
  MetaFilenameMustEndDotMetaDotCsv: 'MetaFilenameMustEndDotMetaDotCsv',
  FileNameTooLong: 'FileNameTooLong',
  FilenameNotUnique: 'FilenameNotUnique',
  FileSizeMustNotBeZero: 'FileSizeMustNotBeZero',
  MustBeCsvFile: 'MustBeCsvFile',
  CannotReplaceDataSetWithApiDataSet: 'CannotReplaceDataSetWithApiDataSet',
};

function baseErrorMappings(
  fileType: FileType,
): FieldMessageMapper<DataFileUploadFormValues>[] {
  if (fileType === 'bulkZip') {
    return [
      mapFieldErrors<DataFileUploadFormValues>({
        target: 'bulkZipFile' as FieldName<DataFileUploadFormValues>,
        messages: {
          ...fileErrorMappings,
          ZipFilenameMustEndDotZip: 'ZipFilenameMustEndDotZip',
          MustBeZipFile: 'MustBeZipFile',
          BulkDataZipMustContainDataSetNamesCsv:
            'BulkDataZipMustContainDataSetNamesCsv',
          DataSetNamesCsvReaderException: 'DataSetNamesCsvReaderException',
          DataSetNamesCsvIncorrectHeaders: 'DataSetNamesCsvIncorrectHeaders',
          DataSetNamesCsvFilenamesShouldNotEndDotCsv:
            'DataSetNamesCsvFilenamesShouldNotEndDotCsv',
          DataSetNamesCsvFilenamesShouldBeUnique:
            'DataSetNamesCsvFilenamesShouldBeUnique',
          FileNotFoundInZip: 'FileNotFoundInZip',
          ZipContainsUnusedFiles: 'ZipContainsUnusedFiles',
          DataReplacementAlreadyInProgress:
            'Data replacement already in progress',
          DataSetTitleTooLong: 'DataSetTitleTooLong',
        },
      }),
    ];
  }

  if (fileType === 'zip') {
    return [
      mapFieldErrors<DataFileUploadFormValues>({
        target: 'zipFile' as FieldName<DataFileUploadFormValues>,
        messages: {
          ...fileErrorMappings,
          ZipFilenameMustEndDotZip: 'ZipFilenameMustEndDotZip',
          MustBeZipFile: 'MustBeZipFile',
          DataZipFileShouldContainTwoFiles: 'DataZipFileShouldContainTwoFiles',
        },
      }),
    ];
  }

  return [
    mapFieldErrors<DataFileUploadFormValues>({
      target: 'dataFile' as FieldName<DataFileUploadFormValues>,
      messages: fileErrorMappings,
    }),
    mapFieldErrors<DataFileUploadFormValues>({
      target: 'metadataFile' as FieldName<DataFileUploadFormValues>,
      messages: fileErrorMappings,
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
      title: Yup.string(),
      uploadType: Yup.string().oneOf(['csv', 'zip', 'bulkZip']).defined(),
      zipFile: Yup.file().when('uploadType', {
        is: 'zip',
        then: s =>
          s
            .required('Choose a zip file')
            .minSize(0, 'Choose a ZIP file that is not empty'),
        otherwise: s => s.nullable(),
      }),
      bulkZipFile: Yup.file().when('uploadType', {
        is: 'bulkZip',
        then: s =>
          s
            .required('Choose a zip file')
            .minSize(0, 'Choose a ZIP file that is not empty'),
        otherwise: s => s.nullable(),
      }),
    });

    if (!isDataReplacement) {
      return schema.shape({
        title: Yup.string().when('uploadType', {
          is: (uploadType: FileType) =>
            uploadType === 'csv' || uploadType === 'zip',
          then: s =>
            s
              .required('Enter a title')
              .test({
                name: 'unique',
                message: 'Enter a unique title',
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
              })
              .max(
                titleMaxLength,
                `Title must be ${titleMaxLength} characters or less`,
              ),
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
          : { ...defaultInitialValues, title: '' }
      }
      resetAfterSubmit
      validationSchema={validationSchema}
    >
      {({ formState, reset, getValues }) => {
        const uploadType = getValues('uploadType');

        return (
          <Form id="dataFileUploadForm" onSubmit={onSubmit}>
            <div style={{ position: 'relative' }}>
              {formState.isSubmitting && (
                <LoadingSpinner text="Uploading files" overlay />
              )}
              {!isDataReplacement && uploadType !== 'bulkZip' && (
                <FormFieldTextInput<DataFileUploadFormValues>
                  name="title"
                  label="Title"
                  className="govuk-!-width-two-thirds"
                  maxLength={titleMaxLength}
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
                  ...(!isDataReplacement
                    ? [
                        {
                          label: 'Bulk ZIP upload',
                          hint: 'To import multiple data files at once',
                          value: 'bulkZip',
                          conditional: (
                            <FormFieldFileInput<DataFileUploadFormValues>
                              hint="Must contain dataset_names.csv and pairs of csv/meta.csv data files"
                              name="bulkZipFile"
                              label="Upload bulk ZIP file"
                              accept=".zip"
                            />
                          ),
                        },
                      ]
                    : []),
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
