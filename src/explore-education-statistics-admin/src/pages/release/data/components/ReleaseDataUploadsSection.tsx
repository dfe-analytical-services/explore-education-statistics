import ImporterStatus from '@admin/components/ImporterStatus';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import releaseDataFileService, {
  DataFile,
  DeleteDataFilePlan,
  ImportStatusCode,
} from '@admin/services/releaseDataFileService';
import releaseMetaFileService from '@admin/services/releaseMetaFileService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import logger from '@common/services/logger';
import delay from '@common/utils/delay';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { format } from 'date-fns';
import { Formik } from 'formik';
import React, { useState } from 'react';

interface FormValues {
  subjectTitle: string;
  dataFile: File | null;
  metadataFile: File | null;
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'dataFile',
    messages: {
      CANNOT_OVERWRITE_DATA_FILE: 'Choose a unique data file name',
      DATA_AND_METADATA_FILES_CANNOT_HAVE_THE_SAME_NAME:
        'Choose a different file name for data and metadata files',
      DATA_FILE_CANNOT_BE_EMPTY: 'Choose a data file that is not empty',
      DATA_FILE_MUST_BE_CSV_FILE:
        'Data file must be a csv file with UTF-8 encoding',
      DATA_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
        'Data filename cannot contain spaces or special characters',
      DATA_FILE_ALREADY_UPLOADED: 'Data file has already been uploaded',
    },
  }),
  mapFieldErrors<FormValues>({
    target: 'metadataFile',
    messages: {
      CANNOT_OVERWRITE_METADATA_FILE: 'Choose a unique metadata file name',
      METADATA_FILE_CANNOT_BE_EMPTY: 'Choose a metadata file that is not empty',
      META_FILE_MUST_BE_CSV_FILE:
        'Meta file must be a csv file with UTF-8 encoding',
      META_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
        'Meta filename cannot contain spaces or special characters',
      META_FILE_IS_INCORRECTLY_NAMED: 'Meta filename is incorrectly named',
    },
  }),
  // mapFieldErrors<FormValues>({
  //   target: 'zipFile',
  //   messages: {
  //     DATA_FILE_MUST_BE_ZIP_FILE:
  //         'Data file must be a zip file',
  //     DATA_ZIP_FILE_CAN_ONLY_CONTAIN_TWO_FILES:
  //         'Data zip file can only contain two files',
  //     DATA_ZIP_FILE_DOES_NOT_CONTAIN_CSV_FILES:
  //         'Data zip file does not contain csv files',
  //     DATA_ZIP_FILE_ALREADY_EXISTS:
  //         'Data zip file already exists',
  //     CANNOT_OVERWRITE_DATA_FILE: 'Choose a unique data file name',
  //     DATA_AND_METADATA_FILES_CANNOT_HAVE_THE_SAME_NAME:
  //         'Choose a different file name for data and metadata files',
  //     DATA_FILE_CANNOT_BE_EMPTY: 'Choose a data file that is not empty',
  //     DATA_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
  //         'Data filename cannot contain spaces or special characters',
  //     CANNOT_OVERWRITE_METADATA_FILE: 'Choose a unique metadata file name',
  //     METADATA_FILE_CANNOT_BE_EMPTY: 'Choose a metadata file that is not empty',
  //     META_FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
  //         'Meta filename cannot contain spaces or special characters',
  //     META_FILE_IS_INCORRECTLY_NAMED:
  //         'Meta filename is incorrectly named',
  //   },
  // }),
  mapFieldErrors<FormValues>({
    target: 'subjectTitle',
    messages: {
      SUBJECT_TITLE_MUST_BE_UNIQUE: 'Subject title must be unique',
    },
  }),
];

interface Props {
  releaseId: string;
  canUpdateRelease: boolean;
}

interface DeleteDataFile {
  plan: DeleteDataFilePlan;
  file: DataFile;
}

const formId = 'dataFileUploadForm';

