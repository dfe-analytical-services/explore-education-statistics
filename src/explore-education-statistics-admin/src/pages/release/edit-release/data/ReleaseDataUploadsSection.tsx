import ImporterStatus from '@admin/components/ImporterStatus';
import Link from '@admin/components/Link';
import service from '@admin/services/release/edit-release/data/service';
import permissionService from '@admin/services/permissions/service';
import { DataFile } from '@admin/services/release/edit-release/data/types';
import { ImportStatusCode } from '@admin/services/release/imports/types';
import submitWithFormikValidation from '@admin/validation/formikSubmitHandler';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import Button from '@common/components/Button';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldFileSelector from '@common/components/form/FormFieldFileSelector';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { format } from 'date-fns';
import { FormikActions, FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';

interface FormValues {
  subjectTitle: string;
  dataFile: File | null;
  metadataFile: File | null;
}

interface Props {
  publicationId: string;
  releaseId: string;
}

const formId = 'dataFileUploadForm';

const emptyDataFile: DataFile = {
  canDelete: false,
  fileSize: { size: 0, unit: '' },
  filename: '',
  metadataFilename: '',
  rows: 0,
  title: '',
  userName: '',
  created: new Date(),
};

const ReleaseDataUploadsSection = ({
  publicationId,
  releaseId,
  handleApiErrors,
}: Props & ErrorControlProps) => {
  const [dataFiles, setDataFiles] = useState<DataFile[]>([]);
  const [deleteDataFile, setDeleteDataFile] = useState<DataFile>(emptyDataFile);
  const [canUpdateRelease, setCanUpdateRelease] = useState(false);

  useEffect(() => {
    Promise.all([
      service.getReleaseDataFiles(releaseId),
      permissionService.canUpdateRelease(releaseId),
    ])
      .then(([releaseDataFiles, canUpdateReleaseResponse]) => {
        setDataFiles(releaseDataFiles);
        setCanUpdateRelease(canUpdateReleaseResponse);
      })
      .catch(handleApiErrors);
  }, [publicationId, releaseId, handleApiErrors]);

  const resetPage = async <T extends {}>({ resetForm }: FormikActions<T>) => {
    resetForm();

    document
      .querySelectorAll(`#${formId} input[type='file']`)
      .forEach(input => {
        const fileInput = input as HTMLInputElement;
        fileInput.value = '';
      });

    const files = await service
      .getReleaseDataFiles(releaseId)
      .catch(handleApiErrors);

    setDataFiles(files);
  };

  const statusChangeHandler = async (
    dataFile: DataFile,
    importstatusCode: ImportStatusCode,
  ) => {
    const updatedDataFiles = [...dataFiles];
    const updatedFile = updatedDataFiles.find(
      file => file.filename === dataFile.filename,
    );

    if (!updatedFile) {
      return;
    }
    updatedFile.canDelete =
      importstatusCode &&
      (importstatusCode === 'COMPLETE' || importstatusCode === 'FAILED');
    setDataFiles(updatedDataFiles);
  };

  const errorCodeMappings = [
    errorCodeToFieldError(
      'CANNOT_OVERWRITE_DATA_FILE',
      'dataFile',
      'Choose a unique data file name',
    ),
    errorCodeToFieldError(
      'CANNOT_OVERWRITE_METADATA_FILE',
      'metadataFile',
      'Choose a unique metadata file name',
    ),
    errorCodeToFieldError(
      'DATA_AND_METADATA_FILES_CANNOT_HAVE_THE_SAME_NAME',
      'dataFile',
      'Choose a different file name for data and metadata files',
    ),
    errorCodeToFieldError(
      'DATA_FILE_CANNOT_BE_EMPTY',
      'dataFile',
      'Choose a data file that is not empty',
    ),
    errorCodeToFieldError(
      'METADATA_FILE_CANNOT_BE_EMPTY',
      'metadataFile',
      'Choose a metadata file that is not empty',
    ),
    errorCodeToFieldError(
      'DATA_FILE_MUST_BE_CSV_FILE',
      'dataFile',
      'Data file must be a csv file',
    ),
    errorCodeToFieldError(
      'META_FILE_MUST_BE_CSV_FILE',
      'metadataFile',
      'Meta file must be a csv file',
    ),
    errorCodeToFieldError(
      'SUBJECT_TITLE_MUST_BE_UNIQUE',
      'subjectTitle',
      'Subject title must be unique',
    ),
  ];

  const submitFormHandler = submitWithFormikValidation<FormValues>(
    async (values, actions) => {
      await service.uploadDataFiles(releaseId, {
        subjectTitle: values.subjectTitle,
        dataFile: values.dataFile as File,
        metadataFile: values.metadataFile as File,
      });

      await resetPage(actions);
    },
    handleApiErrors,
    ...errorCodeMappings,
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        subjectTitle: '',
        dataFile: null,
        metadataFile: null,
      }}
      onSubmit={submitFormHandler}
      validationSchema={Yup.object<FormValues>({
        subjectTitle: Yup.string()
          .required('Enter a subject title')
          .test('unique', 'Subject title must be unique', function unique(
            value: string,
          ) {
            if (!value) {
              return true;
            }
            return (
              dataFiles.find(
                f => f.title.toUpperCase() === value.toUpperCase(),
              ) === undefined
            );
          }),
        dataFile: Yup.mixed().required('Choose a data file'),
        metadataFile: Yup.mixed().required('Choose a metadata file'),
      })}
      render={(form: FormikProps<FormValues>) => {
        return (
          <Form id={formId}>
            {canUpdateRelease ? (
              <>
                <FormFieldset
                  id={`${formId}-allFieldsFieldset`}
                  legend="Add new data to release"
                >
                  <FormFieldTextInput<FormValues>
                    id={`${formId}-subjectTitle`}
                    name="subjectTitle"
                    label="Subject title"
                    width={20}
                  />

                  <FormFieldFileSelector<FormValues>
                    id={`${formId}-dataFile`}
                    name="dataFile"
                    label="Upload data file"
                    formGroupClass="govuk-!-margin-top-6"
                    form={form}
                  />

                  <FormFieldFileSelector<FormValues>
                    id={`${formId}-metadataFile`}
                    name="metadataFile"
                    label="Upload metadata file"
                    form={form}
                  />
                </FormFieldset>

                <Button
                  type="submit"
                  className="govuk-button govuk-!-margin-right-6"
                >
                  Upload data files
                </Button>
                <Link
                  to="#"
                  className="govuk-button govuk-button--secondary"
                  onClick={() => resetPage(form)}
                >
                  Cancel
                </Link>
              </>
            ) : (
              'Release has been approved, and can no longer be updated.'
            )}

            {dataFiles.length > 0 && (
              <>
                <hr />
                <h2 className="govuk-heading-m">Uploaded data files</h2>
              </>
            )}

            {dataFiles.map(dataFile => (
              <SummaryList
                key={dataFile.filename}
                additionalClassName="govuk-!-margin-bottom-9"
              >
                <SummaryListItem term="Subject title">
                  <h4 className="govuk-heading-m">{dataFile.title}</h4>
                </SummaryListItem>
                <SummaryListItem term="Data file">
                  <Link
                    to="#"
                    onClick={() =>
                      service
                        .downloadDataFile(releaseId, dataFile.filename)
                        .catch(handleApiErrors)
                    }
                  >
                    {dataFile.filename}
                  </Link>
                </SummaryListItem>
                <SummaryListItem term="Metadata file">
                  <Link
                    to="#"
                    onClick={() =>
                      service
                        .downloadDataMetadataFile(
                          releaseId,
                          dataFile.metadataFilename,
                        )
                        .catch(handleApiErrors)
                    }
                  >
                    {dataFile.metadataFilename}
                  </Link>
                </SummaryListItem>
                <SummaryListItem term="Data file size">
                  {dataFile.fileSize.size.toLocaleString()}{' '}
                  {dataFile.fileSize.unit}
                </SummaryListItem>
                <SummaryListItem term="Number of rows">
                  {dataFile.rows.toLocaleString()}
                </SummaryListItem>

                <ImporterStatus
                  releaseId={releaseId}
                  dataFile={dataFile}
                  onStatusChangeHandler={statusChangeHandler}
                />
                <SummaryListItem term="Uploaded by">
                  <a href={`mailto:${dataFile.userName}`}>
                    {dataFile.userName}
                  </a>
                </SummaryListItem>
                <SummaryListItem term="Date uploaded">
                  {format(dataFile.created, 'd/M/yyyy HH:mm')}
                </SummaryListItem>
                {canUpdateRelease && dataFile.canDelete && (
                  <SummaryListItem
                    term="Actions"
                    actions={
                      <Link to="#" onClick={() => setDeleteDataFile(dataFile)}>
                        Delete files
                      </Link>
                    }
                  />
                )}
              </SummaryList>
            ))}

            <ModalConfirm
              mounted={deleteDataFile && deleteDataFile.title.length > 0}
              title="Confirm deletion of selected data files"
              onExit={() => setDeleteDataFile(emptyDataFile)}
              onCancel={() => setDeleteDataFile(emptyDataFile)}
              onConfirm={async () => {
                await service
                  .deleteDataFiles(releaseId, deleteDataFile)
                  .catch(handleApiErrors)
                  .finally(() => {
                    setDeleteDataFile(emptyDataFile);
                    resetPage(form);
                  });
              }}
            >
              <p>
                This data will no longer be available for use in this release
              </p>
            </ModalConfirm>
          </Form>
        );
      }}
    />
  );
};

export default withErrorControl(ReleaseDataUploadsSection);