const ReleaseDataUploadsSection = ({ releaseId, canUpdateRelease }: Props) => {
  const [deleteDataFile, setDeleteDataFile] = useState<DeleteDataFile>();
  const [isUploading, setIsUploading] = useState(false);

  const {
    value: dataFiles = [],
    setState: setDataFiles,
    isLoading,
  } = useAsyncHandledRetry(
    () => releaseDataFileService.getReleaseDataFiles(releaseId),
    [releaseId],
  );

  const setFileDeleting = (dataFile: DeleteDataFile, deleting: boolean) => {
    setDataFiles({
      value: dataFiles.map(file =>
        file.filename !== dataFile.file.filename
          ? file
          : {
              ...file,
              isDeleting: deleting,
            },
      ),
    });
  };

  const handleStatusChange = async (
    dataFile: DataFile,
    statusCode: ImportStatusCode,
  ) => {
    setDataFiles({
      value: dataFiles.map(file =>
        file.filename !== dataFile.filename
          ? file
          : {
              ...file,
              canDelete:
                statusCode &&
                (statusCode === 'NOT_FOUND' ||
                  statusCode === 'COMPLETE' ||
                  statusCode === 'FAILED'),
            },
      ),
    });
  };

  const handleSubmit = useFormSubmit<FormValues>(async (values, actions) => {
    setIsUploading(true);

    try {
      const file = await releaseDataFileService.uploadDataFiles(releaseId, {
        subjectTitle: values.subjectTitle,
        dataFile: values.dataFile as File,
        metadataFile: values.metadataFile as File,
      });

      actions.resetForm();

      setDataFiles({
        value: [...dataFiles, file],
      });
    } finally {
      setIsUploading(false);
    }
  }, errorMappings);

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={{
        subjectTitle: '',
        dataFile: null,
        metadataFile: null,
      }}
      onReset={() => {
        document
          .querySelectorAll(`#${formId} input[type='file']`)
          .forEach(input => {
            const fileInput = input as HTMLInputElement;
            fileInput.value = '';
          });
      }}
      onSubmit={handleSubmit}
      validationSchema={Yup.object<FormValues>({
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
                dataFiles.find(
                  f => f.title.toUpperCase() === value.toUpperCase(),
                ) === undefined
              );
            },
          }),
        dataFile: Yup.file().required('Choose a data file'),
        metadataFile: Yup.file().required('Choose a metadata file'),
      })}
    >
      {form => (
        <>
          <h2>Add data file to release</h2>

          <div className="govuk-inset-text">
            <h3>Before you start</h3>

            <ul>
              <li>
                make sure your data has passed the screening checks in our{' '}
                <a href="https://github.com/dfe-analytical-services/ees-data-screener">
                  R Project
                </a>{' '}
              </li>
              <li>
                if your data does not meet these standards, you won’t be able to
                upload it to your release
              </li>
              <li>
                if you have any issues uploading data and files, or questions
                about data standards contact:{' '}
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>
              </li>
            </ul>
          </div>

          {canUpdateRelease ? (
            <Form id={formId}>
              <div style={{ position: 'relative' }}>
                {isUploading && (
                  <LoadingSpinner text="Uploading files" overlay />
                )}

                <FormFieldTextInput<FormValues>
                  id={`${formId}-subjectTitle`}
                  name="subjectTitle"
                  label="Subject title"
                  width={20}
                />

                <FormFieldFileInput<FormValues>
                  id={`${formId}-dataFile`}
                  name="dataFile"
                  label="Upload data file"
                />

                <FormFieldFileInput<FormValues>
                  id={`${formId}-metadataFile`}
                  name="metadataFile"
                  label="Upload metadata file"
                />

                <ButtonGroup>
                  <Button type="submit" disabled={form.isSubmitting}>
                    Upload data files
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
          ) : (
            <WarningMessage>
              This release has been approved, and can no longer be updated.
            </WarningMessage>
          )}

          <hr className="govuk-!-margin-top-6 govuk-!-margin-bottom-6" />

          <h2>Uploaded data files</h2>

          <LoadingSpinner loading={isLoading}>
            {dataFiles.length > 0 ? (
              <Accordion id="uploadedDataFiles">
                {dataFiles.map(dataFile => (
                  <AccordionSection
                    key={dataFile.title}
                    heading={dataFile.title}
                    headingTag="h3"
                  >
                    <div style={{ position: 'relative' }}>
                      {dataFile.isDeleting && (
                        <LoadingSpinner text="Deleting files" overlay />
                      )}
                      <SummaryList>
                        <SummaryListItem term="Subject title">
                          {dataFile.title}
                        </SummaryListItem>
                        <SummaryListItem term="Data file">
                          <ButtonText
                            onClick={() =>
                              releaseDataFileService.downloadDataFile(
                                releaseId,
                                dataFile.filename,
                              )
                            }
                          >
                            {dataFile.filename}
                          </ButtonText>
                        </SummaryListItem>
                        <SummaryListItem term="Metadata file">
                          <ButtonText
                            onClick={() =>
                              releaseMetaFileService.downloadDataMetadataFile(
                                releaseId,
                                dataFile.metadataFilename,
                              )
                            }
                          >
                            {dataFile.metadataFilename}
                          </ButtonText>
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
                          onStatusChangeHandler={handleStatusChange}
                        />
                        <SummaryListItem term="Uploaded by">
                          <a href={`mailto:${dataFile.userName}`}>
                            {dataFile.userName}
                          </a>
                        </SummaryListItem>
                        <SummaryListItem term="Date uploaded">
                          {format(dataFile.created, 'd MMMM yyyy HH:mm')}
                        </SummaryListItem>
                        {canUpdateRelease && dataFile.canDelete && (
                          <SummaryListItem
                            term="Actions"
                            actions={
                              <ButtonText
                                onClick={() =>
                                  releaseDataFileService
                                    .getDeleteDataFilePlan(releaseId, dataFile)
                                    .then(plan => {
                                      setDeleteDataFile({
                                        plan,
                                        file: dataFile,
                                      });
                                    })
                                }
                              >
                                Delete files
                              </ButtonText>
                            }
                          />
                        )}
                      </SummaryList>
                    </div>
                  </AccordionSection>
                ))}
              </Accordion>
            ) : (
              <p className="govuk-inset-text">
                No data files have been uploaded.
              </p>
            )}
          </LoadingSpinner>

          {deleteDataFile && (
            <ModalConfirm
              mounted
              title="Confirm deletion of selected data files"
              onExit={() => setDeleteDataFile(undefined)}
              onCancel={() => setDeleteDataFile(undefined)}
              onConfirm={async () => {
                const { file } = deleteDataFile;

                setDeleteDataFile(undefined);
                setFileDeleting(deleteDataFile, true);

                try {
                  await releaseDataFileService.deleteDataFiles(releaseId, file);

                  setDataFiles({
                    value: dataFiles.filter(dataFile => dataFile !== file),
                  });
                } catch (err) {
                  logger.error(err);
                  setFileDeleting(deleteDataFile, false);
                }
              }}
            >
              <p>
                This data will no longer be available for use in this release.
              </p>

              {deleteDataFile.plan.deleteDataBlockPlan.dependentDataBlocks
                .length > 0 && (
                <>
                  <p>The following data blocks will also be deleted:</p>

                  <ul>
                    {deleteDataFile.plan.deleteDataBlockPlan.dependentDataBlocks.map(
                      block => (
                        <li key={block.name}>
                          <p>{block.name}</p>
                          {block.contentSectionHeading && (
                            <p>
                              {`It will also be removed from the "${block.contentSectionHeading}" content section.`}
                            </p>
                          )}
                          {block.infographicFilesInfo.length > 0 && (
                            <p>
                              The following infographic files will also be
                              removed:
                              <ul>
                                {block.infographicFilesInfo.map(fileInfo => (
                                  <li key={fileInfo.filename}>
                                    <p>{fileInfo.filename}</p>
                                  </li>
                                ))}
                              </ul>
                            </p>
                          )}
                        </li>
                      ),
                    )}
                  </ul>
                </>
              )}
              {deleteDataFile.plan.footnoteIds.length > 0 && (
                <p>
                  {`${deleteDataFile.plan.footnoteIds.length} ${
                    deleteDataFile.plan.footnoteIds.length > 1
                      ? 'footnotes'
                      : 'footnote'
                  } will be removed or updated.`}
                </p>
              )}
            </ModalConfirm>
          )}
        </>
      )}
    </Formik>
  );
};

export default ReleaseDataUploadsSection;
